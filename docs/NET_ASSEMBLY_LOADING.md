# .NET Assembly Loading in EkkoJS

EkkoJS supports dynamic loading of .NET assemblies through the `dotnet:` protocol, allowing JavaScript code to seamlessly interact with .NET types and methods.

## Features

- Load .NET assemblies dynamically at runtime
- Call static methods on .NET types
- Create instances of .NET classes
- Access properties (getters and setters)
- Invoke instance methods
- Support for async methods (returns Promises)
- Type-safe method invocation with proper argument handling

## Usage

### Import a specific type
```javascript
import Calculator from 'dotnet:TestLibrary/Calculator';

// Use static methods
const result = Calculator.add(5, 3); // 8
```

### Import all exports from an assembly
```javascript
import TestLib from 'dotnet:TestLibrary';

// Access types through default export
const { Calculator, StringHelper, Constants } = TestLib.default;

// Use constants
console.log(Constants.version); // "1.0.0"

// Create instances
const helper = StringHelper.new('[PREFIX] ');
console.log(helper.format('Hello')); // "[PREFIX] Hello"
```

## Assembly Mapping Files

.NET assemblies must have a corresponding `.ekko.json` mapping file that describes the exported types and their members.

### Mapping File Format

```json
{
  "assembly": "path/to/Assembly.dll",
  "version": "1.0.0",
  "exports": {
    "ExportName": {
      "type": "Namespace.TypeName",
      "createInstance": true,
      "constructor": {
        "parameters": ["string", "int"]
      },
      "methods": {
        "jsMethodName": {
          "name": "ActualMethodName",
          "static": false,
          "async": true,
          "parameters": ["string"]
        }
      },
      "properties": {
        "jsPropertyName": {
          "name": "ActualPropertyName",
          "readonly": false,
          "static": false
        }
      },
      "fields": {
        "jsFieldName": "ActualFieldName"
      },
      "events": {
        "jsEventName": {
          "name": "ActualEventName",
          "eventArgs": "Namespace.EventArgsType"
        }
      }
    }
  }
}
```

### Mapping File Location

The assembly loader searches for mapping files in the following locations:
1. Current working directory: `AssemblyName.ekko.json`
2. Assemblies subdirectory: `assemblies/AssemblyName.ekko.json`
3. User profile: `~/.ekkojs/assemblies/AssemblyName.ekko.json`

## Implementation Details

### Module Loading Process

1. When importing `dotnet:AssemblyName/ExportName`, the loader:
   - Searches for `AssemblyName.ekko.json` mapping file
   - Loads the .NET assembly specified in the mapping
   - Creates a `DotNetTypeWrapper` for the requested export
   - Converts it to a JavaScript-friendly object

2. The JavaScript object exposes:
   - Static methods as regular functions
   - A `new()` function for creating instances (if `createInstance: true`)
   - Static properties and fields

3. Instance objects expose:
   - Instance methods as regular functions
   - Properties with proper getter/setter support
   - Event subscription/unsubscription support

### Type Conversion

- .NET primitive types are automatically converted to JavaScript equivalents
- Collections are converted to JavaScript arrays
- Tasks are converted to Promises
- Complex objects remain as wrapped .NET objects

### Method Invocation

Methods are wrapped to handle JavaScript's variadic argument passing:
- 0-3 parameters: Direct function signatures for optimal performance
- 4+ parameters: Array-based parameter passing

## Example: Complete Test

```javascript
// Test static methods
import Calculator from 'dotnet:TestLibrary/Calculator';
console.log('5 + 3 =', Calculator.add(5, 3));

// Test all exports
import TestLib from 'dotnet:TestLibrary';
const { StringHelper, Constants } = TestLib.default;

// Use constants
console.log('Version:', Constants.version);

// Create instance
const helper = StringHelper.new('[PREFIX] ');
console.log(helper.format('Hello')); // "[PREFIX] Hello"

// Use properties
helper.prefix = '[NEW] ';
console.log(helper.prefix); // "[NEW] "

// Async methods return promises
const result = await helper.processAsync('test');
console.log(result); // "[NEW] TEST"
```

## Limitations

- Generic types require explicit type parameters in the mapping file
- Extension methods are not directly supported (must be called as static methods)
- Operator overloading is not exposed
- Indexers are not currently supported

## Security Considerations

- Assemblies are loaded with full trust
- Only load assemblies from trusted sources
- Consider implementing assembly signing verification
- Mapping files should be protected from tampering