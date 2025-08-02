# Error Handling Strategy for .NET/JavaScript Interop

## Overview

When .NET libraries are exposed to JavaScript, errors must be transformed to appear as native JavaScript errors, hiding .NET implementation details and stack traces.

## Error Transformation Layer

### 1. Error Interceptor Pattern

```csharp
public class JavaScriptErrorBoundary
{
    public static object WrapCall(Func<object> dotNetCall, string moduleName, string methodName)
    {
        try
        {
            return dotNetCall();
        }
        catch (Exception ex)
        {
            throw TransformToJavaScriptError(ex, moduleName, methodName);
        }
    }
    
    public static async Task<object> WrapCallAsync(Func<Task<object>> dotNetCall, string moduleName, string methodName)
    {
        try
        {
            return await dotNetCall();
        }
        catch (Exception ex)
        {
            throw TransformToJavaScriptError(ex, moduleName, methodName);
        }
    }
    
    private static JavaScriptException TransformToJavaScriptError(Exception ex, string moduleName, string methodName)
    {
        // Map common .NET exceptions to JavaScript error types
        var jsError = ex switch
        {
            ArgumentNullException => new JavaScriptException(
                "TypeError",
                $"{methodName}: Required argument is null or undefined",
                "ERR_INVALID_ARG_TYPE"
            ),
            
            ArgumentException argEx => new JavaScriptException(
                "TypeError", 
                $"{methodName}: {SimplifyMessage(argEx.Message)}",
                "ERR_INVALID_ARG_VALUE"
            ),
            
            InvalidOperationException => new JavaScriptException(
                "Error",
                $"{methodName}: Operation is not valid in current state",
                "ERR_INVALID_STATE"
            ),
            
            NotSupportedException => new JavaScriptException(
                "Error",
                $"{methodName}: Operation is not supported",
                "ERR_NOT_SUPPORTED"
            ),
            
            FileNotFoundException fnfEx => new JavaScriptException(
                "Error",
                $"{methodName}: File not found: {fnfEx.FileName}",
                "ENOENT"
            ),
            
            UnauthorizedAccessException => new JavaScriptException(
                "Error",
                $"{methodName}: Permission denied",
                "EACCES"
            ),
            
            OutOfMemoryException => new JavaScriptException(
                "RangeError",
                "Out of memory",
                "ERR_OUT_OF_MEMORY"
            ),
            
            _ => new JavaScriptException(
                "Error",
                $"{methodName}: {SimplifyMessage(ex.Message)}",
                "ERR_INTERNAL"
            )
        };
        
        // Add JavaScript-style stack trace
        jsError.SetJavaScriptStack(moduleName, methodName, ex);
        
        return jsError;
    }
    
    private static string SimplifyMessage(string dotNetMessage)
    {
        // Remove .NET-specific details
        return dotNetMessage
            .Replace("System.", "")
            .Replace("Microsoft.", "")
            .Replace("Parameter name:", "Argument:")
            .Trim();
    }
}
```

### 2. JavaScript Exception Class

```csharp
public class JavaScriptException : Exception
{
    public string ErrorType { get; }
    public string Code { get; }
    public string JavaScriptStack { get; private set; }
    
    public JavaScriptException(string errorType, string message, string code) 
        : base(message)
    {
        ErrorType = errorType;
        Code = code;
    }
    
    public void SetJavaScriptStack(string moduleName, string methodName, Exception originalException)
    {
        var sb = new StringBuilder();
        
        // JavaScript-style error header
        sb.AppendLine($"{ErrorType}: {Message}");
        
        // Simplified stack trace
        sb.AppendLine($"    at {moduleName}.{methodName} (native)");
        
        // Add calling JavaScript context if available
        if (V8Context.Current?.CallStack != null)
        {
            foreach (var frame in V8Context.Current.CallStack)
            {
                sb.AppendLine($"    at {frame}");
            }
        }
        
        JavaScriptStack = sb.ToString();
        
        // Log full .NET stack trace for debugging (not exposed to JS)
        LogInternalError(originalException);
    }
    
    private void LogInternalError(Exception ex)
    {
        // Log to internal diagnostics, not visible to JavaScript
        EkkoLogger.LogError($"Internal .NET error in interop: {ex}");
    }
}
```

### 3. Module Wrapper Implementation

```csharp
public class DotNetModuleWrapper
{
    private readonly Type _targetType;
    private readonly string _moduleName;
    
    public object CreateJavaScriptProxy()
    {
        var proxy = new ExpandoObject();
        var dict = (IDictionary<string, object>)proxy;
        
        foreach (var method in _targetType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var methodName = method.Name;
            var capturedMethod = method;
            
            if (IsAsyncMethod(method))
            {
                dict[methodName] = new Func<object[], Task<object>>(async (args) =>
                {
                    return await JavaScriptErrorBoundary.WrapCallAsync(
                        async () => {
                            var result = capturedMethod.Invoke(null, args);
                            if (result is Task task)
                            {
                                await task;
                                var resultProperty = task.GetType().GetProperty("Result");
                                return resultProperty?.GetValue(task);
                            }
                            return result;
                        },
                        _moduleName,
                        methodName
                    );
                });
            }
            else
            {
                dict[methodName] = new Func<object[], object>((args) =>
                {
                    return JavaScriptErrorBoundary.WrapCall(
                        () => capturedMethod.Invoke(null, args),
                        _moduleName,
                        methodName
                    );
                });
            }
        }
        
        return proxy;
    }
}
```

### 4. Built-in Module Error Handling

For built-in modules like `ekko:fs`, wrap all operations:

