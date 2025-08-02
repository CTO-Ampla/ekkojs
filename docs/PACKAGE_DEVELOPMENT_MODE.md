# Package Development Mode and Linking

## Overview

During package development, developers need to test their packages without building them into assemblies. The package linking system allows loading packages directly from their source directories.

## Package Development Structure

### 1. Development Package Layout

```
my-http-package/
├── package.ekko.json         # Package manifest
├── src/                      # Source files (JS or TS)
│   ├── index.ts             # Entry point
│   ├── utils.ts
│   └── request.ts
├── lib/                      # Optional: JavaScript modules
│   └── helpers.js
├── native/                   # Native libraries
│   ├── win-x64/
│   ├── linux-x64/
│   └── darwin-x64/
├── assets/                   # Static assets
└── tsconfig.json            # Optional: TypeScript config
```

**Note**: No `dist/` folder needed! EkkoJS compiles TypeScript on-the-fly.

### 2. Package Manifest for Development

```json
{
  "name": "@mycompany/http-client",
  "version": "0.1.0-dev",
  "description": "HTTP client for EkkoJS",
  "main": "src/index.ts",
  "type": "module",
  "exports": {
    ".": "./src/index.ts",
    "./advanced": "./src/advanced.ts",
    "./utils": "./lib/helpers.js"
  },
  "development": {
    "sourceMaps": true,
    "typeCheck": true
  },
  "dependencies": {
    "@ekko/core": "^1.0.0"
  }
}
```

## CLI Commands for Package Development

### 1. Link Command

```bash
# In package directory
cd ~/projects/my-http-package
ekko link

# What it does:
# 1. Validates package.ekko.json
# 2. Registers package in global link registry
# 3. Sets up file watchers if configured
# 4. Compiles TypeScript if needed

# Output:
✅ Package "@mycompany/http-client" linked from /home/user/projects/my-http-package
```

### 2. Link Usage

```bash
# In project using the package
cd ~/projects/my-app
ekko link @mycompany/http-client

# What it does:
# 1. Creates link in project's package registry
# 2. Package can now be imported normally

# Output:
✅ Linked @mycompany/http-client -> /home/user/projects/my-http-package
```

### 3. Unlink Command

```bash
# Remove link from project
ekko unlink @mycompany/http-client

# Remove package from global link registry
cd ~/projects/my-http-package
ekko unlink
```

## Implementation

### 1. Link Registry

```csharp
public class PackageLinkRegistry
{
    private readonly string _registryPath;
    private Dictionary<string, PackageLink> _links;
    
    public PackageLinkRegistry()
    {
        _registryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".ekkojs",
            "links.json"
        );
        LoadRegistry();
    }
    
    public void RegisterLink(string packageName, string packagePath)
    {
        var manifest = LoadPackageManifest(packagePath);
        
        _links[packageName] = new PackageLink
        {
            Name = packageName,
            Version = manifest.Version,
            Path = packagePath,
            ManifestPath = Path.Combine(packagePath, "package.ekko.json"),
            IsTypeScript = File.Exists(Path.Combine(packagePath, "tsconfig.json")),
            LinkedAt = DateTime.UtcNow
        };
        
        SaveRegistry();
    }
    
    public PackageLink? GetLink(string packageName)
    {
        return _links.TryGetValue(packageName, out var link) ? link : null;
    }
}

public class PackageLink
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Path { get; set; }
    public string ManifestPath { get; set; }
    public bool IsTypeScript { get; set; }
    public DateTime LinkedAt { get; set; }
}
```

### 2. Development Package Loader

```csharp
public class DevelopmentPackageLoader : IPackageLoader
{
    private readonly PackageLinkRegistry _linkRegistry;
    private readonly TypeScriptCompiler _tsCompiler;
    private readonly FileSystemWatcher _watcher;
    
    public async Task<IEkkoPackage> LoadPackage(string packageName, string? version = null)
    {
        // Check if package is linked
        var link = _linkRegistry.GetLink(packageName);
        if (link != null)
        {
            return await LoadDevelopmentPackage(link);
        }
        
        // Fall back to normal package loading
        return await LoadCompiledPackage(packageName, version);
    }
    
    private async Task<IEkkoPackage> LoadDevelopmentPackage(PackageLink link)
    {
        // Create development package wrapper
        var devPackage = new DevelopmentPackage(link);
        
        // Compile TypeScript if needed
        if (link.IsTypeScript && ShouldCompile(link))
        {
            await CompileTypeScript(link);
        }
        
        // Set up file watcher
        if (IsWatchEnabled(link))
        {
            SetupFileWatcher(link);
        }
        
        return devPackage;
    }
}
```

