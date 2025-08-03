# EkkoJS Architecture

## Overview

EkkoJS is a modern JavaScript runtime built on .NET 9 and V8, designed with a 100% ES module-only approach. All code execution happens through ClearScript's native ES module support with the document loader pattern.

## Architecture Diagram

```mermaid
graph TB
    subgraph "Entry Points"
        CLI["EkkoJS.CLI<br/>Command Line Interface"]
        PKG_CLI["EkkoJS.Package.CLI<br/>Package Build Tool"]
        PKG_GUI["EkkoJS.Package.GUI<br/>GUI Components"]
    end

    subgraph "Core Runtime (EkkoJS.Core)"
        subgraph "JavaScript Engine"
            V8["V8ScriptEngine<br/>(ClearScript)"]
            Runtime["EkkoRuntime<br/>Main Runtime Orchestrator"]
            EventLoop["EventLoop<br/>Async Event Processing"]
            TimerManager["TimerManager<br/>setTimeout/setInterval"]
        end

        subgraph "Module System (100% ES Modules)"
            DocLoader["ModuleDocumentLoader<br/>ClearScript Document Loader<br/>(ALL ModuleCategory.Standard)"]
            
            subgraph "Built-in Modules"
                FSModule["FileSystemModule<br/>ekko:fs"]
                PathModule["PathModule<br/>ekko:path"]
            end
        end

        subgraph "TypeScript Support"
            TSCompiler["TypeScriptCompiler<br/>Compiles to ESNext"]
        end

        subgraph "Package System"
            IEkkoPackage["IEkkoPackage<br/>Package Interface"]
            PackageFS["IPackageFileSystem<br/>Virtual File System"]
            PackageManifest["PackageManifest<br/>ekko.json"]
        end

        subgraph ".NET Integration"
            DotNetLoader["DotNetAssemblyLoader<br/>Dynamic Assembly Loading"]
            DotNetWrapper["DotNetTypeWrapper<br/>JavaScript Interop"]
            AssemblyMapping["AssemblyMapping<br/>JSON Configuration"]
        end

        subgraph "Native Integration"
            NativeLoader["NativeLibraryLoader<br/>Dynamic P/Invoke"]
            NativeMapping["NativeLibraryMapping<br/>JSON Configuration"]
        end

        subgraph "IPC System"
            IpcClient["IpcClient<br/>Client Communication"]
            IpcTransport["IpcTransport<br/>Transport Layer"]
            NamedPipe["NamedPipeTransport"]
            TCP["TcpTransport"]
            Unix["UnixSocketTransport"]
            IpcMapping["IpcServiceMapping<br/>JSON Configuration"]
        end
    end

    subgraph "Package Compilation (Roslyn)"
        PackageCompiler["PackageCompiler<br/>Roslyn Integration"]
        VirtualFS["Virtual File System<br/>Embedded Resources"]
        SigningSupport["Assembly Signing<br/>Strong Name & Authenticode"]
    end

    %% Entry Point Connections
    CLI --> Runtime
    PKG_CLI --> PackageCompiler
    
    %% Runtime Core Connections
    Runtime --> V8
    Runtime --> EventLoop
    Runtime --> TimerManager
    Runtime --> DocLoader
    Runtime --> TSCompiler
    Runtime --> DotNetLoader
    Runtime --> NativeLoader
    
    %% Module Loading Flow
    DocLoader --> |"loads from"| PackageFS
    DocLoader --> |"loads from"| FileSystem[File System]
    DocLoader --> |"provides"| FSModule
    DocLoader --> |"provides"| PathModule
    
    %% Package System
    PackageCompiler --> |"generates"| IEkkoPackage
    IEkkoPackage --> PackageFS
    IEkkoPackage --> PackageManifest
    PackageCompiler --> VirtualFS
    PackageCompiler --> SigningSupport
    
    %% Protocol Handling
    DocLoader --> |"ekko:"| FSModule
    DocLoader --> |"ekko:"| PathModule
    DocLoader --> |"package:"| IEkkoPackage
    DocLoader --> |"dotnet:"| DotNetLoader
    DocLoader --> |"native:"| NativeLoader
    DocLoader --> |"ipc:"| IpcClient
    
    %% .NET Integration
    DotNetLoader --> DotNetWrapper
    DotNetLoader --> AssemblyMapping
    
    %% Native Integration
    NativeLoader --> NativeMapping
    
    %% IPC System
    IpcClient --> IpcTransport
    IpcTransport --> NamedPipe
    IpcTransport --> TCP
    IpcTransport --> Unix
    IpcClient --> IpcMapping
    
    %% TypeScript Flow
    TSCompiler --> |"compiles to ES modules"| V8
    
    %% All Execution Through V8
    V8 --> |"executes all as ES modules"| DocLoader

    style Runtime fill:#f9f,stroke:#333,stroke-width:4px
    style DocLoader fill:#9f9,stroke:#333,stroke-width:4px
    style V8 fill:#99f,stroke:#333,stroke-width:4px
```

