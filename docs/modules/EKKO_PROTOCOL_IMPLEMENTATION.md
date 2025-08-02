# Ekko Protocol Implementation

## Overview

The ekko: protocol has been successfully implemented in EkkoJS, allowing scripts to import built-in modules using ES module syntax:

```javascript
// Default imports
import fs from 'ekko:fs';
import path from 'ekko:path';

// Named imports
import { readFileSync, writeFileSync } from 'ekko:fs';
import { join, dirname, basename } from 'ekko:path';

// Dynamic imports
const fs = await import('ekko:fs');
```

## Architecture

### ESModuleLoader
- Central component that manages ES module registration and loading
- Supports both static and dynamic imports
- Caches loaded modules to avoid redundant initialization
- Parses module specifiers to extract protocol and module name
- Provides special handling for modules that need JavaScript wrappers
- Creates proper ES module namespace objects with frozen exports

### ESModuleCompiler
- Transforms ES module syntax to executable code
- Handles static imports by converting them to dynamic imports
- Supports default imports, named imports, and mixed imports
- Wraps modules in async functions for top-level await support
- Generates unique identifiers to avoid naming conflicts

### IModule Interface
- Simple interface that all modules must implement
- Properties: `Name`, `Protocol`, `GetExports()`
- Allows for different module types (ekko:, dotnet:, ipc:)

### Built-in Modules

#### ekko:fs
File system operations with the following methods:
- `readFile(path)` / `readFileSync(path)` - Read file contents
- `writeFile(path, content)` / `writeFileSync(path, content)` - Write file
- `exists(path)` / `existsSync(path)` - Check if file/directory exists
- `mkdir(path)` / `mkdirSync(path)` - Create directory
- `readdir(path)` / `readdirSync(path)` - List directory contents
- `rm(path)` / `rmSync(path)` - Remove file or directory
- `stat(path)` / `statSync(path)` - Get file/directory statistics

#### ekko:path
Path manipulation utilities:
- `join(...parts)` - Join path segments
- `resolve(...parts)` - Resolve to absolute path
- `dirname(path)` - Get directory name
- `basename(path, ext?)` - Get file name
- `extname(path)` - Get file extension
- `parse(path)` - Parse path into components
- `format(pathObject)` - Build path from components
- `isAbsolute(path)` - Check if path is absolute
- `relative(from, to)` - Get relative path
- `sep` - Path separator for the OS
- `delimiter` - Path delimiter for the OS

## Implementation Details

### Handling Variadic Functions
JavaScript's spread operator (`...args`) creates JavaScript arrays, while C# expects .NET arrays. The implementation uses:

1. **Internal Functions**: C# methods that accept .NET types
2. **JavaScript Wrappers**: Generated at runtime to convert arguments
3. **Array Conversion**: Helper method to convert JavaScript arrays to string[]

### Module Registration
Modules are registered during runtime initialization:

```csharp
private void RegisterBuiltInModules()
{
    _moduleLoader!.RegisterModule(new FileSystemModule());
    _moduleLoader!.RegisterModule(new PathModule());
}
```

### Import Handler
The ES module loader provides full ES module support:

1. **Static imports** - Transformed at compile time by ESModuleCompiler
2. **Dynamic imports** - Available via `globalThis.import()` function
3. **import.meta** - Provides module metadata including URL

```javascript
// Static imports (transformed at compile time)
import fs from 'ekko:fs';
import { join } from 'ekko:path';

// Dynamic imports (runtime)
const module = await import('ekko:fs');

// import.meta
console.log(import.meta.url); // Current module URL
```

## Usage Example

```javascript
// Import built-in modules using ES modules
import fs from 'ekko:fs';
import { join, resolve } from 'ekko:path';

// Use file system operations
const content = fs.readFileSync('config.json');
const data = JSON.parse(content);

// Use path utilities
const configPath = join('app', 'config', 'settings.json');
const absPath = resolve(configPath);

// Write modified data
fs.writeFileSync(absPath, JSON.stringify(data, null, 2));

// Async example with top-level await
const files = fs.readdirSync('.');
for (const file of files) {
    console.log(`Found: ${file}`);
}
```

## Future Extensions

The protocol system is designed to be extensible:
- `dotnet:` - For loading .NET assemblies
- `ipc:` - For inter-process communication modules
- `npm:` - For loading npm packages (future)
- Custom protocols can be added by implementing IModule