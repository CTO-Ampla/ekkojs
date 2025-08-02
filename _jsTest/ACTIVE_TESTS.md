# Active Test Files

This document lists the test files that are actively used and maintained for testing EkkoJS functionality.

## Core Runtime Tests
- `simple-test.js` - Basic runtime verification
- `test.js` - General runtime functionality
- `timer-test.js` - Timer and async operations

## Module System Tests  
- `modules/test-es-simple.js` - Basic ES module imports
- `modules/test-dynamic-import.js` - Dynamic import() functionality

## TypeScript Tests
- `typescript/hello.ts` - Basic TypeScript execution
- `typescript/test-imports.ts` - TypeScript with module imports

## .NET Integration Tests
- `dotnet/test-assembly.js` - Basic .NET assembly loading with `dotnet:TestLibrary`
- `dotnet/test-instance.js` - .NET object instantiation and method calls

## Native Library Tests
- `native/test-mathlib.js` - Basic native function calls
- `native/test-mathlib-struct.js` - Struct parameter passing
- `native/test-mathlib-string.js` - String return values

## IPC Communication Tests
- `ipc/test-ipc-simple.js` - Basic IPC connection and method calls
- `ipc/test-ipc-complete.js` - Full IPC functionality demonstration

## Running Tests

```bash
# From project root
cd /mnt/d/git/ekkojs

# Run a simple test
dotnet run --project EkkoJS.CLI -- run _jsTest/simple-test.js

# Run TypeScript
dotnet run --project EkkoJS.CLI -- run _jsTest/typescript/hello.ts

# Run IPC test (requires service running)
dotnet run --project EkkoJS.Demo.IpcService &
dotnet run --project EkkoJS.CLI -- run _jsTest/ipc/test-ipc-simple.js
```