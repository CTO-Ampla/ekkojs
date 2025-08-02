# Package Module Resolution Strategy

## Overview

When modules are loaded from package assemblies, we need to track their context so that relative imports are resolved correctly within the same package.

## Module Context System

### 1. Module Context Tracking

```csharp
public class ModuleContext
{
    public string ModuleId { get; set; }           // Full module identifier
    public string? PackageName { get; set; }       // Package it belongs to
    public string? PackageVersion { get; set; }    // Package version
    public IPackageFileSystem? PackageFs { get; set; } // Virtual file system
    public string ModulePath { get; set; }         // Path within package
    public string BaseDirectory { get; set; }      // For relative resolution
}

public class ModuleContextManager
{
    // Track which module is currently executing
    private readonly AsyncLocal<ModuleContext?> _currentContext = new();
    private readonly Dictionary<string, ModuleContext> _moduleContexts = new();
    
    public ModuleContext? CurrentContext 
    {
        get => _currentContext.Value;
        set => _currentContext.Value = value;
    }
    
    public void RegisterModuleContext(string moduleId, ModuleContext context)
    {
        _moduleContexts[moduleId] = context;
    }
    
    public ModuleContext? GetModuleContext(string moduleId)
    {
        return _moduleContexts.TryGetValue(moduleId, out var context) ? context : null;
    }
}
```

### 2. Enhanced Module Loader

```csharp
public class PackageAwareModuleLoader : ESModuleLoader
{
    private readonly ModuleContextManager _contextManager;
    
    public override async Task<object> ImportModule(string specifier, string? referrer = null)
    {
        // Resolve the specifier based on current context
        var resolvedSpecifier = ResolveModuleSpecifier(specifier, referrer);
        
        // Load the module
        return await LoadModuleWithContext(resolvedSpecifier);
    }
    
    private string ResolveModuleSpecifier(string specifier, string? referrer)
    {
        // Case 1: Protocol-based import (absolute)
        if (specifier.Contains(':'))
        {
            return specifier; // No change needed
        }
        
        // Case 2: Relative import (./xxx, ../xxx)
        if (specifier.StartsWith("./") || specifier.StartsWith("../"))
        {
            // Get context of the importing module
            var context = GetImporterContext(referrer);
            
            if (context?.PackageName != null)
            {
                // Resolve within the same package
                var resolvedPath = ResolveRelativePath(context.BaseDirectory, specifier);
                return $"package-internal:{context.PackageName}@{context.PackageVersion}:{resolvedPath}";
            }
            else
            {
                // Regular file system resolution
                return ResolveFileSystemPath(referrer, specifier);
            }
        }
        
        // Case 3: Bare import (might be a package)
        return $"package:{specifier}"; // Try as package first
    }
    
    private ModuleContext? GetImporterContext(string? referrer)
    {
        if (referrer == null) return null;
        
        // Check if referrer is from a package
        if (referrer.StartsWith("package:") || referrer.StartsWith("package-internal:"))
        {
            return _contextManager.GetModuleContext(referrer);
        }
        
        return null;
    }
}
```

### 3. Package Internal Module Loading

```csharp
public class PackageInternalModuleLoader
{
    public async Task<object> LoadInternalModule(string specifier)
    {
        // Parse: package-internal:@ekko/http@1.0.0:dist/utils.js
        var parts = specifier.Split(':');
        var packageInfo = parts[1].Split('@');
        var packageName = packageInfo[0] + "@" + packageInfo[1];
        var version = packageInfo[2];
        var modulePath = parts[2];
        
        // Get the package
        var package = GetLoadedPackage(packageName, version);
        if (package == null)
            throw new ModuleNotFoundException($"Package not loaded: {packageName}");
        
        // Load from package file system
        var fs = package.GetFileSystem();
        var moduleCode = fs.ReadTextFile(modulePath);
        
        // Create context for this module
        var moduleContext = new ModuleContext
        {
            ModuleId = specifier,
            PackageName = packageName,
            PackageVersion = version,
            PackageFs = fs,
            ModulePath = modulePath,
            BaseDirectory = Path.GetDirectoryName(modulePath) ?? ""
        };
        
        // Execute with context
        return await ExecuteModuleWithContext(moduleCode, moduleContext);
    }
    
    private async Task<object> ExecuteModuleWithContext(string code, ModuleContext context)
    {
        // Set context before execution
        _contextManager.CurrentContext = context;
        
        try
        {
            // Transform imports in the code
            var transformedCode = TransformImports(code, context);
            
            // Execute in V8
            var module = await CompileAndExecute(transformedCode, context.ModuleId);
            
            // Register the module context
            _contextManager.RegisterModuleContext(context.ModuleId, context);
            
            return module;
        }
        finally
        {
            // Clear context after execution
            _contextManager.CurrentContext = null;
        }
    }
}
```

