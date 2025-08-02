# EkkoJS Architecture Documentation

## Executive Summary

EkkoJS is a JavaScript/TypeScript runtime that combines the V8 JavaScript engine with the .NET ecosystem, providing seamless interoperability between JavaScript and .NET while maintaining high performance and security. The architecture is built around several core principles:

- **V8/.NET Integration**: Deep integration between V8 JavaScript engine and .NET runtime
- **Package Distribution**: Assemblies as self-contained packages with embedded resources
- **TypeScript First**: Native TypeScript support with on-demand compilation
- **Protocol-based Modules**: Extensible module system using protocol schemes
- **Cross-platform**: Unified runtime across Windows, Linux, and macOS
- **Security-focused**: Sandbox execution with granular permissions

## Core Architecture

### Runtime Bootstrap Process

The EkkoJS runtime follows a structured initialization sequence:

1. **.NET Host Initialization**: Creates the host application domain
2. **V8 Engine Creation**: Initializes V8 with specific flags for optimal performance
3. **Core Modules Registration**: Registers built-in modules (`ekko:*` protocol)
4. **TypeScript Compiler Initialization**: Sets up on-demand TypeScript compilation
5. **Global Object Setup**: Configures the global JavaScript environment
6. **Entry Point Execution**: Loads and executes the specified script or module

```csharp
public class EkkoRuntime
{
    private V8ScriptEngine _engine;
    private TypeScriptCompiler _tsCompiler;
    private ModuleLoader _moduleLoader;
    private InteropBridge _interopBridge;
    
    public async Task Initialize()
    {
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);
        _engine.MaxHeapSize = GetConfiguredHeapSize();
        
        await InitializeCoreModules();
        await InitializeTypeScriptCompiler();
        await InitializeInteropBridge();
    }
}
```

### JavaScript ↔ .NET Interop Layer

EkkoJS provides seamless type mapping between JavaScript and .NET:

- **Primitives**: Automatic conversion between JS numbers/strings and .NET types
- **Complex Types**: Arrays map to List<T>, Objects to Dictionary or custom classes
- **Async**: JavaScript Promises automatically convert to .NET Tasks
- **Memory Management**: Automatic disposal tracking for IDisposable objects

## Project Structure and Organization

### Folder Structure Conventions

EkkoJS enforces a standardized project structure centered around the `.ekko/` configuration directory:

```
project/
├── .ekko/                   # Configuration directory (mandatory)
│   ├── ekko.json           # Main configuration file
│   ├── build.json          # Build settings (optional)
│   ├── native.json         # Native library mappings (optional)
│   └── extensions.json     # Local extensions (optional)
├── src/                     # Source code (mandatory)
├── assets/                  # Static resources (optional)
├── native/                  # Native libraries (optional)
├── tests/                   # Test files (optional)
├── docs/                    # Documentation (optional)
├── README.md               # Project readme (allowed in root)
└── LICENSE                 # License file (allowed in root)
```

### Root Folder Policy

EkkoJS enforces strict root directory rules:

- **Mandatory `.ekko/` directory**: Identifies an EkkoJS project
- **Limited root files**: Only README.md, LICENSE, .gitignore, .gitattributes allowed by default
- **Validation tools**: `ekko validate` checks compliance
- **Auto-fixing**: `ekko fix-structure` automatically reorganizes non-compliant projects

### Folder Categories

EkkoJS recognizes standardized folder categories:

- **Core**: `.ekko/` (config), `src/` (source), `tests/` (testing), `assets/` (assets), `docs/` (documentation)
- **Build & Development**: `native/`, `scripts/`, `tools/`, `dist/`, `generated/`
- **Dependencies**: `packages/`, `libs/`, `vendor/`, `modules/`
- **Deployment**: `.github/`, `ci/`, `deploy/`, `docker/`, `k8s/`

## Package System

### Development Mode

During development, packages can be linked directly from source directories:

```bash
# Link a package for development
cd ~/projects/my-package
ekko link

# Use linked package in another project
cd ~/projects/my-app
ekko link @mycompany/my-package
```

**Key Features:**
- **On-demand TypeScript compilation**: No build step required
- **Live reloading**: Changes reflected immediately
- **Security boundaries**: Cannot import outside package directory
- **Same import syntax**: Use `package:` protocol in development

### Distribution Architecture

Packages are distributed as .NET assemblies with embedded resources:

```csharp
public interface IEkkoPackage
{
    string Name { get; }
    string Version { get; }
    PackageManifest GetManifest();
    IPackageFileSystem GetFileSystem();
}
```

**Benefits:**
- **Self-contained**: Everything in a single .dll file
- **Version isolation**: Multiple versions can coexist
- **Fast loading**: No file system scanning
- **Cross-platform**: Same package works everywhere
- **Secure**: Assemblies can be signed

### Module Resolution Strategy

The module resolution system handles different import types:

