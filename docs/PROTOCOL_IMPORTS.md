# EkkoJS Protocol-Based Import System

## Core Concept: Protocol Prefixes

Using protocol prefixes provides a clean namespace separation and makes imports explicit about their source.

```javascript
// Built-in Ekko modules
import fs, { readFile, writeFile } from "ekko:fs";
import { createServer } from "ekko:http";
import { Connection } from "ekko:db";

// .NET assemblies
import { SqlConnection } from "dotnet:System.Data.SqlClient";
import { JObject } from "dotnet:Newtonsoft.Json";

// IPC services
import { analyze } from "ipc:python-ml-service";
import { compile } from "ipc:rust-compiler";

// Node modules (if compatibility layer enabled)
import express from "node:express";

// Regular file imports (no protocol)
import { helper } from "./utils.js";
```

## Implementation Architecture

### 1. Module Resolver Design

```csharp
public interface IProtocolResolver
{
    string Protocol { get; }
    bool CanResolve(string specifier);
    Task<ModuleInfo> ResolveAsync(string specifier, ModuleContext context);
    Task<object> LoadAsync(ModuleInfo moduleInfo);
}

public class ModuleResolver
{
    private readonly Dictionary<string, IProtocolResolver> _resolvers = new()
    {
        ["ekko"] = new EkkoProtocolResolver(),
        ["dotnet"] = new DotNetProtocolResolver(),
        ["ipc"] = new IPCProtocolResolver(),
        ["node"] = new NodeCompatProtocolResolver(),
        ["npm"] = new NPMProtocolResolver(),
        ["http"] = new HttpProtocolResolver(),
        ["https"] = new HttpsProtocolResolver()
    };
    
    public async Task<Module> ResolveModule(string specifier, ModuleContext context)
    {
        var protocolIndex = specifier.IndexOf(':');
        if (protocolIndex > 0)
        {
            var protocol = specifier.Substring(0, protocolIndex);
            if (_resolvers.TryGetValue(protocol, out var resolver))
            {
                return await resolver.ResolveAsync(specifier, context);
            }
        }
        
        // Default file resolver for no protocol
        return await FileResolver.ResolveAsync(specifier, context);
    }
}
```

### 2. Ekko Protocol Implementation

```csharp
public class EkkoProtocolResolver : IProtocolResolver
{
    private readonly Dictionary<string, Func<Module>> _builtinModules = new()
    {
        ["fs"] = () => new FileSystemModule(),
        ["http"] = () => new HttpModule(),
        ["crypto"] = () => new CryptoModule(),
        ["process"] = () => new ProcessModule(),
        ["os"] = () => new OSModule(),
        ["path"] = () => new PathModule(),
        ["stream"] = () => new StreamModule(),
        ["buffer"] = () => new BufferModule(),
        ["console"] = () => new ConsoleModule(),
        ["timer"] = () => new TimerModule(),
        ["db"] = () => new DatabaseModule(),
        ["test"] = () => new TestModule(),
        ["cli"] = () => new CLIModule(),
        ["server"] = () => new ServerModule()
    };
    
    public async Task<Module> ResolveAsync(string specifier, ModuleContext context)
    {
        // Parse "ekko:fs" -> "fs"
        var moduleName = specifier.Substring(5); // Remove "ekko:"
        
        if (_builtinModules.TryGetValue(moduleName, out var factory))
        {
            return factory();
        }
        
        throw new ModuleNotFoundError($"Unknown ekko module: {moduleName}");
    }
}
```

### 3. Built-in Module Structure

```javascript
// Each built-in module exposes both default and named exports
// ekko:fs module structure
export default {
    readFile,
    writeFile,
    readdir,
    mkdir,
    rm,
    stat,
    watch,
    createReadStream,
    createWriteStream,
    // ... all fs methods
};

export {
    readFile,
    writeFile,
    readdir,
    mkdir,
    rm,
    stat,
    watch,
    createReadStream,
    createWriteStream,
    FileHandle,
    FSWatcher,
    // ... all exports
};
```

### 4. TypeScript Definitions

```typescript
// Auto-generated ekko.d.ts
declare module "ekko:fs" {
    export function readFile(path: string, encoding?: string): Promise<string>;
    export function readFile(path: string, options: ReadFileOptions): Promise<Buffer>;
    export function writeFile(path: string, data: string | Buffer, options?: WriteFileOptions): Promise<void>;
    
    export interface FileHandle {
        read(buffer: Buffer, offset: number, length: number, position: number): Promise<ReadResult>;
        close(): Promise<void>;
    }
    
    const fs: {
        readFile: typeof readFile;
        writeFile: typeof writeFile;
        // ... all methods
    };
    
    export default fs;
}

declare module "ekko:http" {
    export function createServer(handler?: RequestHandler): HttpServer;
    export class HttpServer {
        listen(port: number, hostname?: string): Promise<void>;
        close(): Promise<void>;
    }
}
```