![ARCHITECTURE Diagram 1](diagrams/ARCHITECTURE_diagram_1.png)

## Key Architectural Decisions

### 1. 100% ES Module System
- **ALL** code is executed as ES modules (`ModuleCategory.Standard`)
- No CommonJS support, no script mode
- ClearScript's document loader handles all module loading
- Static imports only (dynamic import not currently supported)

### 2. Protocol-Based Module System
- `ekko:` - Built-in modules (fs, path)
- `package:` - Compiled package assemblies
- `dotnet:` - .NET assembly integration
- `native:` - Native library integration
- `ipc:` - Inter-process communication

### 3. Package Compilation
- JavaScript/TypeScript packages compile to .NET assemblies
- Embedded virtual file system using Base64 encoding
- Self-contained distribution model
- Support for assembly signing (strong name & Authenticode)

### 4. Language Support
- JavaScript: Direct execution as ES modules
- TypeScript: Compiled to ESNext modules, then executed
- All files treated as modules regardless of extension

### 5. Integration Layers
- **.NET Integration**: Dynamic assembly loading with JSON mapping
- **Native Integration**: Dynamic P/Invoke with JSON mapping
- **IPC Integration**: Multi-transport (Named Pipes, TCP, Unix Sockets)

### 6. Core Components

#### EkkoRuntime
- Main orchestrator for the JavaScript runtime
- Initializes V8 engine with ClearScript
- Manages event loop and timers
- Configures document loader for ES modules

#### ModuleDocumentLoader
- ClearScript DocumentLoader implementation
- Handles all protocol-based imports
- Returns all content as ES modules
- Loads from:
  - File system
  - Package assemblies (embedded resources)
  - Built-in modules

#### PackageCompiler
- Uses Roslyn for dynamic compilation
- Generates C# code implementing IEkkoPackage
- Embeds JavaScript files as Base64 resources
- Creates self-contained .NET assemblies

## Data Flow

```mermaid
sequenceDiagram
    participant JS as JavaScript Code
    participant Runtime as EkkoRuntime
    participant DocLoader as DocumentLoader
    participant V8 as V8 Engine
    participant Module as Module Source

    JS->>Runtime: import "package:@demo/hello"
    Runtime->>V8: Execute as ES Module
    V8->>DocLoader: LoadDocumentAsync("package:@demo/hello")
    DocLoader->>Module: Load from Assembly/FileSystem
    Module-->>DocLoader: Return JavaScript text
    DocLoader-->>V8: Document with ModuleCategory.Standard
    V8-->>JS: Module exports
```

![ARCHITECTURE Diagram 2](diagrams/ARCHITECTURE_diagram_2.png)

## Module Resolution Flow (Target Architecture)

```mermaid
flowchart TB
    Import["import statement"] --> V8["V8 Engine"]
    V8 --> DocLoader["Document Loader<br/>(Single entry point for ALL imports)"]
    
    DocLoader --> Protocol{"Check Protocol"}
    
    %% All protocols handled in Document Loader
    Protocol -->|"ekko:"| BuiltIn["GetBuiltInModule()<br/>Generate JS exports"]
    Protocol -->|"package:"| Package["LoadPackageModule()<br/>Read JS from assembly"]
    Protocol -->|"file path"| FileSystem["File.ReadAllTextAsync()<br/>Read JS from disk"]
    Protocol -->|"dotnet:"| DotNet["GetDotNetModule()<br/>Generate proxy JS"]
    Protocol -->|"native:"| Native["GetNativeModule()<br/>Generate wrapper JS"]
    Protocol -->|"ipc:"| Ipc["GetIpcModule()<br/>Generate client JS"]
    
    %% All generate ES module JavaScript text
    BuiltIn --> ModuleGen["Generated ES Module JavaScript"]
    Package --> SourceCode["Source ES Module JavaScript"]
    FileSystem --> SourceCode
    DotNet --> ModuleGen
    Native --> ModuleGen
    Ipc --> ModuleGen
    
    SourceCode --> ReturnDoc["StringDocument<br/>ModuleCategory.Standard"]
    ModuleGen --> ReturnDoc
    
    ReturnDoc --> V8Execute["V8 Executes ES Module"]
    
    style DocLoader fill:#9f9,stroke:#333,stroke-width:4px
    style V8 fill:#99f,stroke:#333,stroke-width:2px
```