### 3. Development Package Implementation

```csharp
public class DevelopmentPackage : IEkkoPackage
{
    private readonly PackageLink _link;
    private readonly DevelopmentFileSystem _fileSystem;
    private readonly PackageManifest _manifest;
    private readonly TypeScriptCompiler _tsCompiler;
    
    public DevelopmentPackage(PackageLink link, TypeScriptCompiler tsCompiler)
    {
        _link = link;
        _fileSystem = new DevelopmentFileSystem(link.Path, tsCompiler);
        _manifest = LoadManifest();
        _tsCompiler = tsCompiler;
    }
    
    public string Name => _manifest.Name;
    public string Version => _manifest.Version + "-dev";
    
    public IPackageFileSystem GetFileSystem() => _fileSystem;
    public PackageManifest GetManifest() => _manifest;
}

public class DevelopmentFileSystem : IPackageFileSystem
{
    private readonly string _rootPath;
    private readonly TypeScriptCompiler _tsCompiler;
    private readonly Dictionary<string, string> _compiledCache = new();
    
    public DevelopmentFileSystem(string rootPath, TypeScriptCompiler tsCompiler)
    {
        _rootPath = rootPath;
        _tsCompiler = tsCompiler;
    }
    
    public bool FileExists(string path)
    {
        var fullPath = Path.Combine(_rootPath, path);
        // Check for both .ts and .js files
        return File.Exists(fullPath) || 
               File.Exists(Path.ChangeExtension(fullPath, ".ts")) ||
               File.Exists(Path.ChangeExtension(fullPath, ".js"));
    }
    
    public byte[] ReadFile(string path)
    {
        // For non-text files, just read as-is
        if (!IsTextFile(path))
        {
            var fullPath = Path.Combine(_rootPath, path);
            return File.ReadAllBytes(fullPath);
        }
        
        // For text files, use ReadTextFile which handles compilation
        var text = ReadTextFile(path);
        return Encoding.UTF8.GetBytes(text);
    }
    
    public string ReadTextFile(string path)
    {
        var fullPath = Path.Combine(_rootPath, path);
        
        // If requesting a .js file but only .ts exists, compile it
        if (path.EndsWith(".js") && !File.Exists(fullPath))
        {
            var tsPath = Path.ChangeExtension(fullPath, ".ts");
            if (File.Exists(tsPath))
            {
                return CompileTypeScript(tsPath, path);
            }
        }
        
        // If it's a .ts file, compile it
        if (path.EndsWith(".ts") && File.Exists(fullPath))
        {
            return CompileTypeScript(fullPath, path);
        }
        
        // Otherwise, read as-is
        return File.ReadAllText(fullPath);
    }
    
    private string CompileTypeScript(string tsPath, string requestedPath)
    {
        // Check cache first
        var lastModified = File.GetLastWriteTimeUtc(tsPath);
        var cacheKey = $"{tsPath}:{lastModified:O}";
        
        if (_compiledCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }
        
        // Compile TypeScript
        var tsContent = File.ReadAllText(tsPath);
        var compiled = _tsCompiler.Compile(tsContent, tsPath);
        
        // Cache the result
        _compiledCache[cacheKey] = compiled;
        
        // Clean old cache entries
        if (_compiledCache.Count > 100)
        {
            _compiledCache.Clear();
        }
        
        return compiled;
    }
    
    public Stream OpenStream(string path)
    {
        // For text files that might need compilation, use ReadTextFile
        if (IsTextFile(path))
        {
            var content = ReadTextFile(path);
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }
        
        // For binary files, stream directly
        var fullPath = Path.Combine(_rootPath, path);
        return File.OpenRead(fullPath);
    }
    
    private bool IsTextFile(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        return ext == ".js" || ext == ".ts" || ext == ".json" || 
               ext == ".css" || ext == ".html" || ext == ".xml";
    }
}
```

### 4. Module Resolution for Development Packages