1. **Protocol-based imports**: `ekko:fs`, `package:@mycompany/utils`, `dotnet:MyLibrary`
2. **Relative imports**: `./utils.js`, `../helpers/index.ts`
3. **Bare imports**: Resolved as packages first, then built-ins

```javascript
// Module context tracking ensures correct resolution
import { readFile } from 'ekko:fs';           // Built-in module
import utils from 'package:@mycompany/utils'; // External package
import { helper } from './helper.js';          // Relative import within package
```

### Native Library Extraction

Native libraries are extracted from packages on-demand:

**Extraction Strategy:**
- **Lazy extraction**: Only when first accessed
- **Platform detection**: Automatic selection of correct library
- **Integrity verification**: SHA256 hash validation
- **Atomic extraction**: Temporary file approach prevents corruption
- **Caching**: User-specific cache with cleanup policies

**Extraction Locations:**
1. User cache: `~/.ekkojs/cache/packages/`
2. Temp directory: `/tmp/ekkojs/packages/`
3. Application directory: `./ekko_cache/packages/`

## Module Systems

### TypeScript Support

EkkoJS provides first-class TypeScript support:

- **Automatic detection**: `.ts` and `.tsx` files automatically compiled
- **In-memory compilation**: No separate build step required
- **ES Module support**: Full import/export syntax
- **Type definitions**: Built-in modules include TypeScript definitions

```typescript
// Direct execution without compilation
import fs from 'ekko:fs';
import { join } from 'ekko:path';

interface Config {
    name: string;
    version: number;
    settings: Record<string, any>;
}

const config: Config = {
    name: 'MyApp',
    version: 1,
    settings: { debug: true }
};

fs.writeFileSync('config.json', JSON.stringify(config, null, 2));
```

### Type Generation for JavaScript Packages

For JavaScript packages, EkkoJS provides multiple type generation strategies:

1. **JSDoc Type Inference**: Extract types from JSDoc comments
2. **Code Structure Analysis**: Infer types from code patterns
3. **Manual Type Definitions**: Provide separate `.d.ts` files
4. **Hybrid Approach**: Combine generation with manual overrides

```json
{
  "types": {
    "source": "hybrid",
    "generate": {
      "from": "src/**/*.js",
      "strategy": "jsdoc"
    },
    "overrides": ["types/overrides.d.ts"]
  }
}
```

### Native Library Loading

EkkoJS supports loading native C/C++ libraries through the `native:` protocol:

```javascript
import mathlib from 'native:mathlib';

// Call native functions with type safety
const result = mathlib.add(5, 3);
const version = mathlib.getVersion();

// Work with structs
const point = new mathlib.Point({ x: 10, y: 20 });
```

**Features:**
- Cross-platform loading (Windows DLL, Linux SO, macOS DYLIB)
- Dynamic P/Invoke wrapper generation
- Type-safe function calls with automatic marshaling
- Struct support with field mapping
- Multiple architecture support

### .NET Assembly Loading

The `dotnet:` protocol enables seamless .NET interop:

```javascript
import Calculator from 'dotnet:TestLibrary/Calculator';
import TestLib from 'dotnet:TestLibrary';

// Use static methods
const result = Calculator.add(5, 3);

// Create instances
const { StringHelper } = TestLib.default;
const helper = StringHelper.new('[PREFIX] ');
console.log(helper.format('Hello')); // "[PREFIX] Hello"
```

**Capabilities:**
- Dynamic assembly loading at runtime
- Static and instance method calls
- Property access (getters/setters)
- Async method support (returns Promises)
- Type-safe method invocation

## IPC Communication

### Architecture Overview

EkkoJS IPC enables bi-directional communication between EkkoJS applications and external processes:

**Transport Layer:**
- Named Pipes (Windows/Linux/macOS)
- Unix Domain Sockets (Linux/macOS)
- TCP Sockets (cross-platform fallback)

**Message Protocol:**
```json
{
  "messageId": "uuid-v4",
  "type": "request|response|event|subscribe|unsubscribe",
  "channel": "channel-name",
  "action": "method-name",
  "data": { ... },
  "timestamp": "ISO-8601",
  "correlationId": "uuid-v4"
}
```

### Communication Patterns

**Request/Response:**
```javascript
import service from 'ipc:database-service';
const result = await service.getUserById(123);
```

**Pub/Sub Events:**
```javascript
import events from 'ipc:notification-service';
events.subscribe('user.created', (data) => {
    console.log('New user:', data);
});
events.publish('order.completed', { orderId: 456 });
```

### Service Discovery

Services expose capabilities via `.ekko.ipc.json` files:

```json
{
  "service": {
    "name": "database-service",
    "transport": {
      "type": "namedpipe",
      "address": "ekko-db-service"
    }
  },
  "methods": {
    "getUserById": {
      "parameters": [{"name": "id", "type": "number"}],
      "returns": {"type": "object"}
    }
  },
  "events": {
    "user.created": {
      "schema": {"id": "number", "name": "string"}
    }
  }
}
```

## Build and Deployment

### Package Build Process

The build process transforms development packages into distribution assemblies:

