using System.Dynamic;
using System.Reflection;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace EkkoJS.Core.DotNet;


public class DotNetTypeWrapper : DynamicObject
{
    private readonly Type _type;
    private readonly ExportDefinition _definition;
    private readonly DotNetAssemblyLoader _loader;
    private V8ScriptEngine? _engine;

    public DotNetTypeWrapper(Type type, ExportDefinition definition, DotNetAssemblyLoader loader)
    {
        _type = type;
        _definition = definition;
        _loader = loader;
    }
    
    // Create a plain JavaScript object with all the members
    public object CreateJavaScriptObject(V8ScriptEngine engine)
    {
        // Store engine reference for later use
        _engine = engine;
        
        // Store this wrapper in engine for access
        engine.Script.__dotNetTypeWrapper = this;
        
        var objCode = @"
            (function() {
                const wrapper = globalThis.__dotNetTypeWrapper;
                const obj = {};
                
                // Helper to call .NET methods with proper argument handling
                function createMethodWrapper(methodName) {
                    return function(...args) {
                        // Call TryGetMember to get the delegate, then invoke it
                        const method = wrapper[methodName];
                        if (method && method.Invoke) {
                            return method.Invoke(args);
                        } else if (typeof method === 'function') {
                            return method(args);
                        }
                        throw new Error(`Method ${methodName} not found`);
                    };
                }
                
                return obj;
            })()
        ";
        
        dynamic jsObj = engine.Evaluate(objCode);
        
        // Add constructor if type supports instances  
        if (_definition.CreateInstance)
        {
            engine.Script.__jsObj = jsObj;
            engine.Execute(@"
                (function() {
                    const wrapper = globalThis.__dotNetTypeWrapper;
                    const obj = globalThis.__jsObj;
                    obj.new = function(...args) {
                        return wrapper.Call(args);
                    };
                })();
            ");
        }
        
        // Add static methods
        foreach (var (jsName, methodDef) in _definition.Methods)
        {
            if (methodDef.Static)
            {
                var methodInfo = _type.GetMethod(methodDef.Name, BindingFlags.Public | BindingFlags.Static);
                if (methodInfo != null)
                {
                    // Use ClearScript's parameter handling
                    var parameters = methodInfo.GetParameters();
                    
                    if (parameters.Length == 0)
                    {
                        // No parameters
                        jsObj[jsName] = new Func<object?>(() => methodInfo.Invoke(null, null));
                    }
                    else if (parameters.Length == 1)
                    {
                        // Single parameter
                        jsObj[jsName] = new Func<object?, object?>((arg1) => methodInfo.Invoke(null, new[] { arg1 }));
                    }
                    else if (parameters.Length == 2)
                    {
                        // Two parameters - this covers our Calculator methods
                        jsObj[jsName] = new Func<object?, object?, object?>((arg1, arg2) => 
                        {
                            try
                            {
                                return methodInfo.Invoke(null, new[] { arg1, arg2 });
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw ex.InnerException ?? ex;
                            }
                        });
                    }
                    else if (parameters.Length == 3)
                    {
                        // Three parameters
                        jsObj[jsName] = new Func<object?, object?, object?, object?>((arg1, arg2, arg3) => 
                            methodInfo.Invoke(null, new[] { arg1, arg2, arg3 }));
                    }
                    else
                    {
                        // For more parameters, use params array
                        jsObj[jsName] = new Func<object?[], object?>((args) => methodInfo.Invoke(null, args));
                    }
                }
            }
        }
        
        // Add static properties
        foreach (var (jsName, propDef) in _definition.Properties)
        {
            if (propDef.Static)
            {
                var propInfo = _type.GetProperty(propDef.Name, BindingFlags.Public | BindingFlags.Static);
                if (propInfo != null && propInfo.CanRead)
                {
                    jsObj[jsName] = propInfo.GetValue(null);
                }
            }
        }
        
        // Add static fields
        foreach (var (jsName, fieldName) in _definition.Fields)
        {
            var fieldInfo = _type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo != null)
            {
                jsObj[jsName] = fieldInfo.GetValue(null);
            }
        }
        
        // Clean up
        engine.Script.__dotNetTypeWrapper = null;
        engine.Script.__methodName = null;
        engine.Script.__jsObj = null;
        engine.Script.__methodDelegate = null;
        engine.Script.__methodInfo = null;
        engine.Script.__jsName = null;
        
        return jsObj;
    }
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var memberName = binder.Name;

        // Special handling for constructor
        if (memberName == "new" && _definition.CreateInstance)
        {
            result = new Func<object[], object>((args) =>
            {
                var instance = Activator.CreateInstance(_type, args) 
                    ?? throw new InvalidOperationException($"Failed to create instance of {_type.Name}");
                return new DotNetInstanceWrapper(instance, _type, _definition, _loader);
            });
            return true;
        }

