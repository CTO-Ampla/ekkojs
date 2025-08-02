# EkkoJS Package Distribution Architecture

## Overview

EkkoJS packages are distributed as .NET assemblies (.dll files) that contain all package resources embedded within them. Each package assembly acts as a virtual file system, providing access to JavaScript code, assets, and native libraries.

## Package Assembly Structure

### 1. Package Interface

Every EkkoJS package must implement the `IEkkoPackage` interface:

```csharp
public interface IEkkoPackage
{
    string Name { get; }
    string Version { get; }
    PackageManifest GetManifest();
    IPackageFileSystem GetFileSystem();
}

public interface IPackageFileSystem
{
    bool FileExists(string path);
    byte[] ReadFile(string path);
    string ReadTextFile(string path);
    string[] ListFiles(string pattern = "*");
    Stream OpenStream(string path);
    PackageFileInfo GetFileInfo(string path);
}
```

### 2. Package Manifest

The manifest is embedded as `package.ekko.json`:

```json
{
  "name": "@ekko/http-client",
  "version": "1.0.0",
  "description": "HTTP client for EkkoJS",
  "main": "dist/index.js",
  "types": "dist/index.d.ts",
  "exports": {
    ".": "./dist/index.js",
    "./advanced": "./dist/advanced.js"
  },
  "dependencies": {
    "@ekko/core": "^1.0.0",
    "@ekko/promises": "^2.1.0"
  },
  "resources": {
    "native": {
      "libcurl": {
        "windows": "native/win-x64/curl.dll",
        "linux": "native/linux-x64/libcurl.so",
        "darwin": "native/darwin-x64/libcurl.dylib"
      }
    },
    "assets": [
      "templates/*.html",
      "schemas/*.json"
    ]
  },
  "permissions": [
    "network",
    "file:read"
  ]
}
```

### 3. Embedded Resource Structure

Resources are embedded with a hierarchical naming convention:

```
EkkoJS.Packages.HttpClient.Resources.package.ekko.json
EkkoJS.Packages.HttpClient.Resources.dist.index.js
EkkoJS.Packages.HttpClient.Resources.dist.index.d.ts
EkkoJS.Packages.HttpClient.Resources.native.win-x64.curl.dll
EkkoJS.Packages.HttpClient.Resources.native.linux-x64.libcurl.so
EkkoJS.Packages.HttpClient.Resources.templates.default.html
```

### 4. Package Implementation Example

```csharp
namespace EkkoJS.Packages.HttpClient
{
    public class HttpClientPackage : IEkkoPackage
    {
        private readonly PackageManifest _manifest;
        private readonly PackageFileSystem _fileSystem;

        public string Name => "@ekko/http-client";
        public string Version => "1.0.0";

        public HttpClientPackage()
        {
            _fileSystem = new PackageFileSystem(this.GetType().Assembly);
            _manifest = LoadManifest();
        }

        public PackageManifest GetManifest() => _manifest;
        public IPackageFileSystem GetFileSystem() => _fileSystem;

        private PackageManifest LoadManifest()
        {
            var json = _fileSystem.ReadTextFile("package.ekko.json");
            return JsonSerializer.Deserialize<PackageManifest>(json)!;
        }
    }
}
```

### 5. Virtual File System Implementation

```csharp
public class PackageFileSystem : IPackageFileSystem
{
    private readonly Assembly _assembly;
    private readonly string _resourcePrefix;
    private readonly Dictionary<string, string> _fileMap;

    public PackageFileSystem(Assembly assembly)
    {
        _assembly = assembly;
        _resourcePrefix = assembly.GetName().Name + ".Resources.";
        _fileMap = BuildFileMap();
    }

    public bool FileExists(string path)
    {
        var resourceName = PathToResourceName(path);
        return _fileMap.ContainsKey(resourceName);
    }

    public byte[] ReadFile(string path)
    {
        var resourceName = PathToResourceName(path);
        if (!_fileMap.TryGetValue(resourceName, out var fullName))
            throw new FileNotFoundException($"File not found: {path}");

        using var stream = _assembly.GetManifestResourceStream(fullName);
        if (stream == null)
            throw new FileNotFoundException($"Resource not found: {path}");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public string ReadTextFile(string path)
    {
        var bytes = ReadFile(path);
        return Encoding.UTF8.GetString(bytes);
    }

    public Stream OpenStream(string path)
    {
        var resourceName = PathToResourceName(path);
        if (!_fileMap.TryGetValue(resourceName, out var fullName))
            throw new FileNotFoundException($"File not found: {path}");

        var stream = _assembly.GetManifestResourceStream(fullName);
        if (stream == null)
            throw new FileNotFoundException($"Resource not found: {path}");

        return stream;
    }

    private Dictionary<string, string> BuildFileMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var names = _assembly.GetManifestResourceNames();
        
        foreach (var name in names)
        {
            if (name.StartsWith(_resourcePrefix))
            {
                var shortName = name.Substring(_resourcePrefix.Length);
                map[shortName] = name;
            }
        }
        
        return map;
    }

    private string PathToResourceName(string path)
    {
        // Convert path separators to dots for resource naming
        return path.Replace('/', '.').Replace('\\', '.');
    }
}
```

