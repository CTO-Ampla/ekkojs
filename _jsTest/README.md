# EkkoJS Test Files

This directory contains all test files and examples for the EkkoJS runtime.

## Directory Structure

### `/modules/`
ES module system tests including dynamic imports and module transformations.

### `/typescript/`
TypeScript execution tests demonstrating direct `.ts` file execution.

### `/dotnet/`
.NET assembly loading tests using the `dotnet:` protocol.

### `/native/`
Native library (C/C++) loading tests using the `native:` protocol.

### `/ipc/`
IPC (Inter-Process Communication) tests using the `ipc:` protocol.

### `/native-libraries/`
Contains the C/C++ demo library (mathlib) source code and build files.

### `/test-assemblies/`
Contains the .NET test assembly (TestLibrary) source code and mapping files.

### `/ipc-configs/`
IPC service configuration files (`.ekko.ipc.json`) for demo services.

## Running Tests

Tests can be run using the EkkoJS CLI:

```bash
# Run a specific test
dotnet run --project EkkoJS.CLI -- run _jsTest/modules/test-es-simple.js

# Run TypeScript tests
dotnet run --project EkkoJS.CLI -- run _jsTest/typescript/hello.ts

# Run IPC tests (requires demo service running)
dotnet run --project EkkoJS.Demo.IpcService
dotnet run --project EkkoJS.CLI -- run _jsTest/ipc/test-ipc-simple.js
```

## Test Files in Active Use

### Core Tests
- `simple-test.js` - Basic runtime functionality
- `test.js` - General runtime tests
- `timer-test.js` - Timer and async functionality

### Module System
- `modules/test-es-simple.js` - Basic ES module imports
- `modules/test-dynamic-import.js` - Dynamic import() functionality

### TypeScript
- `typescript/hello.ts` - Basic TypeScript execution
- `typescript/test-imports.ts` - TypeScript with imports

### .NET Integration
- `dotnet/test-assembly.js` - Basic .NET assembly loading
- `dotnet/test-instance.js` - .NET instance creation

### Native Libraries
- `native/test-mathlib.js` - Basic native library calls
- `native/test-mathlib-struct.js` - Struct parameter passing

### IPC Communication
- `ipc/test-ipc-simple.js` - Basic IPC connection and calls
- `ipc/test-ipc-complete.js` - Full IPC feature demonstration