### 4. Import Transformation

When loading JavaScript from a package, we need to transform the imports:

```csharp
public class ImportTransformer
{
    public string TransformImports(string code, ModuleContext context)
    {
        // Use regex or parser to find all import statements
        var importPattern = @"import\s+(.+?)\s+from\s+['""](.+?)['""];?";
        
        return Regex.Replace(code, importPattern, match =>
        {
            var importClause = match.Groups[1].Value;
            var specifier = match.Groups[2].Value;
            
            // Transform relative imports
            if (specifier.StartsWith("./") || specifier.StartsWith("../"))
            {
                var resolvedPath = ResolveRelativePath(context.BaseDirectory, specifier);
                var internalSpecifier = $"package-internal:{context.PackageName}@{context.PackageVersion}:{resolvedPath}";
                return $"import {importClause} from '{internalSpecifier}';";
            }
            
            // Leave other imports unchanged
            return match.Value;
        });
    }
}
```

### 5. Module Resolution Examples

```javascript
// File: @ekko/http-client package, dist/index.js
import { parseUrl } from './utils.js';
import logger from 'package:@ekko/logger';

// After transformation:
import { parseUrl } from 'package-internal:@ekko/http-client@1.0.0:dist/utils.js';
import logger from 'package:@ekko/logger';
```

### 6. V8 Import Hook Integration

```csharp
public class V8ImportResolver
{
    public void SetupImportHooks(V8ScriptEngine engine)
    {
        engine.Script.import = new Func<string, object>((specifier) =>
        {
            // Get current executing context
            var currentContext = _contextManager.CurrentContext;
            
            // Resolve based on context
            if (currentContext?.PackageName != null && IsRelativeImport(specifier))
            {
                // Import from same package
                var resolvedPath = ResolveWithinPackage(currentContext, specifier);
                return LoadFromPackage(currentContext.PackageName, resolvedPath);
            }
            else
            {
                // Regular import
                return DefaultImport(specifier);
            }
        });
    }
}
```

### 7. Circular Dependency Handling

```csharp
public class CircularDependencyDetector
{
    private readonly Stack<string> _loadingStack = new();
    
    public bool CheckCircularDependency(string moduleId)
    {
        return _loadingStack.Contains(moduleId);
    }
    
    public IDisposable BeginLoad(string moduleId)
    {
        _loadingStack.Push(moduleId);
        return new LoadScope(() => _loadingStack.Pop());
    }
    
    private class LoadScope : IDisposable
    {
        private readonly Action _onDispose;
        public LoadScope(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose();
    }
}
```

## Alternative Approach: Module URL Scheme

Instead of transforming imports, we could use a custom URL scheme:

```javascript
// Original code in package
import { parseUrl } from './utils.js';

// Runtime sees it as:
import { parseUrl } from 'ekko-package://self/utils.js';

// Where 'self' is resolved to current package context
```

```csharp
public class ModuleUrlResolver
{
    public string ResolveModuleUrl(string specifier, ModuleContext context)
    {
        if (specifier.StartsWith("ekko-package://self/"))
        {
            var path = specifier.Substring("ekko-package://self/".Length);
            return $"ekko-package://{context.PackageName}@{context.PackageVersion}/{path}";
        }
        
        return specifier;
    }
}
```

## Benefits of This Approach

1. **Transparent to Package Authors**: They write normal relative imports
2. **Isolated Execution**: Each package's modules are properly scoped
3. **Efficient**: No file system access needed for package-internal imports
4. **Secure**: Packages can't accidentally access files outside their boundary
5. **Debuggable**: Module IDs clearly show their origin

## Usage Example

```javascript
// User code
import http from 'package:@ekko/http-client';

// Inside @ekko/http-client/dist/index.js
import { RequestBuilder } from './request.js';  // Automatically resolved within package
import { encode } from './utils/encoder.js';    // Also within package

// The module loader knows these are package-internal because:
// 1. The importing module is from a package
// 2. The imports are relative
// 3. Context is tracked throughout execution
```