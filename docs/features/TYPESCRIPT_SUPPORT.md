# TypeScript Support in EkkoJS

## Overview

EkkoJS provides first-class TypeScript support, allowing you to run `.ts` and `.tsx` files directly without a separate compilation step. TypeScript files are automatically detected and transpiled to JavaScript before execution.

## How It Works

1. **Automatic Detection**: Files with `.ts` or `.tsx` extensions are automatically detected as TypeScript
2. **In-Memory Compilation**: TypeScript is compiled to JavaScript in memory using the official TypeScript compiler
3. **ES Module Support**: TypeScript files can use ES module imports, including ekko: protocol imports
4. **Type Definitions**: Built-in modules include TypeScript type definitions for better IDE support

## Usage

### Running TypeScript Files

```bash
# Run a TypeScript file directly
ekko run script.ts

# TypeScript files in REPL
ekko repl
> import './myModule.ts'
```

### Example TypeScript Code

```typescript
// script.ts
import fs from 'ekko:fs';
import { join, dirname } from 'ekko:path';

interface Config {
    name: string;
    version: number;
    settings: Record<string, any>;
}

const config: Config = {
    name: 'MyApp',
    version: 1,
    settings: {
        debug: true,
        port: 3000
    }
};

// Write typed configuration
fs.writeFileSync('config.json', JSON.stringify(config, null, 2));

// Use path utilities with type safety
const configPath = join(process.cwd(), 'config.json');
console.log(`Config saved to: ${configPath}`);
```

## TypeScript Configuration

EkkoJS uses the following default TypeScript compiler options:

```javascript
{
    module: "ESNext",
    target: "ES2022",
    strict: true,
    esModuleInterop: true,
    skipLibCheck: true,
    forceConsistentCasingInFileNames: true,
    moduleResolution: "node",
    noEmitOnError: false,
    removeComments: false,
    sourceMap: false
}
```

## Type Definitions

### Built-in Module Types

EkkoJS provides TypeScript type definitions for all built-in modules:

#### ekko:fs
```typescript
import fs from 'ekko:fs';
import { readFileSync, writeFileSync, Stats } from 'ekko:fs';

// All functions are fully typed
const content: string = fs.readFileSync('file.txt');
const stats: Stats = fs.statSync('file.txt');
```

#### ekko:path
```typescript
import path from 'ekko:path';
import { ParsedPath, join, resolve } from 'ekko:path';

// Type-safe path operations
const parsed: ParsedPath = path.parse('/home/user/file.txt');
const joined: string = join('dir', 'subdir', 'file.txt');
```

### Using Type Definitions

To get TypeScript IntelliSense in your IDE:

1. Create a `tsconfig.json` in your project root:
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "moduleResolution": "node"
  }
}
```

2. The type definitions are automatically available for ekko: protocol imports

## Implementation Details

### TypeScript Compiler Integration

The TypeScript compiler is loaded dynamically:
- Downloaded from CDN on first use
- Cached locally for offline use
- Runs in a separate V8 engine instance for isolation

### Compilation Process

1. TypeScript source is detected by file extension
2. Code is passed to the TypeScript compiler
3. Compiler errors are reported with proper formatting
4. JavaScript output is passed to the ES module transformer
5. Final code is executed in the main runtime

### Performance

- First run may be slower due to compiler download
- Subsequent runs use cached compiler
- No file system overhead - all compilation happens in memory
- TypeScript compilation adds minimal overhead to script execution

## Future Enhancements

- Support for project-wide tsconfig.json
- Watch mode for development
- Source map support for better debugging
- Integration with TypeScript language server
- Support for .d.ts declaration files in projects