```csharp
public class DevelopmentModuleResolver : IModuleResolver
{
    public string ResolveModule(string specifier, ModuleContext context)
    {
        // If current module is from development package
        if (context.IsDevelopment)
        {
            if (IsRelativeImport(specifier))
            {
                // Resolve to file system path
                var basePath = Path.GetDirectoryName(context.FilePath);
                var resolvedPath = Path.GetFullPath(Path.Combine(basePath, specifier));
                
                // Ensure it's within package boundaries
                if (!resolvedPath.StartsWith(context.PackagePath))
                {
                    throw new SecurityException("Cannot import outside package directory");
                }
                
                return $"dev-package:{context.PackageName}:{GetRelativePath(context.PackagePath, resolvedPath)}";
            }
        }
        
        return DefaultResolve(specifier, context);
    }
}
```

### 5. On-Demand TypeScript Compilation

Since EkkoJS compiles TypeScript on-the-fly, there's no need for file watchers or pre-compilation:

```csharp
public class DevelopmentModuleLoader
{
    private readonly TypeScriptCompiler _tsCompiler;
    
    public async Task<string> LoadModule(string modulePath, IPackageFileSystem fs)
    {
        // Get the module content
        var content = fs.ReadTextFile(modulePath);
        
        // The DevelopmentFileSystem already handles TS compilation
        // Just return the content (already compiled if it was .ts)
        return content;
    }
}
```

The beauty is that TypeScript compilation happens transparently:
- Request `index.js` → If only `index.ts` exists, compile and return
- Request `index.ts` → Compile and return JavaScript
- Compilation results are cached based on file modification time

### 6. Project Link Configuration

```json
// In project's ekko.config.json
{
  "name": "my-app",
  "version": "1.0.0",
  "linkedPackages": {
    "@mycompany/http-client": {
      "path": "/home/user/projects/my-http-package",
      "version": "link"
    }
  }
}
```

### 7. CLI Implementation

```csharp
public class LinkCommand : Command
{
    public LinkCommand() : base("link", "Link a package for development")
    {
        var packageArgument = new Argument<string?>(
            name: "package",
            description: "Package name to link",
            getDefaultValue: () => null
        );
        
        AddArgument(packageArgument);
        
        this.SetHandler(async (string? packageName) =>
        {
            if (packageName == null)
            {
                // Link current directory as package
                await LinkCurrentDirectory();
            }
            else
            {
                // Link package to current project
                await LinkPackageToProject(packageName);
            }
        }, packageArgument);
    }
    
    private async Task LinkCurrentDirectory()
    {
        var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "package.ekko.json");
        if (!File.Exists(manifestPath))
        {
            Console.WriteLine("❌ No package.ekko.json found in current directory");
            return;
        }
        
        var manifest = JsonSerializer.Deserialize<PackageManifest>(
            await File.ReadAllTextAsync(manifestPath)
        );
        
        var registry = new PackageLinkRegistry();
        registry.RegisterLink(manifest.Name, Directory.GetCurrentDirectory());
        
        Console.WriteLine($"✅ Package \"{manifest.Name}\" linked from {Directory.GetCurrentDirectory()}");
    }
}
```

## Development Workflow Example

```bash
# 1. Create new package
ekko create package @mycompany/http-client
cd http-client

# 2. Develop with TypeScript (or JavaScript)
echo 'export const get = (url: string) => fetch(url);' > src/index.ts

# 3. Link for development
ekko link

# 4. In another project
cd ~/my-app
ekko link @mycompany/http-client

# 5. Use in code
# app.js
import http from 'package:@mycompany/http-client';
const data = await http.get('https://api.example.com');

# 6. Edit and test immediately (no compilation step!)
# Edit http-client/src/index.ts
# Changes are reflected immediately on next run
# TypeScript is compiled on-demand when modules are loaded

# 7. When ready to publish
cd ~/http-client
ekko build package  # Creates the .dll with embedded resources
ekko publish
```

## Benefits

1. **Seamless Development**: No need to rebuild packages constantly
2. **Live Reloading**: Changes reflected immediately
3. **TypeScript Support**: Auto-compilation on save
4. **Same Import Syntax**: Use `package:` protocol in development
5. **Security**: Can't import outside package directory
6. **Easy Testing**: Test packages in real projects before publishing