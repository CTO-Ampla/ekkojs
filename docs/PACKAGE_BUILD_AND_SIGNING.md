# Package Build and Assembly Signing for EkkoJS

## Self-Contained .NET 9 Package Compilation

Yes, since EkkoJS is a self-contained .NET 9 runtime, it can compile and package assemblies properly. Here's how it works:

### 1. Self-Contained Runtime Architecture

EkkoJS embeds the necessary .NET 9 runtime components, which means:
- The runtime includes the .NET 9 compiler services (Roslyn)
- It can dynamically compile C# code to assemblies at runtime
- All necessary .NET libraries are bundled with EkkoJS

### 2. Package Build Process

When building an EkkoJS package into a .NET assembly:

```bash
ekko build package
```

The process:
1. **TypeScript Compilation**: Transpiles TS to JS using embedded TypeScript compiler
2. **Assembly Generation**: Creates a .NET 9 project dynamically
3. **Resource Embedding**: Embeds all package files as assembly resources
4. **Interface Implementation**: Generates IEkkoPackage implementation
5. **Compilation**: Uses Roslyn to compile the assembly
6. **Output**: Produces a .dll compatible with .NET 9

### 3. Dynamic Compilation Implementation

```csharp
public class PackageCompiler
{
    private readonly CSharpCompilation _compilation;
    
    public async Task<byte[]> CompilePackageAsync(PackageManifest manifest, Dictionary<string, byte[]> files)
    {
        // Create compilation with .NET 9 references
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IEkkoPackage).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location)
        };
        
        // Generate source code for package
        var sourceCode = GeneratePackageSource(manifest, files);
        
        // Create syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        
        // Create compilation
        var compilation = CSharpCompilation.Create(
            $"{manifest.Name}.dll",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                platform: Platform.AnyCpu
            )
        );
        
        // Compile to memory
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        
        if (!result.Success)
        {
            throw new CompilationException(result.Diagnostics);
        }
        
        return ms.ToArray();
    }
}
```

## Assembly Signing

Yes, EkkoJS supports assembly signing for security and trust verification.

### 1. Strong Name Signing

For strong name signing during package build:

```bash
# Sign with strong name key
ekko build package --sign-key ./mykey.snk

# Or with key container
ekko build package --sign-key-container "MyKeyContainer"
```

Configuration in `.ekko/build.json`:
```json
{
  "signing": {
    "strongName": {
      "keyFile": "./keys/company.snk",
      "delaySign": false
    }
  }
}
```

Implementation:
```csharp
var compilation = CSharpCompilation.Create(
    assemblyName,
    syntaxTrees,
    references,
    new CSharpCompilationOptions(
        OutputKind.DynamicallyLinkedLibrary,
        cryptoKeyFile: keyFilePath,
        delaySign: false,
        strongNameProvider: new DesktopStrongNameProvider()
    )
);
```

### 2. Authenticode Signing

For code signing certificates (post-compilation):

```bash
# Sign with certificate
ekko build package --sign-cert "CN=My Company" --timestamp "http://timestamp.digicert.com"
```

Configuration:
```json
{
  "signing": {
    "authenticode": {
      "certificate": "CN=My Company, O=My Company Inc., C=US",
      "timestampUrl": "http://timestamp.digicert.com",
      "description": "EkkoJS Package"
    }
  }
}
```

Implementation using SignTool programmatically:
```csharp
public async Task SignAssemblyAsync(string assemblyPath, AuthenticodeSigning config)
{
    var signTool = new SignToolWrapper();
    
    await signTool.SignAsync(new SignOptions
    {
        FilePath = assemblyPath,
        Certificate = config.Certificate,
        TimestampUrl = config.TimestampUrl,
        Description = config.Description,
        Url = config.Url
    });
}
```

### 3. Package Verification

EkkoJS verifies signed packages during loading:

```csharp
public class PackageLoader
{
    public async Task<IEkkoPackage> LoadPackageAsync(string packagePath)
    {
        // Verify strong name
        var assembly = Assembly.LoadFrom(packagePath);
        var strongName = assembly.GetName().GetPublicKey();
        
        if (strongName?.Length > 0)
        {
            // Verify strong name signature
            if (!StrongNameSignatureVerificationEx(packagePath, true, out var wasVerified))
            {
                throw new SecurityException("Strong name verification failed");
            }
        }
        
        // Verify Authenticode signature
        var authenticode = AuthenticodeTools.IsTrusted(packagePath);
        if (Config.RequireAuthenticode && !authenticode.IsTrusted)
        {
            throw new SecurityException($"Authenticode verification failed: {authenticode.Reason}");
        }
        
        // Load package
        return (IEkkoPackage)Activator.CreateInstance(
            assembly.GetType($"{assembly.GetName().Name}.Package")
        );
    }
}
```

### 4. Security Configuration

Global signing requirements in `.ekko/security.json`:
```json
{
  "packages": {
    "requireStrongName": true,
    "requireAuthenticode": false,
    "trustedPublishers": [
      "CN=My Company, O=My Company Inc., C=US",
      "CN=Trusted Partner, O=Partner Corp, C=US"
    ],
    "allowUnsigned": {
      "development": true,
      "production": false
    }
  }
}
```

### 5. Development vs Production

During development:
```bash
# Build unsigned for local testing
ekko build package --configuration Debug

# Test with relaxed security
ekko run --allow-unsigned-packages
```

For production:
```bash
# Build with full signing
ekko build package --configuration Release --sign-key ./company.snk --sign-cert "CN=My Company"

# Run with strict security
ekko run --require-signed-packages
```

## Benefits of This Approach

1. **Security**: Signed assemblies prevent tampering and ensure authenticity
2. **Trust**: Users can verify packages come from trusted sources
3. **GAC Support**: Strong-named assemblies can be installed in Global Assembly Cache
4. **Version Control**: Strong names include version info for side-by-side execution
5. **Enterprise Ready**: Meets corporate security requirements

## Signing Workflow Example

```bash
# 1. Generate strong name key (one time)
sn -k company.snk

# 2. Configure build settings
cat > .ekko/build.json << EOF
{
  "signing": {
    "strongName": {
      "keyFile": "./company.snk"
    },
    "authenticode": {
      "certificate": "CN=My Company",
      "timestampUrl": "http://timestamp.digicert.com"
    }
  }
}
EOF

# 3. Build and sign package
ekko build package

# 4. Verify signatures
ekko verify-package ./dist/MyPackage.dll

# Output:
# ✓ Strong name signature valid
# ✓ Authenticode signature valid
# ✓ Certificate: CN=My Company, O=My Company Inc., C=US
# ✓ Timestamp: 2024-01-15 10:30:00 UTC
```

This comprehensive signing support ensures EkkoJS packages can meet enterprise security requirements while maintaining the ease of JavaScript development.