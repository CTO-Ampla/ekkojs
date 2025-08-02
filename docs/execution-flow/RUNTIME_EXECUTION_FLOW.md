# EkkoJS Runtime Execution Flow

## Scenario: `ekko run ./index.js`

### index.js contents:
```javascript
import * as fs from "ekko:fs";
console.log(fs.readFileSync("./index.js"));
```

## Step-by-Step Execution Flow

### 1. CLI Entry Point
```
User executes: ekko run ./index.js
↓
```

### 2. .NET Host Initialization
```csharp
// EkkoJS.exe main entry
static async Task Main(string[] args)
{
    var runtime = new EkkoRuntime();
    await runtime.Initialize();
    await runtime.ExecuteFile(args[1]); // ./index.js
}
```

### 3. Runtime Bootstrap
```csharp
public async Task Initialize()
{
    // Step 3.1: Create V8 engine instance
    _engine = new V8ScriptEngine(
        V8ScriptEngineFlags.EnableTaskPromiseConversion |
        V8ScriptEngineFlags.EnableModuleSupport
    );
    
    // Step 3.2: Set up module resolver
    _moduleResolver = new ModuleResolver();
    _moduleResolver.RegisterProtocol("ekko", new EkkoProtocolResolver());
    
    // Step 3.3: Install import hooks
    _engine.Execute(@"
        globalThis.originalImport = import;
        globalThis.import = function(specifier) {
            return host.resolveAndLoadModule(specifier);
        };
    ");
    
    // Step 3.4: Set up global objects
    _engine.AddHostObject("host", new HostBridge(this));
    _engine.AddHostType("Console", typeof(Console));
    
    // Step 3.5: Initialize built-in modules
    await InitializeBuiltinModules();
}
```

### 4. File Loading
```csharp
public async Task ExecuteFile(string filePath)
{
    // Step 4.1: Resolve absolute path
    var absolutePath = Path.GetFullPath(filePath);
    
    // Step 4.2: Read file contents
    var sourceCode = await File.ReadAllTextAsync(absolutePath);
    
    // Step 4.3: Detect module type (ESM due to import statement)
    var moduleType = DetectModuleType(sourceCode); // Returns ModuleType.ESM
    
    // Step 4.4: Create module context
    var context = new ModuleContext
    {
        Url = new Uri(absolutePath).ToString(),
        Type = moduleType,
        Referrer = null
    };
    
    // Step 4.5: Parse and execute as module
    await ExecuteModule(sourceCode, context);
}
```

### 5. Module Parsing
```csharp
private async Task ExecuteModule(string sourceCode, ModuleContext context)
{
    // Step 5.1: V8 parses the module
    // Encounters: import * as fs from "ekko:fs"
    
    // Step 5.2: V8 calls our import hook
    // Our hook intercepts "ekko:fs"
}
```

### 6. Import Resolution - "ekko:fs"
```csharp
// In HostBridge.resolveAndLoadModule
public async Task<object> ResolveAndLoadModule(string specifier)
{
    // Step 6.1: Parse protocol
    var protocolIndex = specifier.IndexOf(':');
    var protocol = specifier.Substring(0, protocolIndex); // "ekko"
    var moduleName = specifier.Substring(protocolIndex + 1); // "fs"
    
    // Step 6.2: Get protocol resolver
    var resolver = _moduleResolver.GetResolver(protocol); // EkkoProtocolResolver
    
    // Step 6.3: Resolve module
    var moduleInfo = await resolver.ResolveAsync(specifier, _currentContext);
}
```

### 7. Built-in Module Loading
```csharp
// In EkkoProtocolResolver
public async Task<Module> ResolveAsync(string specifier, ModuleContext context)
{
    // Step 7.1: Extract module name
    var moduleName = specifier.Replace("ekko:", ""); // "fs"
    
    // Step 7.2: Check if module exists
    if (!_builtinModules.ContainsKey(moduleName))
        throw new ModuleNotFoundError($"Unknown ekko module: {moduleName}");
    
    // Step 7.3: Check module cache
    var cacheKey = $"ekko:{moduleName}@{Runtime.Version}";
    if (_moduleCache.TryGet(cacheKey, out var cached))
        return cached;
    
    // Step 7.4: Create module instance
    var module = CreateFileSystemModule();
    
    // Step 7.5: Cache and return
    _moduleCache.Set(cacheKey, module);
    return module;
}
```

### 8. FileSystem Module Creation
```csharp
private Module CreateFileSystemModule()
{
    // Step 8.1: Create module object
    var module = new Module("ekko:fs");
    
    // Step 8.2: Add all fs methods
    module.AddExport("readFileSync", new Func<string, string>((path) => 
    {
        // Validate permissions
        CheckFilePermission(path, FileAccess.Read);
        
        // Resolve path relative to current working directory
        var fullPath = ResolvePath(path);
        
        // Read file
        return File.ReadAllText(fullPath);
    }));
    
    module.AddExport("readFile", new Func<string, Task<string>>(...));
    module.AddExport("writeFileSync", new Action<string, string>(...));
    // ... more methods
    
    // Step 8.3: Create namespace object for * import
    var namespaceObject = new ExpandoObject();
    foreach (var export in module.Exports)
    {
        ((IDictionary<string, object>)namespaceObject)[export.Key] = export.Value;
    }
    
    module.NamespaceObject = namespaceObject;
    return module;
}
```

### 9. Module Injection into V8
```csharp
// Back in the import handler
public async Task<object> ResolveAndLoadModule(string specifier)
{
    // ... previous steps
    
    // Step 9.1: Convert module to V8 object
    var v8Module = ConvertToV8Module(module);
    
    // Step 9.2: Register in V8's module map
    _engine.Script.moduleMap.set(specifier, v8Module);
    
    // Step 9.3: Return namespace object for * import
    return module.NamespaceObject;
}
```

### 10. Execution Continues
```javascript
// V8 now has 'fs' bound to the namespace object
// Execution continues to the next line:
console.log(fs.readFileSync("./index.js"));
```

### 11. readFileSync Execution
```csharp
// When fs.readFileSync is called:

// Step 11.1: V8 invokes our .NET delegate
Func<string, string> readFileSync = (path) => 
{
    // Step 11.2: Permission check
    if (!_permissions.CanReadFile(path))
        throw new PermissionDeniedError($"No read permission for: {path}");
    
    // Step 11.3: Path resolution
    var fullPath = Path.GetFullPath(path); // Resolves to current dir
    
    // Step 11.4: Read file
    var content = File.ReadAllText(fullPath);
    
    // Step 11.5: Return to JavaScript
    return content;
};
```

### 12. Console Output
```csharp
// console.log execution
// Step 12.1: V8 calls the bound Console.WriteLine
// Step 12.2: The file contents are printed to stdout
```

### 13. Execution Complete
```
Output:
import * as fs from "ekko:fs";
console.log(fs.readFileSync("./index.js"));
```

## Performance Optimizations in this Flow

1. **Module Caching**: "ekko:fs" is cached after first load
2. **Compiled Code Cache**: V8 bytecode is cached for the file
3. **Permission Caching**: File permissions are cached
4. **Path Resolution Cache**: Resolved paths are cached

## Error Handling Points

1. **File not found**: Clear error with suggestion
2. **Permission denied**: Shows required permission
3. **Module not found**: Lists available ekko modules
4. **Syntax errors**: V8 provides detailed error with line numbers

## Memory Management

1. **Module lifecycle**: Modules stay in memory for app lifetime
2. **File handles**: Automatically closed after read
3. **String interning**: Repeated strings are interned
4. **GC coordination**: V8 and .NET GC are coordinated