        // Check methods
        if (_definition.Methods.TryGetValue(memberName, out var methodDef))
        {
            if (methodDef.Static)
            {
                result = CreateMethodDelegate(_type, methodDef, null);
                return true;
            }
        }

        // Check properties
        if (_definition.Properties.TryGetValue(memberName, out var propDef))
        {
            if (propDef.Static)
            {
                var propInfo = _type.GetProperty(propDef.Name, BindingFlags.Public | BindingFlags.Static);
                if (propInfo != null)
                {
                    result = propInfo.GetValue(null);
                    return true;
                }
            }
        }

        // Check fields
        if (_definition.Fields.TryGetValue(memberName, out var fieldName))
        {
            var fieldInfo = _type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo != null)
            {
                result = fieldInfo.GetValue(null);
                return true;
            }
        }

        result = null;
        return false;
    }

    // Make this callable as a constructor from JavaScript
    public object Call(object args)
    {
        if (!_definition.CreateInstance)
        {
            throw new InvalidOperationException($"Type {_type.Name} does not support instance creation");
        }


        // Handle the arguments - convert V8Array to regular array
        object?[]? ctorArgs = null;
        if (args is System.Collections.IEnumerable argsEnum && !(args is string))
        {
            var argsList = new List<object?>();
            foreach (var item in argsEnum)
            {
                argsList.Add(item);
            }
            ctorArgs = argsList.ToArray();
        }
        else if (args != null)
        {
            ctorArgs = new[] { args };
        }

        var instance = Activator.CreateInstance(_type, ctorArgs) 
            ?? throw new InvalidOperationException($"Failed to create instance of {_type.Name}");
        
        // Return a new wrapper for the instance
        var wrapper = new DotNetInstanceWrapper(instance, _type, _definition, _loader);
        
        // If we have access to the engine, create a JavaScript-friendly object
        if (_engine != null)
        {
            return wrapper.CreateJavaScriptObject(_engine);
        }
        
        return wrapper;
    }

    private static Delegate CreateMethodDelegate(Type type, MethodDefinition methodDef, object? instance)
    {
        var methodInfo = type.GetMethod(methodDef.Name, 
            instance == null ? BindingFlags.Public | BindingFlags.Static : BindingFlags.Public | BindingFlags.Instance);
        
        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method not found: {methodDef.Name} on type {type.Name}");
        }

        // Create a simple delegate
        return new Func<object?[], object?>((args) =>
        {
            try
            {
                var result = methodInfo.Invoke(instance, args);
                
                // Handle async methods
                if (methodDef.Async && result is Task task)
                {
                    return task;
                }
                
                return result;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        });
    }
}

public class DotNetInstanceWrapper : DynamicObject
{
    private readonly object _instance;
    private readonly Type _type;
    private readonly ExportDefinition _definition;
    private readonly DotNetAssemblyLoader _loader;
    private readonly Dictionary<string, Delegate> _eventHandlers = new();

    public DotNetInstanceWrapper(object instance, Type type, ExportDefinition definition, DotNetAssemblyLoader loader)
    {
        _instance = instance;
        _type = type;
        _definition = definition;
        _loader = loader;
    }
    
    public object CreateJavaScriptObject(V8ScriptEngine engine)
    {
        // Create a plain JavaScript object for the instance
        var objCode = @"
            (function() {
                const obj = {};
                return obj;
            })()
        ";
        
        dynamic jsObj = engine.Evaluate(objCode);
        
        // Add instance methods
        foreach (var (jsName, methodDef) in _definition.Methods)
        {
            if (!methodDef.Static)
            {
                var methodInfo = _type.GetMethod(methodDef.Name, BindingFlags.Public | BindingFlags.Instance);
                if (methodInfo != null)
                {
                    var parameters = methodInfo.GetParameters();
                    
                    if (parameters.Length == 0)
                    {
                        jsObj[jsName] = new Func<object?>(() => methodInfo.Invoke(_instance, null));
                    }
                    else if (parameters.Length == 1)
                    {
                        jsObj[jsName] = new Func<object?, object?>((arg1) => methodInfo.Invoke(_instance, new[] { arg1 }));
                    }
                    else if (parameters.Length == 2)
                    {
                        jsObj[jsName] = new Func<object?, object?, object?>((arg1, arg2) => 
                            methodInfo.Invoke(_instance, new[] { arg1, arg2 }));
                    }
                    else
                    {
                        jsObj[jsName] = new Func<object?[], object?>((args) => methodInfo.Invoke(_instance, args));
                    }
                }
            }
        }
        