## Package Loading in Runtime

### 1. Package Protocol

Packages are loaded using the `package:` protocol:

```javascript
// Load from installed packages
import http from 'package:@ekko/http-client';
import { post } from 'package:@ekko/http-client/advanced';

// The runtime resolves this to:
// 1. Find @ekko/http-client package assembly
// 2. Load the assembly and get IEkkoPackage
// 3. Use IPackageFileSystem to read the main file
// 4. Execute the JavaScript code
```

### 2. Package Loader Integration

```csharp
public class PackageModuleLoader
{
    private readonly Dictionary<string, IEkkoPackage> _loadedPackages;
    private readonly string _packagesDirectory;

    public async Task<object> LoadPackage(string packageName)
    {
        if (_loadedPackages.ContainsKey(packageName))
            return _loadedPackages[packageName];

        // Find package assembly
        var packagePath = ResolvePackagePath(packageName);
        var assembly = Assembly.LoadFrom(packagePath);
        
        // Find IEkkoPackage implementation
        var packageType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IEkkoPackage).IsAssignableFrom(t));
            
        if (packageType == null)
            throw new InvalidOperationException($"No IEkkoPackage found in {packageName}");

        var package = (IEkkoPackage)Activator.CreateInstance(packageType)!;
        _loadedPackages[packageName] = package;

        // Load and execute main module
        var manifest = package.GetManifest();
        var mainFile = manifest.Main ?? "index.js";
        var fileSystem = package.GetFileSystem();
        var code = fileSystem.ReadTextFile(mainFile);

        // Execute in V8 context
        return await ExecutePackageCode(package, code);
    }
}
```

## Development to Distribution Workflow

### 1. Development Structure

```
my-package/
├── src/
│   ├── index.ts
│   └── advanced.ts
├── native/
│   ├── win-x64/
│   ├── linux-x64/
│   └── darwin-x64/
├── assets/
│   └── templates/
├── package.ekko.json
├── tsconfig.json
└── build.ekko.json
```

### 2. Build Process

```bash
# Build command
ekko build package

# What it does:
1. Compile TypeScript to JavaScript
2. Bundle dependencies
3. Create .NET assembly project
4. Embed all resources
5. Implement IEkkoPackage
6. Compile to .dll
7. Sign assembly (optional)
```

### 3. Distribution

```bash
# Publish to EkkoJS registry
ekko publish

# Install from registry
ekko install @ekko/http-client

# Packages are installed to:
~/.ekkojs/packages/@ekko/http-client/1.0.0/http-client.dll
```

## Advanced Features

### 1. Native Library Extraction

For native libraries, the package can extract them on first use:

```csharp
public class NativeLibraryExtractor
{
    public string ExtractLibrary(IPackageFileSystem fs, string libraryName)
    {
        var platform = GetPlatform();
        var manifest = fs.GetManifest();
        var nativePath = manifest.Resources.Native[libraryName][platform];
        
        var targetPath = Path.Combine(
            Path.GetTempPath(),
            "ekkojs",
            manifest.Name,
            manifest.Version,
            Path.GetFileName(nativePath)
        );

        if (!File.Exists(targetPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            var bytes = fs.ReadFile(nativePath);
            File.WriteAllBytes(targetPath, bytes);
        }

        return targetPath;
    }
}
```

### 2. Package Caching

Extracted files and compiled JavaScript can be cached:

```csharp
public class PackageCache
{
    private readonly string _cacheDir;
    
    public bool TryGetCachedModule(string packageName, string version, out CompiledModule module)
    {
        var cachePath = GetCachePath(packageName, version);
        if (File.Exists(cachePath))
        {
            // Load compiled module from cache
            module = LoadFromCache(cachePath);
            return true;
        }
        
        module = null;
        return false;
    }
}
```

### 3. Dependency Resolution

Package dependencies are resolved recursively:

```csharp
public class DependencyResolver
{
    public async Task<List<IEkkoPackage>> ResolveDependencies(PackageManifest manifest)
    {
        var resolved = new List<IEkkoPackage>();
        var queue = new Queue<(string name, string version)>();
        
        foreach (var dep in manifest.Dependencies)
        {
            queue.Enqueue((dep.Key, dep.Value));
        }
        
        while (queue.Count > 0)
        {
            var (name, version) = queue.Dequeue();
            var package = await LoadPackage(name, version);
            resolved.Add(package);
            
            // Add transitive dependencies
            var depManifest = package.GetManifest();
            foreach (var dep in depManifest.Dependencies)
            {
                queue.Enqueue((dep.Key, dep.Value));
            }
        }
        
        return resolved;
    }
}
```

## Benefits

1. **Self-contained**: Everything in a single .dll file
2. **Version isolation**: Multiple versions can coexist
3. **Fast loading**: No file system scanning
4. **Secure**: Assemblies can be signed
5. **Cross-platform**: Same package works everywhere
6. **Efficient**: Resources are compressed in assembly
7. **Type-safe**: Strong typing with .NET interfaces