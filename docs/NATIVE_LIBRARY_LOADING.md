# Native Library Loading in EkkoJS

EkkoJS supports loading native C/C++ libraries through the `native:` protocol, with automatic P/Invoke wrapper generation based on JSON mapping files.

## Features

- Cross-platform native library loading (Windows, Linux, macOS)
- Dynamic P/Invoke wrapper generation at runtime
- Support for multiple architectures (x86, x64, ARM64)
- Type-safe function calls with automatic marshaling
- Struct support with field mapping
- Array and string handling
- Callback support (planned)

## Usage

### Import a native library
```javascript
import mathlib from 'native:mathlib';

// Call native functions
const result = mathlib.add(5, 3); // 8
const version = mathlib.getVersion(); // "MathLib v1.0.0"

// Work with structs
const point = new mathlib.Point({ x: 10, y: 20 });
mathlib.translatePoint(point, 5, -5);

// Array operations
const sum = mathlib.sumArray([1, 2, 3, 4, 5], 5); // 15
```

## Native Library Mapping Files

Native libraries must have a corresponding `.ekko.json` mapping file that describes the exported functions, structs, and types.

### Mapping File Format

```json
{
  "library": {
    "windows": {
      "x64": "mathlib.dll",
      "x86": "mathlib32.dll"
    },
    "linux": {
      "x64": "libmathlib.so",
      "x86": "libmathlib32.so",
      "arm64": "libmathlib-arm64.so"
    },
    "darwin": {
      "x64": "libmathlib.dylib",
      "arm64": "libmathlib-arm64.dylib"
    }
  },
  "version": "1.0.0",
  "exports": {
    "functionName": {
      "entryPoint": "native_function_name",
      "returns": "int",
      "parameters": [
        { "name": "param1", "type": "int" },
        { "name": "param2", "type": "double" }
      ],
      "callingConvention": "cdecl"
    }
  },
  "structs": {
    "Point": {
      "fields": [
        { "name": "x", "type": "double" },
        { "name": "y", "type": "double" }
      ],
      "layout": "Sequential"
    }
  },
  "callbacks": {
    "CallbackType": {
      "returns": "void",
      "parameters": [
        { "name": "value", "type": "int" }
      ]
    }
  }
}
```

### Supported Types

- **Primitive types**: `bool`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`
- **String types**: `string` (marshaled as ANSI by default)
- **Pointer types**: `IntPtr`, `UIntPtr`
- **Arrays**: Any type followed by `[]` (e.g., `int[]`, `double[]`)
- **Structs**: Custom struct types defined in the `structs` section
- **Void**: `void` for functions with no return value

### Parameter Modifiers

- `ref`: Pass by reference (in/out parameter)
- `out`: Output parameter only

### Calling Conventions

- `cdecl`: C declaration (default)
- `stdcall`: Standard call
- `fastcall`: Fast call

## Implementation Details

### Dynamic P/Invoke Generation

1. At runtime, EkkoJS reads the mapping file
2. Creates struct types using `System.Reflection.Emit`
3. Generates a wrapper class with P/Invoke methods
4. Creates JavaScript-friendly wrapper functions

### Platform Detection

The loader automatically detects:
- Operating System (Windows, Linux, macOS)
- Architecture (x86, x64, ARM64)
- Selects the appropriate native library from the mapping

### Type Marshaling

- Strings are marshaled as ANSI by default
- Arrays are passed as pointers with separate size parameters
- Structs are passed by value or reference based on parameter modifiers
- Callbacks are converted to function pointers

## Example: Complete Native Library

### C Header (mathlib.h)
```c
#ifdef _WIN32
    #define MATHLIB_API __declspec(dllexport)
#else
    #define MATHLIB_API __attribute__((visibility("default")))
#endif

extern "C" {
    MATHLIB_API int add(int a, int b);
    MATHLIB_API const char* get_version();
    
    typedef struct {
        double x;
        double y;
    } Point;
    
    MATHLIB_API double distance(Point* p1, Point* p2);
}
```

### Mapping File (mathlib.ekko.json)
```json
{
  "library": {
    "windows": { "x64": "mathlib.dll" },
    "linux": { "x64": "libmathlib.so" },
    "darwin": { "x64": "libmathlib.dylib" }
  },
  "exports": {
    "add": {
      "entryPoint": "add",
      "returns": "int",
      "parameters": [
        { "name": "a", "type": "int" },
        { "name": "b", "type": "int" }
      ]
    },
    "getVersion": {
      "entryPoint": "get_version",
      "returns": "string",
      "parameters": []
    },
    "distance": {
      "entryPoint": "distance",
      "returns": "double",
      "parameters": [
        { "name": "p1", "type": "Point", "ref": true },
        { "name": "p2", "type": "Point", "ref": true }
      ]
    }
  },
  "structs": {
    "Point": {
      "fields": [
        { "name": "x", "type": "double" },
        { "name": "y", "type": "double" }
      ]
    }
  }
}
```

### JavaScript Usage
```javascript
import mathlib from 'native:mathlib';

// Simple function call
console.log(mathlib.add(5, 3)); // 8
console.log(mathlib.getVersion()); // "MathLib v1.0.0"

// Using structs
const p1 = new mathlib.Point({ x: 0, y: 0 });
const p2 = new mathlib.Point({ x: 3, y: 4 });
console.log(mathlib.distance(p1, p2)); // 5.0
```

## Building Native Libraries

### Linux
```bash
gcc -shared -fPIC -o libmathlib.so mathlib.c -lm
```

### Windows
```bash
gcc -shared -o mathlib.dll mathlib.c -lm
```

### macOS
```bash
gcc -dynamiclib -o libmathlib.dylib mathlib.c -lm
```

### Cross-compilation (Linux to Windows)
```bash
x86_64-w64-mingw32-gcc -shared -o mathlib.dll mathlib.c -lm
```

## Limitations

- No support for C++ classes (only C-style functions and structs)
- Callbacks are function pointers only (no closures)
- Limited string encoding options (ANSI by default)
- No support for variable argument lists
- Manual memory management required for complex scenarios

## Security Considerations

- Native libraries run with full process permissions
- No sandboxing or isolation
- Verify library signatures before loading
- Be cautious with string and array operations to prevent buffer overflows
- Validate all inputs before passing to native functions