![ARCHITECTURE Diagram 3](diagrams/ARCHITECTURE_diagram_3.png)

### Correct Architecture - How Protocols Should Work

```mermaid
flowchart LR
    subgraph "Document Loader Integration"
        DocLoader["ModuleDocumentLoader"]
        
        subgraph "Protocol Handlers"
            EkkoHandler["ekko: → Built-in modules"]
            PackageHandler["package: → Package assemblies"]
            FileHandler["file path → File system"]
            DotNetHandler["dotnet: → DotNetAssemblyLoader"]
            NativeHandler["native: → NativeLibraryLoader"]
            IpcHandler["ipc: → IpcModule"]
        end
        
        DocLoader --> EkkoHandler
        DocLoader --> PackageHandler
        DocLoader --> FileHandler
        DocLoader --> DotNetHandler
        DocLoader --> NativeHandler
        DocLoader --> IpcHandler
    end
    
    subgraph "What Each Handler Returns"
        EkkoReturn["JavaScript module code<br/>(export const fs = {...})"]
        PackageReturn["JavaScript source from assembly"]
        FileReturn["JavaScript source from disk"]
        DotNetReturn["Generated JS code:<br/>const Type = createProxy(...)<br/>export default Type"]
        NativeReturn["Generated JS code:<br/>export function add(a,b) {...}"]
        IpcReturn["Generated JS code:<br/>const client = createIpcClient(...)<br/>export default client"]
    end
    
    EkkoHandler --> EkkoReturn
    PackageHandler --> PackageReturn
    FileHandler --> FileReturn
    DotNetHandler --> DotNetReturn
    NativeHandler --> NativeReturn
    IpcHandler --> IpcReturn
    
    AllReturns["ALL return ES module JavaScript text<br/>as StringDocument with ModuleCategory.Standard"]
    
    EkkoReturn --> AllReturns
    PackageReturn --> AllReturns
    FileReturn --> AllReturns
    DotNetReturn --> AllReturns
    NativeReturn --> AllReturns
    IpcReturn --> AllReturns
```

![ARCHITECTURE Diagram 4](diagrams/ARCHITECTURE_diagram_4.png)

### Protocol Implementation Details

```mermaid
flowchart LR
    subgraph "dotnet: Protocol (via ESModuleLoader)"
        DotNetImport["import Calculator from 'dotnet:TestLibrary'"] 
        DotNetLoader["DotNetAssemblyLoader"]
        LoadAssembly["Assembly.LoadFrom()<br/>or Assembly.Load()"]
        FindType["Find exported type"]
        DotNetWrapper["new DotNetTypeWrapper(type)"]
        JSProxy["JavaScript Proxy Object<br/>with methods/properties"]
        
        DotNetImport --> DotNetLoader
        DotNetLoader --> LoadAssembly
        LoadAssembly --> FindType
        FindType --> DotNetWrapper
        DotNetWrapper --> JSProxy
    end
    
    subgraph "native: Protocol (via ESModuleLoader)"
        NativeImport["import math from 'native:mathlib'"]
        NativeLoader["NativeLibraryLoader"]
        LoadLib["NativeLibrary.Load()"]
        ReadMapping["Read JSON mapping"]
        PInvoke["Generate P/Invoke delegates"]
        JSFunctions["JavaScript function wrappers"]
        
        NativeImport --> NativeLoader
        NativeLoader --> LoadLib
        NativeLoader --> ReadMapping
        ReadMapping --> PInvoke
        PInvoke --> JSFunctions
    end
    
    subgraph "ipc: Protocol (via ESModuleLoader)"
        IpcImport["import service from 'ipc:myservice'"]
        IpcModule["IpcModule"]
        ReadConfig["Read .ekko.ipc.json"]
        CreateTransport["Create Transport<br/>(NamedPipe/TCP/Unix)"]
        IpcClient["IpcClient with<br/>call/subscribe/publish"]
        
        IpcImport --> IpcModule
        IpcModule --> ReadConfig
        ReadConfig --> CreateTransport
        CreateTransport --> IpcClient
    end
```

