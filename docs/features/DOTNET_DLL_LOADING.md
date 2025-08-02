# .NET DLL Dynamic Loading

## Overview

EkkoJS supports loading .NET assemblies dynamically through the `dotnet:` protocol. This allows JavaScript code to interact with .NET libraries seamlessly, with automatic type marshalling between JavaScript and .NET.

## Mapping File Format

Each .NET assembly requires a mapping file (`assembly.ekko.json`) that describes how to expose .NET types to JavaScript.

### Example Mapping File

```json
{
  "assembly": "MyLibrary.dll",
  "version": "1.0.0",
  "exports": {
    "Calculator": {
      "type": "MyLibrary.Calculator",
      "methods": {
        "add": {
          "name": "Add",
          "static": true
        },
        "subtract": {
          "name": "Subtract",
          "static": true
        }
      }
    },
    "FileHelper": {
      "type": "MyLibrary.IO.FileHelper",
      "createInstance": true,
      "constructor": {
        "parameters": ["string"]
      },
      "methods": {
        "readJson": {
          "name": "ReadJsonFile",
          "async": true
        },
        "writeJson": {
          "name": "WriteJsonFile",
          "parameters": ["string", "object"]
        }
      },
      "properties": {
        "basePath": {
          "name": "BasePath",
          "readonly": false
        }
      }
    },
    "Constants": {
      "type": "MyLibrary.Constants",
      "fields": {
        "version": "VERSION",
        "maxSize": "MAX_FILE_SIZE"
      }
    }
  },
  "typeAliases": {
    "JsonData": "System.Collections.Generic.Dictionary`2[System.String,System.Object]"
  }
}
```

## Usage

### Importing .NET Assemblies

```javascript
// Import using dotnet: protocol
import Calculator from 'dotnet:MyLibrary/Calculator';
import { FileHelper, Constants } from 'dotnet:MyLibrary';

// Static method calls
const sum = Calculator.add(5, 3);
console.log('Sum:', sum); // 8

// Creating instances
const fileHelper = new FileHelper('./data');
const data = await fileHelper.readJson('config.json');

// Accessing properties
console.log('Base path:', fileHelper.basePath);
fileHelper.basePath = './newdata';

// Using constants
console.log('Version:', Constants.version);
console.log('Max size:', Constants.maxSize);
```

## Mapping File Schema

### Root Properties

- `assembly`: Path to the .NET DLL file (relative to mapping file)
- `version`: Version of the mapping (for compatibility)
- `exports`: Object defining all exported types
- `typeAliases`: Optional type name mappings

### Export Definition

Each export can contain:
- `type`: Fully qualified .NET type name
- `createInstance`: Whether to allow `new` operator
- `constructor`: Constructor configuration
- `methods`: Method mappings
- `properties`: Property mappings
- `fields`: Static field mappings
- `events`: Event mappings (optional)

### Method Definition

- `name`: .NET method name
- `static`: Whether the method is static
- `async`: Whether to return a Promise
- `parameters`: Optional parameter type hints
- `generic`: Generic type parameters (optional)

### Property Definition

- `name`: .NET property name
- `readonly`: Whether property is read-only
- `static`: Whether property is static

## Type Marshalling

### Automatic Type Conversion

| JavaScript Type | .NET Type |
|----------------|-----------|
| number | int, double, float, decimal |
| string | string |
| boolean | bool |
| Date | DateTime |
| Array | T[], List<T>, IEnumerable<T> |
| Object | Dictionary<string, object>, custom classes |
| null/undefined | null |
| Promise | Task<T> |

### Custom Type Converters

Mapping files can specify custom converters:

```json
{
  "converters": {
    "MyCustomType": {
      "toJs": "ConvertToJavaScript",
      "fromJs": "ConvertFromJavaScript"
    }
  }
}
```

## Assembly Resolution

1. **Local Directory**: First checks the directory containing the mapping file
2. **Assembly Cache**: Checks `~/.ekkojs/assemblies/`
3. **NuGet Cache**: Checks local NuGet package cache
4. **Project References**: Checks project bin directories

## Security Considerations

- Assemblies are loaded in a restricted AppDomain
- Only explicitly exported types are accessible
- Reflection permissions are limited
- File system access follows EkkoJS sandbox rules

## Advanced Features

### Generic Methods

```json
{
  "methods": {
    "parse": {
      "name": "Parse",
      "generic": ["T"],
      "parameters": ["string"]
    }
  }
}
```

Usage:
```javascript
const result = MyParser.parse<number>("123");
```

### Events

```json
{
  "events": {
    "onProgress": {
      "name": "ProgressChanged",
      "eventArgs": "ProgressChangedEventArgs"
    }
  }
}
```

Usage:
```javascript
fileHelper.onProgress = (sender, args) => {
  console.log(`Progress: ${args.percentage}%`);
};
```

### Extension Methods

```json
{
  "extensions": {
    "StringExtensions": {
      "type": "MyLibrary.Extensions.StringExtensions",
      "extends": "System.String"
    }
  }
}
```