### 5. V8 Integration

```javascript
// In V8 context initialization
globalThis.import = new Proxy(originalImport, {
    apply(target, thisArg, args) {
        const specifier = args[0];
        
        // Intercept and handle protocol imports
        if (specifier.includes(':')) {
            return handleProtocolImport(specifier);
        }
        
        // Default file import
        return Reflect.apply(target, thisArg, args);
    }
});

// Custom import.meta extensions
import.meta.resolve = function(specifier) {
    // Resolve with protocol support
};

import.meta.protocol = function(specifier) {
    // Extract protocol from specifier
    const match = specifier.match(/^([a-z]+):/);
    return match ? match[1] : null;
};
```

### 6. Protocol Registration API

```javascript
// Allow custom protocol registration
Ekko.registerProtocol('custom', {
    resolve(specifier, context) {
        // Custom resolution logic
        return {
            url: specifier,
            format: 'module'
        };
    },
    
    async load(resolved) {
        // Custom loading logic
        const source = await fetchCustomModule(resolved.url);
        return {
            source,
            format: resolved.format
        };
    }
});

// Usage
import { something } from "custom:my-module";
```

### 7. Module Caching Strategy

```javascript
// Protocol-aware module cache
class ModuleCache {
    private cache = new Map<string, Module>();
    
    getCacheKey(specifier: string, context: ModuleContext): string {
        const protocol = extractProtocol(specifier);
        
        switch(protocol) {
            case 'ekko':
                // Built-ins are version-locked
                return `${specifier}@${Ekko.version}`;
                
            case 'dotnet':
                // Include assembly version
                return `${specifier}@${getAssemblyVersion(specifier)}`;
                
            case 'ipc':
                // No caching for IPC by default
                return null;
                
            default:
                // File-based caching
                return `${specifier}@${getFileHash(specifier)}`;
        }
    }
}
```

### 8. Import Maps Support

```json
// ekko.importmap.json
{
    "imports": {
        "fs": "ekko:fs",
        "http": "ekko:http",
        "@company/": "https://cdn.company.com/",
        "lodash": "https://cdn.skypack.dev/lodash"
    },
    "scopes": {
        "/src/": {
            "fs": "ekko:fs/promises"
        }
    }
}
```

### 9. Dynamic Import Support

```javascript
// Dynamic imports with protocols
const fs = await import("ekko:fs");
const { readFile } = await import("ekko:fs");

// Conditional loading
const db = await import(`ekko:db/${dbType}`);

// Lazy loading .NET assemblies
const Excel = await import("dotnet:Microsoft.Office.Interop.Excel");
```

### 10. Error Handling

```javascript
// Clear error messages for protocol imports
try {
    import { missing } from "ekko:nonexistent";
} catch (error) {
    // ModuleNotFoundError: Cannot find module 'ekko:nonexistent'
    // Available ekko modules: fs, http, crypto, process, os, path...
}

try {
    import { Class } from "dotnet:Invalid.Assembly";
} catch (error) {
    // AssemblyNotFoundError: Cannot load .NET assembly 'Invalid.Assembly'
    // Searched locations: GAC, ./ekko_modules/dotnet/, ...
}
```

## Benefits of Protocol-Based Imports

1. **Clear Namespace Separation**: No conflicts between built-ins, files, and external modules
2. **Explicit Dependencies**: Easy to see what's built-in vs external
3. **Extensibility**: Easy to add new protocols (wasm:, gpu:, native:)
4. **Tool-Friendly**: Static analysis tools can understand dependencies
5. **Security**: Can apply different security policies per protocol
6. **Performance**: Can optimize loading strategies per protocol type

## Future Protocol Ideas

```javascript
// WebAssembly modules
import { compute } from "wasm:./math.wasm";

// GPU compute shaders  
import { kernel } from "gpu:./compute.glsl";

// Native bindings
import { fastFunction } from "native:./addon.node";

// Remote modules
import { api } from "remote:https://api.service.com/module";

// Database stored modules
import { storedProc } from "db:procedures/calculateTax";
```

This protocol-based system provides a clean, extensible way to handle all types of imports while maintaining clarity about where modules come from.