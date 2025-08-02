using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.ClearScript.V8;

namespace EkkoJS.Core.Native;

public class NativeLibraryLoader
{
    private readonly Dictionary<string, LoadedNativeLibrary> _loadedLibraries = new();
    private readonly V8ScriptEngine _engine;
    private readonly ModuleBuilder _moduleBuilder;
    private int _typeCounter = 0;

    public NativeLibraryLoader(V8ScriptEngine engine)
    {
        _engine = engine;
        
        // Create dynamic assembly for P/Invoke wrappers
        var assemblyName = new AssemblyName("EkkoJS.Native.Dynamic");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        _moduleBuilder = assemblyBuilder.DefineDynamicModule("NativeWrappers");
    }

    public object LoadLibrary(string libraryName)
    {
        Console.WriteLine($"[NativeLoader] Loading library: {libraryName}");
        
        if (_loadedLibraries.TryGetValue(libraryName, out var loaded))
        {
            Console.WriteLine($"[NativeLoader] Library already loaded: {libraryName}");
            return loaded.Exports;
        }

        // Find and load mapping file
        var mappingPath = FindMappingFile(libraryName);
        Console.WriteLine($"[NativeLoader] Looking for mapping file...");
        if (mappingPath == null)
        {
            throw new FileNotFoundException($"Mapping file not found for library: {libraryName}");
        }
        Console.WriteLine($"[NativeLoader] Found mapping file: {mappingPath}");

        var json = File.ReadAllText(mappingPath);
        Console.WriteLine($"[NativeLoader] Parsing mapping file...");
        var mapping = JsonSerializer.Deserialize<NativeLibraryMapping>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException($"Failed to parse mapping file: {mappingPath}");

        // Determine library path based on platform and architecture
        Console.WriteLine($"[NativeLoader] Determining library path...");
        var libraryPath = GetLibraryPath(mapping, Path.GetDirectoryName(mappingPath) ?? "");
        Console.WriteLine($"[NativeLoader] Library path: {libraryPath}");
        if (!File.Exists(libraryPath))
        {
            throw new FileNotFoundException($"Native library not found: {libraryPath}");
        }

        // Create struct types
        Console.WriteLine($"[NativeLoader] Creating struct types...");
        var structTypes = CreateStructTypes(mapping.Structs);

        // Create wrapper type with P/Invoke methods
        Console.WriteLine($"[NativeLoader] Creating P/Invoke wrapper type...");
        var wrapperType = CreateWrapperType(libraryName, libraryPath, mapping, structTypes);

        // Create JavaScript exports object
        Console.WriteLine($"[NativeLoader] Creating JavaScript exports...");
        var exports = CreateExportsObject(wrapperType, mapping, structTypes);

        _loadedLibraries[libraryName] = new LoadedNativeLibrary
        {
            Mapping = mapping,
            WrapperType = wrapperType,
            Exports = exports,
            LibraryPath = libraryPath
        };

        return exports;
    }

