# EkkoJS Project Structure Conventions

## Overview

EkkoJS projects follow a consistent folder structure with `.ekko/` containing all configuration files. The main `ekko.json` file serves as the entry point (similar to Node.js `package.json`) and defines the project type, dependencies, and structure.

## Standard Folder Structure

Every EkkoJS project follows this conventional structure:

```
project/
├── .ekko/                   # Configuration directory (mandatory)
│   ├── ekko.json           # Main configuration file (mandatory)
│   ├── build.json          # Build settings (optional)
│   ├── native.json         # Native library mappings (optional)
│   └── extensions.json     # Local extensions (optional)
├── src/                     # Source code (mandatory)
│   └── index.ts            # Default entry point
├── assets/                  # Static resources (optional)
├── native/                  # Native libraries (optional)
├── tests/                   # Test files (optional)
├── docs/                    # Documentation (optional)
└── README.md               # Project readme
```

### Folder Purposes

- **`.ekko/`** - All EkkoJS configuration files
- **`src/`** - TypeScript/JavaScript source code  
- **`assets/`** - Static files (templates, data, images)
- **`native/`** - Platform-specific native libraries
- **`tests/`** - Unit and integration tests
- **`docs/`** - Project documentation

## Main Configuration File

The `.ekko/ekko.json` is the main entry point (like package.json in Node.js):

**`.ekko/ekko.json`**:
```json
{
  "name": "my-project",
  "version": "1.0.0",
  "type": "application | package | extension",
  "description": "Project description",
  "author": "Your Name",
  "license": "MIT",
  
  // Entry point (for applications)
  "main": "src/index.ts",
  
  // Dependencies
  "dependencies": {
    "@ekko/core": "^1.0.0",
    "@mycompany/utils": "^2.0.0"
  },
  
  // Local package references (for applications)
  "localPackages": {
    "@mycompany/auth": "../packages/auth",
    "@mycompany/db": "../packages/database"
  },
  
  // Folder structure overrides (optional)
  "structure": {
    "source": "src",
    "assets": "assets",
    "native": "native",
    "tests": "tests",
    "output": "dist"
  },
  
  // Runtime configuration
  "runtime": {
    "target": "cli | gui | service | library",
    "permissions": ["network", "file:read", "file:write"],
    "node-compat": false
  },
  
  // Package exports (for packages)
  "exports": {
    ".": "./src/index.ts",
    "./utils": "./src/utils.ts"
  }
}
```

## Project Types

### 1. Application

An application has an entry point and can be executed:

```json
// .ekko/ekko.json for an application
{
  "name": "my-cli-app",
  "version": "1.0.0",
  "type": "application",
  "main": "src/main.ts",
  "runtime": {
    "target": "cli"
  },
  "localPackages": {
    "@mycompany/auth": "../packages/auth"
  }
}
```

### 2. Package

A package is a reusable library without an entry point:
```json
// .ekko/ekko.json for a package
{
  "name": "@mycompany/http-client",
  "version": "2.1.0",
  "type": "package",
  "exports": {
    ".": "./src/index.ts",
    "./advanced": "./src/advanced.ts"
  },
  "dependencies": {
    "@ekko/core": "^1.0.0"
  },
  "permissions": [
    "network"
  ]
}
```

### 3. Extension

Extensions add new commands to the EkkoJS CLI:

```json
// .ekko/ekko.json for an extension
{
  "name": "@ekko/test",
  "version": "1.0.0",
  "type": "extension",
  "commands": {
    "test": {
      "description": "Run tests",
      "handler": "src/commands/test.ts"
    }
  }
}
```

## Customizing Folder Structure

While the default structure is recommended, you can override it in `.ekko/ekko.json`:

```json
{
  "structure": {
    "source": "source",        // Use 'source/' instead of 'src/'
    "assets": "resources",     // Use 'resources/' instead of 'assets/'
    "native": "libs/native",   // Use nested path for native libs
    "tests": "test",          // Use 'test/' instead of 'tests/'
    "output": "build"         // Build output directory
  }
}
```

## Folder Structure Examples

### Simple Package

```
my-utils/
├── .ekko/
│   └── ekko.json           # Package configuration
├── src/
│   ├── index.ts           # Main exports
│   ├── string-utils.ts
│   └── date-utils.ts
├── tests/
│   ├── string-utils.test.ts
│   └── date-utils.test.ts
└── README.md
```

### Package with Native Libraries

```
my-crypto/
├── .ekko/
│   ├── ekko.json
│   └── native.json         # Native library mappings
├── src/
│   └── index.ts
├── native/
│   ├── win-x64/
│   │   └── crypto.dll
│   ├── linux-x64/
│   │   └── libcrypto.so
│   └── darwin-x64/
│       └── libcrypto.dylib
└── assets/
    └── algorithms.json
```

### Application with Local Packages

```
my-app/
├── .ekko/
│   └── ekko.json
├── src/
│   ├── main.ts            # Entry point
│   └── services/
├── packages/              # Local packages
│   ├── auth/
│   │   ├── .ekko/
│   │   │   └── ekko.json
│   │   └── src/
│   └── database/
│       ├── .ekko/
│       │   └── ekko.json
│       └── src/
└── tests/
```

With `.ekko/ekko.json`:
```json
{
  "name": "my-app",
  "type": "application",
  "main": "src/main.ts",
  "localPackages": {
    "@myapp/auth": "./packages/auth",
    "@myapp/database": "./packages/database"
  }
}
```

## Additional Configuration Files

Besides the main `ekko.json`, you can have optional configuration files in `.ekko/`:

### Native Library Mapping (`.ekko/native.json`)

```json
{
  "libraries": {
    "crypto": {
      "windows": "native/win-x64/crypto.dll",
      "linux": "native/linux-x64/libcrypto.so",
      "darwin": "native/darwin-x64/libcrypto.dylib",
      "functions": {
        "encrypt": { "returns": "string", "args": ["string", "string"] },
        "decrypt": { "returns": "string", "args": ["string", "string"] }
      }
    }
  }
}
```

### Build Configuration (`.ekko/build.json`)

```json
{
  "compiler": {
    "target": "ES2022",
    "strict": true
  },
  "bundle": {
    "minify": true,
    "external": ["native:*"]
  },
  "assembly": {
    "sign": true,
    "metadata": {
      "company": "My Company"
    }
  }
}
```

## Best Practices

1. **Keep `.ekko/` clean** - Only EkkoJS configuration files
2. **Use conventional folders** - `src/`, `assets/`, `native/`, `tests/`
3. **Override only when needed** - Use `structure` config for custom paths
4. **Separate concerns** - Don't mix source with assets or native libs
5. **Document dependencies** - Clear `localPackages` references

## Summary

The folder structure is designed to be:
- **Predictable**: Same structure across all EkkoJS projects
- **Flexible**: Can override defaults when needed
- **Clean**: Configuration separate from code
- **Scalable**: Works for simple packages to large monorepos