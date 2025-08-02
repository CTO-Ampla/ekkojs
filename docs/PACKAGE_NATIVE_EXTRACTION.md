# Native Library Extraction Strategy for EkkoJS Packages

## Overview

Native libraries embedded in package assemblies must be extracted to disk before use. This document outlines the extraction strategy and caching mechanism.

## Extraction Process

### 1. Lazy Extraction

Native libraries are only extracted when first accessed:

```csharp
public class NativeLibraryManager
{
    private readonly IPackageFileSystem _fileSystem;
    private readonly PackageManifest _manifest;
    private readonly Dictionary<string, string> _extractedPaths = new();
    
    public string GetNativeLibraryPath(string libraryName)
    {
        // Check if already extracted
        if (_extractedPaths.TryGetValue(libraryName, out var cachedPath))
        {
            if (File.Exists(cachedPath))
                return cachedPath;
        }
        
        // Extract on demand
        var extractedPath = ExtractNativeLibrary(libraryName);
        _extractedPaths[libraryName] = extractedPath;
        return extractedPath;
    }
    
    private string ExtractNativeLibrary(string libraryName)
    {
        // Determine platform-specific path
        var platform = GetCurrentPlatform();
        var embeddedPath = _manifest.Resources.Native[libraryName][platform];
        
        // Calculate extraction path with version isolation
        var extractPath = GetExtractionPath(libraryName);
        
        // Extract if needed
        if (!File.Exists(extractPath) || !VerifyLibraryHash(extractPath, libraryName))
        {
            ExtractLibrary(embeddedPath, extractPath);
        }
        
        return extractPath;
    }
}
```

### 2. Extraction Location Strategy

```csharp
public class ExtractionPathStrategy
{
    public string GetExtractionPath(string packageName, string version, string libraryName)
    {
        // Option 1: User-specific cache (default)
        // ~/.ekkojs/cache/packages/@ekko/http-client/1.0.0/native/win-x64/curl.dll
        var userCache = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".ekkojs",
            "cache",
            "packages",
            packageName,
            version,
            "native",
            GetPlatformDirectory(),
            Path.GetFileName(libraryName)
        );
        
        // Option 2: Temp directory (fallback)
        // /tmp/ekkojs/packages/@ekko/http-client/1.0.0/native/curl.dll
        var tempCache = Path.Combine(
            Path.GetTempPath(),
            "ekkojs",
            "packages",
            packageName.Replace('/', '_'),
            version,
            "native",
            Path.GetFileName(libraryName)
        );
        
        // Option 3: Application directory (if writable)
        // ./ekko_cache/packages/@ekko/http-client/1.0.0/native/curl.dll
        var appCache = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "ekko_cache",
            "packages",
            packageName.Replace('/', '_'),
            version,
            "native",
            Path.GetFileName(libraryName)
        );
        
        // Choose based on availability and permissions
        if (CanWriteToDirectory(Path.GetDirectoryName(userCache)))
            return userCache;
        else if (CanWriteToDirectory(Path.GetDirectoryName(appCache)))
            return appCache;
        else
            return tempCache;
    }
}
```

### 3. Integrity Verification

```csharp
public class LibraryIntegrityChecker
{
    private readonly Dictionary<string, string> _libraryHashes;
    
    public bool VerifyLibraryHash(string filePath, string libraryName)
    {
        if (!File.Exists(filePath))
            return false;
            
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        
        return _libraryHashes[libraryName] == hashString;
    }
    
    public void StoreLibraryMetadata(string extractPath, string libraryName)
    {
        // Store metadata alongside extracted library
        var metadataPath = extractPath + ".ekko.meta";
        var metadata = new
        {
            LibraryName = libraryName,
            PackageName = _manifest.Name,
            PackageVersion = _manifest.Version,
            ExtractionTime = DateTime.UtcNow,
            Hash = _libraryHashes[libraryName],
            Platform = GetCurrentPlatform()
        };
        
        File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata));
    }
}
```

### 4. Atomic Extraction

```csharp
public class AtomicExtractor
{
    public void ExtractLibrary(string embeddedPath, string targetPath)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(targetPath)!;
        Directory.CreateDirectory(directory);
        
        // Extract to temporary file first
        var tempPath = targetPath + ".tmp";
        
        try
        {
            // Read from embedded resource
            var libraryBytes = _fileSystem.ReadFile(embeddedPath);
            
            // Write to temp file
            File.WriteAllBytes(tempPath, libraryBytes);
            
            // Set appropriate permissions (Unix)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                SetExecutablePermissions(tempPath);
            }
            
            // Atomic move
            File.Move(tempPath, targetPath, overwrite: true);
        }
        catch
        {
            // Clean up on failure
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }
    }
    
    private void SetExecutablePermissions(string path)
    {
        var info = new Mono.Unix.UnixFileInfo(path);
        info.FileAccessPermissions = 
            Mono.Unix.FileAccessPermissions.UserRead |
            Mono.Unix.FileAccessPermissions.UserWrite |
            Mono.Unix.FileAccessPermissions.UserExecute |
            Mono.Unix.FileAccessPermissions.GroupRead |
            Mono.Unix.FileAccessPermissions.GroupExecute |
            Mono.Unix.FileAccessPermissions.OtherRead |
            Mono.Unix.FileAccessPermissions.OtherExecute;
    }
}
```