    private string? FindMappingFile(string libraryName)
    {
        var searchPaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), $"{libraryName}.ekko.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "_jsTest", "native-libraries", libraryName, $"{libraryName}.ekko.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "native-libraries", libraryName, $"{libraryName}.ekko.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                ".ekkojs", "native-libraries", $"{libraryName}.ekko.json")
        };

        return searchPaths.FirstOrDefault(File.Exists);
    }

    private string GetLibraryPath(NativeLibraryMapping mapping, string basePath)
    {
        var platform = GetPlatformName();
        var architecture = GetArchitecture();

        Dictionary<string, string>? platformLibs = platform switch
        {
            "windows" => mapping.Library.Windows,
            "linux" => mapping.Library.Linux,
            "darwin" => mapping.Library.Darwin,
            _ => null
        };

        if (platformLibs == null || !platformLibs.TryGetValue(architecture, out var libraryFile))
        {
            throw new PlatformNotSupportedException($"No library defined for {platform}/{architecture}");
        }

        return Path.Combine(basePath, libraryFile);
    }

    private static string GetPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "darwin";
        
        throw new PlatformNotSupportedException("Unsupported platform");
    }

    private static string GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
        };
    }

    private Dictionary<string, Type> CreateStructTypes(Dictionary<string, StructDefinition> structs)
    {
        var structTypes = new Dictionary<string, Type>();

        foreach (var (name, structDef) in structs)
        {
            var typeBuilder = _moduleBuilder.DefineType(
                $"NativeStruct_{name}_{_typeCounter++}",
                TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Sealed,
                typeof(ValueType));

            // Add fields
            foreach (var field in structDef.Fields)
            {
                var fieldType = MapNativeType(field.Type);
                if (field.Offset.HasValue)
                {
                    typeBuilder.DefineField(field.Name, fieldType, FieldAttributes.Public)
                        .SetOffset(field.Offset.Value);
                }
                else
                {
                    typeBuilder.DefineField(field.Name, fieldType, FieldAttributes.Public);
                }
            }

            var structType = typeBuilder.CreateType();
            if (structType != null)
            {
                structTypes[name] = structType;
            }
        }

        return structTypes;
    }

    private Type CreateWrapperType(string libraryName, string libraryPath, NativeLibraryMapping mapping, 
        Dictionary<string, Type> structTypes)
    {
        var typeBuilder = _moduleBuilder.DefineType(
            $"NativeWrapper_{libraryName}_{_typeCounter++}",
            TypeAttributes.Public | TypeAttributes.Sealed);

        foreach (var (exportName, funcDef) in mapping.Exports)
        {
            CreatePInvokeMethod(typeBuilder, libraryPath, exportName, funcDef, structTypes);
        }

        var wrapperType = typeBuilder.CreateType();
        if (wrapperType == null)
        {
            throw new InvalidOperationException("Failed to create wrapper type");
        }

        return wrapperType;
    }

    private void CreatePInvokeMethod(TypeBuilder typeBuilder, string libraryPath, string methodName, 
        FunctionDefinition funcDef, Dictionary<string, Type> structTypes)
    {
        // Map parameter types
        var paramTypes = funcDef.Parameters.Select(p => {
            var baseType = MapNativeType(p.Type, structTypes);
            return p.Ref || p.Out ? baseType.MakeByRefType() : baseType;
        }).ToArray();
        var returnType = MapNativeType(funcDef.Returns, structTypes);

        // Define P/Invoke method
        var methodBuilder = typeBuilder.DefineMethod(
            methodName,
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl,
            returnType,
            paramTypes);

        // Set DllImport attributes
        var callingConvention = funcDef.CallingConvention switch
        {
            "stdcall" => CallingConvention.StdCall,
            "cdecl" => CallingConvention.Cdecl,
            "fastcall" => CallingConvention.FastCall,
            _ => CallingConvention.Cdecl // Default
        };

        var dllImportCtor = typeof(DllImportAttribute).GetConstructor(new[] { typeof(string) });
        if (dllImportCtor == null) return;

        var dllImportBuilder = new CustomAttributeBuilder(
            dllImportCtor,
            new object[] { libraryPath },
            new[]
            {
                typeof(DllImportAttribute).GetField("EntryPoint")!,
                typeof(DllImportAttribute).GetField("CallingConvention")!,
                typeof(DllImportAttribute).GetField("CharSet")!,
                typeof(DllImportAttribute).GetField("SetLastError")!
            },
            new object[]
            {
                funcDef.EntryPoint,
                callingConvention,
                CharSet.Ansi,
                false
            });

        methodBuilder.SetCustomAttribute(dllImportBuilder);

        // Set parameter attributes (ref, out)
        for (int i = 0; i < funcDef.Parameters.Count; i++)
        {
            var param = funcDef.Parameters[i];
            var paramBuilder = methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, param.Name);
            
            if (param.Ref || param.Out)
            {
                // Note: For ref/out parameters, the parameter type should already be a ByRef type
            }
        }
    }

    private Type MapNativeType(string typeName, Dictionary<string, Type>? structTypes = null)
    {
        // Handle arrays
        if (typeName.EndsWith("[]"))
        {
            var elementTypeName = typeName.Substring(0, typeName.Length - 2);
            var elementType = MapNativeType(elementTypeName, structTypes);
            return elementType.MakeArrayType();
        }

        // Basic type mapping
        return typeName switch
        {
            "void" => typeof(void),
            "bool" => typeof(bool),
            "byte" => typeof(byte),
            "sbyte" => typeof(sbyte),
            "short" => typeof(short),
            "ushort" => typeof(ushort),
            "int" => typeof(int),
            "uint" => typeof(uint),
            "long" => typeof(long),
            "ulong" => typeof(ulong),
            "float" => typeof(float),
            "double" => typeof(double),
            "string" => typeof(IntPtr), // Use IntPtr for strings to avoid marshaling issues
            "IntPtr" => typeof(IntPtr),
            "UIntPtr" => typeof(UIntPtr),
            _ => structTypes?.GetValueOrDefault(typeName) ?? typeof(IntPtr)
        };
    }

    private object CreateExportsObject(Type wrapperType, NativeLibraryMapping mapping, 
        Dictionary<string, Type> structTypes)
    {
        Console.WriteLine($"[NativeLoader] Creating exports object...");
        
        // Create JavaScript object
        var objCode = @"
            (function() {
                const exports = {};
                return exports;
            })()
        ";
        
        Console.WriteLine($"[NativeLoader] Evaluating JavaScript object creation...");
        dynamic jsExports;
        try
        {
            jsExports = _engine.Evaluate("({})");
            Console.WriteLine($"[NativeLoader] JavaScript object created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NativeLoader] Error creating JavaScript object: {ex.Message}");
            throw;
        }

        // Add functions
        Console.WriteLine($"[NativeLoader] Adding {mapping.Exports.Count} functions...");
        foreach (var (exportName, funcDef) in mapping.Exports)
        {
            Console.WriteLine($"[NativeLoader] Looking for method: {exportName}");
            var method = wrapperType.GetMethod(exportName, BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                Console.WriteLine($"[NativeLoader] Creating JavaScript function for: {exportName}");
                jsExports[exportName] = CreateJavaScriptFunction(method, funcDef, structTypes);
                Console.WriteLine($"[NativeLoader] Added function: {exportName}");
            }
            else
            {
                Console.WriteLine($"[NativeLoader] Method not found: {exportName}");
            }
        }

        // Add struct constructors
        Console.WriteLine($"[NativeLoader] Adding {structTypes.Count} struct constructors...");
        foreach (var (structName, structType) in structTypes)
        {
            Console.WriteLine($"[NativeLoader] Creating constructor for struct: {structName}");
            jsExports[structName] = CreateStructConstructor(structType, structName);
            Console.WriteLine($"[NativeLoader] Added struct constructor: {structName}");
        }

        Console.WriteLine($"[NativeLoader] Exports object created successfully");
        return jsExports;
    }

    private object CreateJavaScriptFunction(MethodInfo method, FunctionDefinition funcDef, Dictionary<string, Type> structTypes)
    {
        Console.WriteLine($"[NativeLoader] CreateJavaScriptFunction called for method: {method.Name}");
        var parameters = method.GetParameters();
        Console.WriteLine($"[NativeLoader] Method has {parameters.Length} parameters");
        
        bool returnsString = funcDef.Returns == "string";
        
        // Helper method to convert arguments for P/Invoke
        object?[] ConvertArgs(object?[] args, Dictionary<string, Type> structTypes)
        {
            for (int i = 0; i < args.Length && i < funcDef.Parameters.Count; i++)
            {
                var paramDef = funcDef.Parameters[i];
                
                if (paramDef.Type == "string" && args[i] is string str)
                {
                    args[i] = Marshal.StringToHGlobalAnsi(str);
                }
                else if (paramDef.Type.EndsWith("[]") && args[i] != null)
                {
                    // Handle array parameters
                    var elementTypeName = paramDef.Type.Substring(0, paramDef.Type.Length - 2);
                    var elementType = MapNativeType(elementTypeName, structTypes);
                    
                    try
                    {
                        dynamic jsArray = args[i];
                        int length = jsArray.length;
                        Console.WriteLine($"[NativeLoader] Converting JS array of length {length} to {elementType.Name}[]");
                        
                        var nativeArray = Array.CreateInstance(elementType, length);
                        for (int j = 0; j < length; j++)
                        {
                            var jsValue = jsArray[j];
                            var convertedValue = Convert.ChangeType(jsValue, elementType);
                            nativeArray.SetValue(convertedValue, j);
                        }
                        args[i] = nativeArray;
                        Console.WriteLine($"[NativeLoader] Converted array successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NativeLoader] Warning: Could not convert array: {ex.Message}");
                    }
                }
                else if (structTypes.ContainsKey(paramDef.Type) && args[i] != null)
                {
                    // Convert JavaScript object to native struct
                    var structType = structTypes[paramDef.Type];
                    var structInstance = Activator.CreateInstance(structType);
                    
                    if (structInstance != null)
                    {
                        // Copy fields from JS object to struct
                        var fields = structType.GetFields();
                        Console.WriteLine($"[NativeLoader] Converting struct {paramDef.Type} with {fields.Length} fields");
                        foreach (var field in fields)
                        {
                            try
                            {
                                dynamic dynamicObj = args[i];
                                var jsValue = dynamicObj[field.Name];
                                Console.WriteLine($"[NativeLoader] Field {field.Name}: JS value = {jsValue}");
                                if (jsValue != null)
                                {
                                    var convertedValue = Convert.ChangeType(jsValue, field.FieldType);
                                    field.SetValue(structInstance, convertedValue);
                                    Console.WriteLine($"[NativeLoader] Set field {field.Name} = {convertedValue}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[NativeLoader] Warning: Could not set field {field.Name}: {ex.Message}");
                            }
                        }
                        
                        // For ref parameters, we need to pass by reference
                        if (paramDef.Ref || paramDef.Out)
                        {
                            // For ref/out parameters, the method expects a reference
                            args[i] = structInstance;
                        }
                        else
                        {
                            args[i] = structInstance;
                        }
                    }
                }
            }
            return args;
        }
        
        // Helper method to convert result
        object? ConvertResult(object? result)
        {
            return returnsString && result is IntPtr ptr ? Marshal.PtrToStringAnsi(ptr) : result;
        }
        
        // Create appropriate delegate based on parameter count
        if (parameters.Length == 0)
        {
            Console.WriteLine($"[NativeLoader] Creating delegate for 0 parameters");
            return new Func<object?>(() => {
                var result = method.Invoke(null, null);
                return ConvertResult(result);
            });
        }
        else if (parameters.Length == 1)
        {
            Console.WriteLine($"[NativeLoader] Creating delegate for 1 parameter");
            return new Func<object?, object?>((arg1) => {
                var args = ConvertArgs(new[] { arg1 }, structTypes);
                var result = method.Invoke(null, args);
                return ConvertResult(result);
            });
        }
        else if (parameters.Length == 2)
        {
            Console.WriteLine($"[NativeLoader] Creating delegate for 2 parameters");
            return new Func<object?, object?, object?>((arg1, arg2) => {
                var args = ConvertArgs(new[] { arg1, arg2 }, structTypes);
                var result = method.Invoke(null, args);
                return ConvertResult(result);
            });
        }
        else if (parameters.Length == 3)
        {
            Console.WriteLine($"[NativeLoader] Creating delegate for 3 parameters");
            return new Func<object?, object?, object?, object?>((arg1, arg2, arg3) => {
                var args = ConvertArgs(new[] { arg1, arg2, arg3 }, structTypes);
                var result = method.Invoke(null, args);
                return ConvertResult(result);
            });
        }
        else
        {
            Console.WriteLine($"[NativeLoader] Creating delegate for {parameters.Length} parameters (array)");
            // For more parameters, use array
            return new Func<object?[], object?>((args) => {
                var convertedArgs = ConvertArgs(args, structTypes);
                var result = method.Invoke(null, convertedArgs);
                return ConvertResult(result);
            });
        }
    }

    private object CreateStructConstructor(Type structType, string structName)
    {
        Console.WriteLine($"[NativeLoader] CreateStructConstructor called for: {structName}");
        
        Console.WriteLine($"[NativeLoader] Setting temporary script variables");
        _engine.Script.__structType = structType;
        _engine.Script.__structName = structName;
        
        var constructorCode = @"
            (function() {
                const structType = globalThis.__structType;
                const structName = globalThis.__structName;
                
                function StructConstructor(init) {
                    // For now, return a simple object that can be used with the native functions
                    const obj = {};
                    if (init) {
                        for (const key in init) {
                            if (init.hasOwnProperty(key)) {
                                obj[key] = init[key];
                            }
                        }
                    }
                    return obj;
                }
                
                StructConstructor.structName = structName;
                
                return StructConstructor;
            })()
        ";
        
        Console.WriteLine($"[NativeLoader] Evaluating struct constructor");
        var constructor = _engine.Evaluate(constructorCode);
        Console.WriteLine($"[NativeLoader] Struct constructor evaluated");
        
        Console.WriteLine($"[NativeLoader] Cleaning up temporary script variables");
        _engine.Script.__structType = null;
        _engine.Script.__structName = null;
        
        Console.WriteLine($"[NativeLoader] Struct constructor created for: {structName}");
        return constructor;
    }

    private class LoadedNativeLibrary
    {
        public required NativeLibraryMapping Mapping { get; set; }
        public required Type WrapperType { get; set; }
        public required object Exports { get; set; }
        public required string LibraryPath { get; set; }
    }
}