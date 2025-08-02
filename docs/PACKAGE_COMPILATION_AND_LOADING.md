# Package Compilation and Loading in EkkoJS

## Overview

EkkoJS supports compiling JavaScript/TypeScript packages into .NET assemblies using Roslyn, and then loading these compiled packages at runtime through the `package:` protocol.

## Package Compilation Process

### 1. Building a Package

To compile a package into a .NET assembly:

```bash
ekko build package [directory]
```

This command:
1. Reads the `.ekko/ekko.json` manifest file
2. Collects all package files (src/, lib/, assets/, .ekko/)
3. Generates C# code that implements `IEkkoPackage`
4. Embeds all files as Base64-encoded resources
5. Compiles using Roslyn to produce a .dll file

### 2. Package Structure

A valid EkkoJS package must have:
```
package-directory/
├── .ekko/
│   └── ekko.json    # Required manifest file
├── src/             # Source code
│   └── index.js     # Main entry point (specified in manifest)
├── lib/             # Additional libraries
├── assets/          # Static resources
└── README.md        # Documentation
```

### 3. Manifest Format (.ekko/ekko.json)

```json
{
  "name": "@scope/package-name",
  "version": "1.0.0",
  "description": "Package description",
  "author": "Author Name",
  "license": "MIT",
  "keywords": ["keyword1", "keyword2"],
  "main": "src/index.js"
}
```

## Loading Compiled Packages

### 1. Package Loading Process

When you import a package using `package:@scope/name`:

1. **Module Loader Resolution**: The `PackageModuleLoader` checks if it can handle the specifier
2. **Assembly Search**: Searches for `scope.name.dll` in:
   - `./node_modules/`
   - `./packages/`
   - `./dist/`
   - Custom paths
3. **Assembly Loading**: Uses `Assembly.LoadFrom()` to load the .dll
4. **Package Instantiation**: Creates instance of the package class
5. **Main File Execution**: Executes the main entry point specified in manifest
6. **Export Collection**: Returns the module's exports

### 2. Import Syntax

```javascript
// Import default export
import myPackage from 'package:@demo/hello-world';

// Use the package
console.log(myPackage.greet('World'));
console.log(myPackage.version);
```

### 3. ES Module Support

The package loader handles ES modules by:
- Detecting `export` statements in the main file
- Creating a wrapper that collects exports
- Transforming ES module syntax to compatible format
- Returning the collected exports object

## Implementation Details

### IEkkoPackage Interface

```csharp
public interface IEkkoPackage
{
    string Name { get; }
    string Version { get; }
    PackageManifest GetManifest();
    IPackageFileSystem GetFileSystem();
}
```

### Virtual File System

Each package includes an embedded virtual file system:
- Files are stored as Base64-encoded strings
- Accessible through `IPackageFileSystem` interface
- Supports file existence checks, reading, and pattern matching

### Generated Assembly Structure

The compiler generates:
1. A namespace based on the package name
2. A `Package` class implementing `IEkkoPackage`
3. An embedded file system with all package resources
4. Methods to access manifest and files

## Example: Complete Workflow

### 1. Create a Package

```bash
mkdir my-utils
cd my-utils
mkdir -p .ekko src

# Create manifest
cat > .ekko/ekko.json << EOF
{
  "name": "@mycompany/utils",
  "version": "1.0.0",
  "description": "Utility functions",
  "author": "My Company",
  "license": "MIT",
  "main": "src/index.js"
}
EOF

# Create main file
cat > src/index.js << EOF
export function formatDate(date) {
    return date.toISOString().split('T')[0];
}

export function capitalize(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

export default {
    formatDate,
    capitalize
};
EOF
```

### 2. Build the Package

```bash
ekko build package
# Output: my-utils/dist/mycompany.utils.dll
```

### 3. Use the Package

```javascript
import utils from 'package:@mycompany/utils';

const today = new Date();
console.log(utils.formatDate(today));  // 2024-01-20
console.log(utils.capitalize('hello')); // Hello
```

## Limitations

1. **No circular dependencies**: Packages cannot have circular import dependencies
2. **ES modules only**: CommonJS is not supported in packages
3. **Static resources**: All files must be known at compile time
4. **No native modules**: Packages cannot include native libraries directly

## Security Considerations

1. **Assembly loading**: Only load packages from trusted sources
2. **No sandboxing**: Packages run with full runtime permissions
3. **Code signing**: Use assembly signing for production packages
4. **Input validation**: Validate all package inputs before processing

## Future Enhancements

1. **Package registry**: Central repository for package discovery
2. **Dependency resolution**: Automatic dependency installation
3. **Hot reloading**: Development mode with live updates
4. **Package validation**: Pre-flight checks before compilation
5. **Source maps**: Debugging support for compiled packages