### 5. Cleanup Strategy

```csharp
public class NativeLibraryCleanup
{
    public void CleanupOldVersions(string packageName)
    {
        var cacheDir = GetPackageCacheDirectory(packageName);
        var currentVersion = _manifest.Version;
        
        foreach (var versionDir in Directory.GetDirectories(cacheDir))
        {
            var version = Path.GetFileName(versionDir);
            if (version != currentVersion && IsVersionOlderThan(version, 30))
            {
                try
                {
                    Directory.Delete(versionDir, recursive: true);
                }
                catch
                {
                    // Library might be in use
                }
            }
        }
    }
    
    public void RegisterCleanupOnExit()
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            CleanupTempLibraries();
        };
    }
}
```

## Platform-Specific Considerations

### Windows

```csharp
public class WindowsNativeLoader
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr LoadLibrary(string dllToLoad);
    
    public IntPtr LoadNativeLibrary(string path)
    {
        // Windows searches for dependencies in:
        // 1. Application directory
        // 2. System directory
        // 3. PATH environment variable
        // 4. Directory containing the DLL
        
        // Add library directory to DLL search path
        var directory = Path.GetDirectoryName(path);
        SetDllDirectory(directory);
        
        return LoadLibrary(path);
    }
}
```

### Linux

```csharp
public class LinuxNativeLoader
{
    [DllImport("libdl.so.2")]
    private static extern IntPtr dlopen(string filename, int flags);
    
    private const int RTLD_NOW = 2;
    private const int RTLD_GLOBAL = 0x100;
    
    public IntPtr LoadNativeLibrary(string path)
    {
        // Set LD_LIBRARY_PATH for dependencies
        var directory = Path.GetDirectoryName(path);
        var currentPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
        Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", 
            $"{directory}:{currentPath}");
        
        return dlopen(path, RTLD_NOW | RTLD_GLOBAL);
    }
}
```

### macOS

```csharp
public class MacOSNativeLoader
{
    [DllImport("libdl.dylib")]
    private static extern IntPtr dlopen(string filename, int flags);
    
    public IntPtr LoadNativeLibrary(string path)
    {
        // Handle @rpath dependencies
        var directory = Path.GetDirectoryName(path);
        Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", directory);
        
        return dlopen(path, RTLD_NOW);
    }
}
```

## Usage Example

```csharp
// In the package implementation
public class HttpClientPackage : IEkkoPackage
{
    private readonly NativeLibraryManager _nativeManager;
    
    public object InitializeCurl()
    {
        // This triggers extraction if needed
        var curlPath = _nativeManager.GetNativeLibraryPath("curl");
        
        // Load the library
        var handle = NativeLibrary.Load(curlPath);
        
        // Create wrapper
        return new CurlWrapper(handle);
    }
}
```

```javascript
// From JavaScript
import http from 'package:@ekko/http-client';

// First use triggers extraction
const response = await http.get('https://api.example.com');
// Subsequent uses reuse extracted library
```

## Performance Optimizations

### 1. Parallel Extraction

```csharp
public async Task ExtractAllNativeLibraries()
{
    var tasks = _manifest.Resources.Native
        .Select(kvp => Task.Run(() => ExtractNativeLibrary(kvp.Key)))
        .ToArray();
        
    await Task.WhenAll(tasks);
}
```

### 2. Memory-Mapped Extraction

```csharp
public void ExtractLargeLibrary(string embeddedPath, string targetPath)
{
    using var stream = _fileSystem.OpenStream(embeddedPath);
    using var fileStream = new FileStream(targetPath, FileMode.Create);
    
    // Use larger buffer for large files
    var buffer = new byte[81920]; // 80KB buffer
    int bytesRead;
    
    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
    {
        fileStream.Write(buffer, 0, bytesRead);
    }
}
```

### 3. Lazy Dependency Loading

```csharp
public class LazyNativeLoader
{
    private readonly Lazy<IntPtr> _libraryHandle;
    
    public LazyNativeLoader(string libraryName)
    {
        _libraryHandle = new Lazy<IntPtr>(() =>
        {
            var path = ExtractAndGetPath(libraryName);
            return NativeLibrary.Load(path);
        });
    }
    
    public IntPtr Handle => _libraryHandle.Value;
}
```

## Security Considerations

1. **Extraction Directory Permissions**: Ensure extracted files are not world-writable
2. **Code Signing**: Verify signature before extraction
3. **Path Validation**: Prevent directory traversal attacks
4. **Cleanup**: Remove extracted files when package is uninstalled