```csharp
public class FileSystemModule
{
    public object readFileSync(string path, object options = null)
    {
        return JavaScriptErrorBoundary.WrapCall(() =>
        {
            // Validate JavaScript arguments first
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            
            // Perform operation
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found", path);
            }
            
            return File.ReadAllText(path);
        }, 
        "fs", 
        "readFileSync");
    }
    
    public Task<object> readFile(string path, object options = null)
    {
        return JavaScriptErrorBoundary.WrapCallAsync(async () =>
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found", path);
            }
            
            return await File.ReadAllTextAsync(path);
        },
        "fs",
        "readFile");
    }
}
```

## Error Display in JavaScript

### What JavaScript Developers See:

```javascript
import fs from 'ekko:fs';

try {
    const content = fs.readFileSync('');  // Empty path
} catch (error) {
    console.error(error);
    // Output:
    // TypeError: readFileSync: Required argument is null or undefined
    //     at fs.readFileSync (native)
    //     at main.js:4:22
    
    console.log(error.code);  // "ERR_INVALID_ARG_TYPE"
}

try {
    const data = await fetch('https://api.example.com/data');
} catch (error) {
    console.error(error);
    // Output:
    // Error: fetch: Network request failed
    //     at fetch (native)
    //     at async processData (app.js:15:18)
    
    console.log(error.code);  // "ENETWORK"
}
```

### What Gets Hidden:

```csharp
// Original .NET stack trace (hidden from JavaScript):
System.ArgumentNullException: Value cannot be null. (Parameter 'path')
   at System.IO.File.ReadAllText(String path)
   at EkkoJS.Core.Modules.FileSystemModule.readFileSync(String path, Object options) in C:\ekkojs\FileSystemModule.cs:line 45
   at System.Dynamic.UpdateDelegates.UpdateAndExecute2[T0,T1,TRet](CallSite site, T0 arg0, T1 arg1)
   // ... rest of .NET stack trace
```

## Error Code Standards

Use Node.js-compatible error codes where possible:

```csharp
public static class JavaScriptErrorCodes
{
    // System errors
    public const string EACCES = "EACCES";           // Permission denied
    public const string EEXIST = "EEXIST";           // File exists
    public const string ENOENT = "ENOENT";           // No such file or directory
    public const string ENOTDIR = "ENOTDIR";         // Not a directory
    public const string EISDIR = "EISDIR";           // Is a directory
    public const string EMFILE = "EMFILE";           // Too many open files
    public const string ENOSPC = "ENOSPC";           // No space left on device
    
    // Argument errors
    public const string ERR_INVALID_ARG_TYPE = "ERR_INVALID_ARG_TYPE";
    public const string ERR_INVALID_ARG_VALUE = "ERR_INVALID_ARG_VALUE";
    public const string ERR_MISSING_ARGS = "ERR_MISSING_ARGS";
    public const string ERR_OUT_OF_RANGE = "ERR_OUT_OF_RANGE";
    
    // Async errors
    public const string ERR_INVALID_CALLBACK = "ERR_INVALID_CALLBACK";
    public const string ERR_MULTIPLE_CALLBACK = "ERR_MULTIPLE_CALLBACK";
    
    // Module errors
    public const string ERR_MODULE_NOT_FOUND = "ERR_MODULE_NOT_FOUND";
    public const string ERR_INVALID_MODULE_SPECIFIER = "ERR_INVALID_MODULE_SPECIFIER";
}
```

## Integration with V8 Engine

```csharp
public class V8ErrorIntegration
{
    public static void ConfigureErrorHandling(V8ScriptEngine engine)
    {
        // Override global error constructors to handle .NET errors
        engine.Execute(@"
            const OriginalError = Error;
            
            class EkkoError extends OriginalError {
                constructor(message, code) {
                    super(message);
                    this.code = code;
                    
                    // Ensure stack trace uses JavaScript format
                    if (Error.captureStackTrace) {
                        Error.captureStackTrace(this, this.constructor);
                    }
                }
            }
            
            // Make available globally
            globalThis.EkkoError = EkkoError;
        ");
        
        // Set up error event handlers
        engine.Script.process = new {
            on = new Action<string, Action<dynamic>>((eventName, handler) => {
                if (eventName == "uncaughtException") {
                    AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                        var jsError = TransformException(e.ExceptionObject as Exception);
                        handler(jsError);
                    };
                }
            })
        };
    }
}
```

## Best Practices

1. **Always wrap .NET calls** in error boundaries
2. **Use JavaScript error types**: TypeError, RangeError, Error
3. **Provide meaningful error codes** compatible with Node.js
4. **Simplify error messages** removing .NET jargon
5. **Log full details internally** for debugging
6. **Test error scenarios** to ensure proper transformation
7. **Document expected errors** in API documentation

## Testing Error Handling

```javascript
// test-error-handling.js
import test from 'ekko:test';
import fs from 'ekko:fs';

test('should throw TypeError for invalid arguments', () => {
    expect(() => fs.readFileSync()).toThrow(TypeError);
    expect(() => fs.readFileSync(null)).toThrow(TypeError);
    expect(() => fs.readFileSync(123)).toThrow(TypeError);
});

test('should throw with correct error code', () => {
    try {
        fs.readFileSync('/path/does/not/exist');
    } catch (error) {
        expect(error.code).toBe('ENOENT');
        expect(error.message).toContain('File not found');
    }
});

test('should have JavaScript stack trace', () => {
    try {
        fs.readFileSync('');
    } catch (error) {
        expect(error.stack).toContain('at fs.readFileSync (native)');
        expect(error.stack).not.toContain('System.');
        expect(error.stack).not.toContain('.cs:line');
    }
});
```