![ARCHITECTURE Diagram 5](diagrams/ARCHITECTURE_diagram_5.png)

## Package Build Process

```mermaid
flowchart TB
    Source["JavaScript/TypeScript<br/>Package Source"] --> Compiler["PackageCompiler"]
    Manifest["ekko.json"] --> Compiler
    
    Compiler --> Analyze["Analyze Package Structure"]
    Analyze --> Generate["Generate C# Code"]
    
    Generate --> Class["Package Class<br/>implements IEkkoPackage"]
    Generate --> VFS["Virtual File System<br/>(Base64 embedded)"]
    
    Class --> Roslyn["Roslyn Compilation"]
    VFS --> Roslyn
    
    Roslyn --> Assembly["Signed .NET Assembly<br/>(.dll)"]
    
    Assembly --> Distribution["Self-contained Package<br/>Ready for Distribution"]
```

![ARCHITECTURE Diagram 6](diagrams/ARCHITECTURE_diagram_6.png)

## Package Assembly Structure

### Package as .NET Assembly Architecture

```mermaid
classDiagram
    class IEkkoPackage {
        <<interface>>
        +string Name
        +string Version
        +PackageManifest GetManifest()
        +IPackageFileSystem GetFileSystem()
    }

    class PackageManifest {
        +string Name
        +string Version
        +string Description
        +string Author
        +string License
        +string Main
        +List~string~ Keywords
        +Dictionary~string,string~ Dependencies
    }

    class IPackageFileSystem {
        <<interface>>
        +bool FileExists(string path)
        +string ReadFile(string path)
        +byte[] ReadFileBytes(string path)
        +string[] GetFiles(string pattern)
        +Dictionary~string,string~ GetAllFiles()
    }

    class GeneratedPackageClass {
        <<generated by Roslyn>>
        -static Dictionary~string,string~ _files
        -PackageManifest _manifest
        +string Name
        +string Version
        +PackageManifest GetManifest()
        +IPackageFileSystem GetFileSystem()
    }

    class VirtualFileSystem {
        -Dictionary~string,string~ _files
        +bool FileExists(string path)
        +string ReadFile(string path)
        +byte[] ReadFileBytes(string path)
        +string[] GetFiles(string pattern)
        +Dictionary~string,string~ GetAllFiles()
    }

    class CompiledAssembly {
        <<.NET Assembly (.dll)>>
        +Embedded Resources
        +Package Class
        +Manifest Data
        +Base64 File Content
    }

    IEkkoPackage <|.. GeneratedPackageClass : implements
    IPackageFileSystem <|.. VirtualFileSystem : implements
    GeneratedPackageClass --> PackageManifest : contains
    GeneratedPackageClass --> VirtualFileSystem : creates
    GeneratedPackageClass ..> CompiledAssembly : compiled into
```

![ARCHITECTURE Diagram 7](diagrams/ARCHITECTURE_diagram_7.png)

### Package Loading and File Access Flow

```mermaid
flowchart TB
    subgraph "Package Source Structure"
        PkgFolder["📁 demo-package/"]
        EkkoFolder["📁 .ekko/"]
        EkkoJson["📄 ekko.json"]
        SrcFolder["📁 src/"]
        IndexJS["📄 index.js"]
        OtherFiles["📄 other files..."]
        
        PkgFolder --> EkkoFolder
        EkkoFolder --> EkkoJson
        PkgFolder --> SrcFolder
        SrcFolder --> IndexJS
        SrcFolder --> OtherFiles
    end

    subgraph "Compilation Process"
        Compiler["PackageCompiler"]
        FileDict["Dictionary<string, string><br/>path → Base64 content"]
        CodeGen["C# Code Generation"]
        
        EkkoJson --> |"Read manifest"| Compiler
        SrcFolder --> |"Scan all files"| Compiler
        Compiler --> |"Base64 encode"| FileDict
        FileDict --> CodeGen
    end

    subgraph "Generated C# Code Structure"
        GenClass["public class Package : IEkkoPackage"]
        StaticFiles["static Dictionary<string, string> _files = new()<br/>{<br/>  ['src/index.js'] = 'base64...',<br/>  ['src/utils.js'] = 'base64...'<br/>}"]
        GetFS["public IPackageFileSystem GetFileSystem()<br/>{<br/>  return new VirtualFileSystem(_files);<br/>}"]
        
        CodeGen --> GenClass
        GenClass --> StaticFiles
        GenClass --> GetFS
    end

    subgraph "Runtime File Access"
        Import["import from 'package:@demo/hello'"]
        DocLoader["ModuleDocumentLoader"]
        LoadAsm["Assembly.LoadFrom('demo.hello.dll')"]
        CreateInst["Activator.CreateInstance(packageType)"]
        GetMain["package.GetFileSystem().ReadFile(manifest.Main)"]
        ReturnJS["Return JavaScript text"]
        
        Import --> DocLoader
        DocLoader --> LoadAsm
        LoadAsm --> CreateInst
        CreateInst --> GetMain
        GetMain --> ReturnJS
    end
```