        // Add instance properties
        foreach (var (jsName, propDef) in _definition.Properties)
        {
            if (!propDef.Static)
            {
                var propInfo = _type.GetProperty(propDef.Name, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                {
                    // Create getter/setter using Object.defineProperty
                    engine.Script.__jsObj = jsObj;
                    engine.Script.__instance = _instance;
                    engine.Script.__propInfo = propInfo;
                    engine.Script.__jsName = jsName;
                    
                    engine.Execute(@"
                        (function() {
                            const obj = globalThis.__jsObj;
                            const instance = globalThis.__instance;
                            const propInfo = globalThis.__propInfo;
                            const name = globalThis.__jsName;
                            
                            Object.defineProperty(obj, name, {
                                get() {
                                    return propInfo.GetValue(instance);
                                },
                                set(value) {
                                    if (propInfo.CanWrite) {
                                        propInfo.SetValue(instance, value);
                                    }
                                },
                                enumerable: true,
                                configurable: true
                            });
                        })();
                    ");
                }
            }
        }
        
        // Clean up
        engine.Script.__jsObj = null;
        engine.Script.__instance = null;
        engine.Script.__propInfo = null;
        engine.Script.__jsName = null;
        
        return jsObj;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var memberName = binder.Name;

        // Check methods
        if (_definition.Methods.TryGetValue(memberName, out var methodDef))
        {
            result = CreateMethodDelegate(_type, methodDef, _instance);
            return true;
        }

        // Check properties
        if (_definition.Properties.TryGetValue(memberName, out var propDef))
        {
            var propInfo = _type.GetProperty(propDef.Name, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null && propInfo.CanRead)
            {
                result = propInfo.GetValue(_instance);
                return true;
            }
        }

        // Check events (getter returns current handler)
        if (_definition.Events.TryGetValue(memberName, out var eventDef))
        {
            _eventHandlers.TryGetValue(memberName, out var handler);
            result = handler;
            return true;
        }

        result = null;
        return false;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        var memberName = binder.Name;

        // Check properties
        if (_definition.Properties.TryGetValue(memberName, out var propDef))
        {
            if (!propDef.ReadOnly)
            {
                var propInfo = _type.GetProperty(propDef.Name, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null && propInfo.CanWrite)
                {
                    propInfo.SetValue(_instance, value);
                    return true;
                }
            }
        }

        // Check events (setter subscribes/unsubscribes)
        if (_definition.Events.TryGetValue(memberName, out var eventDef))
        {
            var eventInfo = _type.GetEvent(eventDef.Name, BindingFlags.Public | BindingFlags.Instance);
            if (eventInfo != null)
            {
                // Unsubscribe previous handler if any
                if (_eventHandlers.TryGetValue(memberName, out var oldHandler))
                {
                    eventInfo.RemoveEventHandler(_instance, oldHandler);
                    _eventHandlers.Remove(memberName);
                }

                // Subscribe new handler if provided
                if (value is Delegate newHandler)
                {
                    // Create a wrapper that converts EventArgs to JavaScript-friendly format
                    var wrapper = CreateEventHandlerWrapper(eventInfo, newHandler);
                    eventInfo.AddEventHandler(_instance, wrapper);
                    _eventHandlers[memberName] = wrapper;
                }
                
                return true;
            }
        }

        return false;
    }

    private static Delegate CreateMethodDelegate(Type type, MethodDefinition methodDef, object? instance)
    {
        var methodInfo = type.GetMethod(methodDef.Name, BindingFlags.Public | BindingFlags.Instance);
        
        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method not found: {methodDef.Name} on type {type.Name}");
        }

        // Create a simple delegate
        return new Func<object?[], object?>((args) =>
        {
            try
            {
                var result = methodInfo.Invoke(instance, args);
                
                // Handle async methods
                if (methodDef.Async && result is Task task)
                {
                    return task;
                }
                
                return result;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        });
    }

    private static Delegate CreateEventHandlerWrapper(EventInfo eventInfo, Delegate jsHandler)
    {
        var eventHandlerType = eventInfo.EventHandlerType!;
        var invokeMethod = eventHandlerType.GetMethod("Invoke")!;
        var parameters = invokeMethod.GetParameters();

        // Create a dynamic event handler
        if (parameters.Length == 2 && parameters[1].ParameterType.IsSubclassOf(typeof(EventArgs)))
        {
            // Standard EventHandler pattern
            return Delegate.CreateDelegate(eventHandlerType, jsHandler.Target, jsHandler.Method);
        }
        else
        {
            // Custom delegate type - create a wrapper
            var wrapper = new Action<object, object>((sender, args) =>
            {
                jsHandler.DynamicInvoke(sender, args);
            });
            
            return Delegate.CreateDelegate(eventHandlerType, wrapper.Target, wrapper.Method);
        }
    }
}