1. **Compile TypeScript**: Convert TS to JavaScript
2. **Bundle Dependencies**: Include required dependencies
3. **Create .NET Assembly**: Generate assembly project
4. **Embed Resources**: Include all package files as embedded resources
5. **Implement IEkkoPackage**: Generate package interface implementation
6. **Compile Assembly**: Build final .dll package
7. **Sign Assembly**: Optional code signing

```bash
ekko build package
# Outputs: MyPackage.dll with all resources embedded
```

### Development Workflow

```bash
# 1. Create package
ekko create package @mycompany/http-client

# 2. Develop with TypeScript
echo 'export const get = (url: string) => fetch(url);' > src/index.ts

# 3. Link for development
ekko link

# 4. Use in another project
cd ~/my-app
ekko link @mycompany/http-client

# 5. Use in code - changes reflected immediately
import http from 'package:@mycompany/http-client';
const data = await http.get('https://api.example.com');

# 6. Build for distribution
ekko build package
ekko publish
```

## Security Model

### Permission System

EkkoJS implements a granular permission system:

```json
{
  "permissions": {
    "net": ["https://api.example.com"],
    "fs": {
      "read": ["./data"],
      "write": ["./output"]
    },
    "dotnet": ["System.Data", "MyCompany.*"],
    "ipc": ["python-service"]
  }
}
```

### Sandbox Execution

Applications can run in isolated contexts:

```javascript
const sandbox = new Ekko.Sandbox({
    memory: '100MB',
    cpu: '50%',
    timeout: 5000,
    permissions: {
        net: false,
        fs: { read: ['./safe'] }
    }
});

const result = await sandbox.run('./untrusted.js');
```

### Security Considerations

- **Assembly loading**: Assemblies loaded with controlled permissions
- **Package boundaries**: Packages cannot access files outside their directory
- **IPC authentication**: Token-based authentication for IPC services
- **Native library verification**: Integrity checks before loading
- **Input validation**: All cross-boundary data is validated

## Runtime Execution Flow

### Example: `ekko run ./index.js`

**File contents:**
```javascript
import * as fs from "ekko:fs";
console.log(fs.readFileSync("./index.js"));
```

**Execution steps:**
1. **.NET Host**: Initialize EkkoJS.exe main entry
2. **Runtime Bootstrap**: Create V8 engine, set up module resolver
3. **File Loading**: Read and detect module type (ESM)
4. **Module Parsing**: V8 parses import statement
5. **Import Resolution**: Resolve `ekko:fs` to built-in module
6. **Module Loading**: Load filesystem module with permissions
7. **Execution**: Call `readFileSync` with path validation
8. **Output**: Display file contents

## Performance Optimizations

### Startup Optimizations

- **Module caching**: Built-in modules cached after first load
- **V8 snapshots**: Pre-compiled core modules for faster startup
- **JIT optimization hints**: Profile-guided optimization
- **Memory pooling**: Built-in object pools for common operations

### Runtime Optimizations

- **Connection pooling**: Reuse connections for IPC
- **Message batching**: Reduce transport overhead
- **Response caching**: Cache frequent responses
- **Lazy loading**: Load components only when needed

### Memory Management

- **Unified event loop**: V8 microtasks, .NET task queue, timers, I/O
- **Automatic disposal**: IDisposable tracking with `using` syntax
- **WeakRef support**: For large .NET objects
- **GC coordination**: V8 and .NET garbage collectors work together

## Cross-Platform Compatibility

### Platform Support

- **Windows**: Named Pipes, Windows Services integration
- **Linux**: Unix Domain Sockets, systemd integration
- **macOS**: Unix Domain Sockets, launchd integration

### Native Library Support

- **Windows**: .dll files with dependency resolution
- **Linux**: .so files with LD_LIBRARY_PATH management  
- **macOS**: .dylib files with @rpath handling
- **Architecture**: x86, x64, ARM64 support

## Monitoring and Debugging

### Built-in Diagnostics

```javascript
// Performance profiling
const profile = await Ekko.profiler.start();
// ... code to profile
const result = await profile.stop();

// Memory diagnostics
const snapshot = Ekko.memory.takeSnapshot();
await snapshot.save('./heap.heapsnapshot');

// Leak detection
Ekko.memory.startLeakDetection({
    threshold: '100MB',
    callback: (leak) => console.error(leak)
});
```

### Logging and Metrics

- **Structured logging**: JSON format with configurable levels
- **Performance metrics**: Built-in performance counters
- **Error tracking**: Detailed error reporting with stack traces
- **Health monitoring**: Service health dashboards

## Future Extensibility

The architecture is designed for extensibility:

- **Protocol system**: Easy to add new import protocols
- **Plugin architecture**: Support for runtime extensions
- **API surface**: Stable APIs for third-party tools
- **Interop bridges**: Additional language runtime integration

This architecture provides a robust foundation for JavaScript/TypeScript applications with deep .NET integration while maintaining security, performance, and cross-platform compatibility.