![ARCHITECTURE Diagram 8](diagrams/ARCHITECTURE_diagram_8.png)

### Detailed File Access API

```mermaid
sequenceDiagram
    participant JS as JavaScript
    participant DL as DocumentLoader
    participant Asm as Assembly
    participant Pkg as Package Instance
    participant VFS as VirtualFileSystem
    participant Files as Embedded Files

    JS->>DL: import "package:@demo/hello"
    DL->>DL: Extract package name: "@demo/hello"
    DL->>DL: Convert to assembly name: "demo.hello.dll"
    DL->>Asm: Assembly.LoadFrom("demo.hello.dll")
    Asm-->>DL: Loaded Assembly
    DL->>Asm: Find type implementing IEkkoPackage
    Asm-->>DL: typeof(Package)
    DL->>Pkg: Activator.CreateInstance()
    Pkg-->>DL: Package instance
    
    DL->>Pkg: GetManifest()
    Pkg-->>DL: PackageManifest { Main = "src/index.js" }
    
    DL->>Pkg: GetFileSystem()
    Pkg->>VFS: new VirtualFileSystem(_files)
    VFS-->>Pkg: VirtualFileSystem instance
    Pkg-->>DL: IPackageFileSystem
    
    DL->>VFS: ReadFile("src/index.js")
    VFS->>Files: _files["src/index.js"]
    Files-->>VFS: "base64encodedcontent..."
    VFS->>VFS: Convert.FromBase64String()
    VFS->>VFS: Encoding.UTF8.GetString()
    VFS-->>DL: "// JavaScript source code..."
    
    DL-->>JS: ES Module with JavaScript content
```

![ARCHITECTURE Diagram 9](diagrams/ARCHITECTURE_diagram_9.png)

### Virtual File System Implementation

```mermaid
classDiagram
    class VirtualFileSystem {
        -Dictionary~string,string~ _files
        --
        +VirtualFileSystem(Dictionary files)
        +bool FileExists(string path)
        +string ReadFile(string path)
        +byte[] ReadFileBytes(string path)
        +string[] GetFiles(string pattern)
        +Dictionary~string,string~ GetAllFiles()
        --
        -string NormalizePath(string path)
        -bool MatchesPattern(string path, string pattern)
    }

    class FileOperations {
        <<usage examples>>
        FileExists("src/index.js") → true
        ReadFile("src/index.js") → "export function..."
        ReadFileBytes("assets/icon.png") → byte[]
        GetFiles("src/*.js") → ["src/index.js", "src/utils.js"]
        GetAllFiles() → complete file dictionary
    }

    VirtualFileSystem --> FileOperations : provides
```

![ARCHITECTURE Diagram 10](diagrams/ARCHITECTURE_diagram_10.png)

### Example Generated Package Class

```csharp
// Auto-generated by PackageCompiler
using EkkoJS.Core.Packages;
using System.Collections.Generic;

namespace demo.hello_world
{
    public class Package : IEkkoPackage
    {
        private static readonly Dictionary<string, string> _files = new Dictionary<string, string>
        {
            ["src/index.js"] = "Ly8gRGVtbyBwYWNrYWdlIG1haW4gZW50cnkgcG9pbnQKZXhwb3J0IGZ1bmN0aW9uIGdyZWV0KG5hbWUgPSAnV29ybGQnKSB7CiAgICByZXR1cm4gYEhlbGxvLCAke25hbWV9IWA7Cn0KCmV4cG9ydCBmdW5jdGlvbiBhZGQoYSwgYikgewogICAgcmV0dXJuIGEgKyBiOwp9CgpleHBvcnQgY29uc3QgdmVyc2lvbiA9ICcxLjAuMCc7CgpleHBvcnQgZGVmYXVsdCB7CiAgICBncmVldCwKICAgIGFkZCwKICAgIHZlcnNpb24KfTs=",
            ["src/utils.js"] = "ZXhwb3J0IGZ1bmN0aW9uIHV0aWxpdHlGdW5jdGlvbigpIHsgLyogLi4uICovIH0=",
            ["assets/data.json"] = "eyJrZXkiOiAidmFsdWUifQ==",
            // ... more files
        };

        private readonly PackageManifest _manifest = new PackageManifest
        {
            Name = "@demo/hello-world",
            Version = "1.0.0",
            Description = "A simple demo package",
            Main = "src/index.js",
            // ... other manifest properties
        };

        public string Name => _manifest.Name;
        public string Version => _manifest.Version;

        public PackageManifest GetManifest() => _manifest;

        public IPackageFileSystem GetFileSystem()
        {
            return new VirtualFileSystem(_files);
        }
    }
}
```

## Implementation Plan for Protocol Integration

### Current State vs Target State

| Protocol | Current Implementation | Target Implementation |
|----------|----------------------|---------------------|
| `ekko:` | ✅ In DocumentLoader | ✅ Already complete |
| `package:` | ✅ In DocumentLoader | ✅ Already complete |
| `file path` | ✅ In DocumentLoader | ✅ Already complete |
| `dotnet:` | ✅ In DocumentLoader | ✅ Complete - Generates ES module JS |
| `native:` | ✅ In DocumentLoader | ✅ Complete - Generates ES module JS |
| `ipc:` | ✅ In DocumentLoader | ✅ Complete - Generates ES module JS |

### Implementation Tasks

#### 1. Implement GetDotNetModule() in ModuleDocumentLoader

```csharp
private string GetDotNetModule(string moduleSpec)
{
    // Parse: "dotnet:AssemblyName" or "dotnet:AssemblyName/TypeName"
    // Use DotNetAssemblyLoader to load assembly and get type
    // Generate JavaScript code that:
    //   1. Creates a host object reference
    //   2. Wraps it with proper ES module exports
    //   3. Handles static methods, properties, constructors
    
    return @"
        const hostType = globalThis.__dotnetTypes['AssemblyName.TypeName'];
        export default hostType;
        // Export individual static methods/properties as named exports
    ";
}
```

#### 2. Implement GetNativeModule() in ModuleDocumentLoader

```csharp
private string GetNativeModule(string libraryName)
{
    // Use NativeLibraryLoader to load the library
    // Read the JSON mapping file
    // Generate JavaScript code that:
    //   1. Creates function wrappers for each native function
    //   2. Handles parameter marshaling
    //   3. Exports as ES module
    
    return @"
        const lib = globalThis.__nativeLibs['libraryName'];
        export function add(a, b) { return lib.add(a, b); }
        export function multiply(a, b) { return lib.multiply(a, b); }
        // ... more exports
    ";
}
```

#### 3. Implement GetIpcModule() in ModuleDocumentLoader

```csharp
private string GetIpcModule(string serviceName)
{
    // Read IPC service mapping
    // Generate JavaScript code that:
    //   1. Creates IPC client instance
    //   2. Wraps methods for RPC calls
    //   3. Sets up event subscriptions
    //   4. Exports as ES module
    
    return @"
        const client = globalThis.__ipcClients['serviceName'];
        export function call(method, ...args) { return client.call(method, args); }
        export function subscribe(event, handler) { return client.subscribe(event, handler); }
        export function publish(channel, data) { return client.publish(channel, data); }
        export default { call, subscribe, publish };
    ";
}
```

### Migration Complete

All protocols have been successfully migrated to the Document Loader pattern:
- ✅ All protocols now generate ES module JavaScript
- ✅ ESModuleLoader has been removed
- ✅ All module loading goes through ModuleDocumentLoader
- ✅ Everything is treated as ES modules (ModuleCategory.Standard)

## Notes

1. All protocols will be handled through the Document Loader
2. Dynamic import() is not currently supported due to ClearScript limitations with custom protocols
3. The system generates ES module JavaScript code for all protocols
4. All imports are executed as ES modules (ModuleCategory.Standard)
5. Package files are stored as Base64-encoded strings in compiled assemblies
6. The virtual file system provides a file-like API for accessing embedded resources
7. Package assemblies are completely self-contained with no external dependencies