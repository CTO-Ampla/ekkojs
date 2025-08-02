using Microsoft.ClearScript;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;

namespace EkkoJS.Core.Modules.BuiltIn;

public class PathModule : IModule
{
    public string Name => "path";
    public string Protocol => "ekko";

    public object GetExports()
    {
        dynamic exports = new ExpandoObject();
        
        // Internal functions that handle the actual work
        dynamic internals = new ExpandoObject();
        
        internals.joinPaths = new Func<object, string>(jsArray =>
        {
            // Convert JavaScript array to string array
            var parts = ConvertJsArrayToStringArray(jsArray);
            if (parts.Length == 0)
                return "";
            return Path.Combine(parts);
        });
        
        internals.resolvePaths = new Func<object, string>(jsArray =>
        {
            // Convert JavaScript array to string array
            var parts = ConvertJsArrayToStringArray(jsArray);
            if (parts.Length == 0)
                return Directory.GetCurrentDirectory();
            var combined = Path.Combine(parts);
            return Path.GetFullPath(combined);
        });
        
        internals.dirname = new Func<string, string?>(path =>
        {
            return Path.GetDirectoryName(path);
        });

        internals.basename = new Func<string, string?, string>((path, ext) =>
        {
            var filename = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(ext) && filename.EndsWith(ext))
            {
                filename = filename.Substring(0, filename.Length - ext.Length);
            }
            return filename;
        });

        internals.extname = new Func<string, string>(path =>
        {
            return Path.GetExtension(path);
        });

        internals.parse = new Func<string, object>(path =>
        {
            dynamic parsed = new ExpandoObject();
            parsed.root = Path.GetPathRoot(path) ?? "";
            parsed.dir = Path.GetDirectoryName(path) ?? "";
            parsed.@base = Path.GetFileName(path);
            parsed.ext = Path.GetExtension(path);
            parsed.name = Path.GetFileNameWithoutExtension(path);
            return parsed;
        });

        internals.format = new Func<dynamic, string>(pathObject =>
        {
            string dir = pathObject.dir ?? "";
            string @base = pathObject.@base ?? "";
            
            if (!string.IsNullOrEmpty(@base))
            {
                return Path.Combine(dir, @base);
            }
            
            string name = pathObject.name ?? "";
            string ext = pathObject.ext ?? "";
            
            if (!string.IsNullOrEmpty(name))
            {
                return Path.Combine(dir, name + ext);
            }
            
            return dir;
        });

        internals.isAbsolute = new Func<string, bool>(path =>
        {
            return Path.IsPathRooted(path);
        });

        internals.relative = new Func<string, string, string>((from, to) =>
        {
            var fromUri = new Uri(Path.GetFullPath(from));
            var toUri = new Uri(Path.GetFullPath(to));
            var relativeUri = fromUri.MakeRelativeUri(toUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        });
        
        // Store internals for access
        exports._internals = internals;
        
        // Public properties
        exports.sep = Path.DirectorySeparatorChar.ToString();
        exports.delimiter = Path.PathSeparator.ToString();

        return exports;
    }
    
    private static string[] ConvertJsArrayToStringArray(object jsArray)
    {
        if (jsArray == null)
            return Array.Empty<string>();
            
        // Handle different array types from ClearScript
        if (jsArray is Microsoft.ClearScript.ScriptObject scriptObject)
        {
            var list = new List<string>();
            foreach (var index in scriptObject.PropertyIndices)
            {
                var value = scriptObject[index];
                list.Add(value?.ToString() ?? "");
            }
            return list.ToArray();
        }
        else if (jsArray is object[] objArray)
        {
            return objArray.Select(o => o?.ToString() ?? "").ToArray()!;
        }
        else if (jsArray is string[] strArray)
        {
            return strArray;
        }
        
        // Fallback
        return new[] { jsArray.ToString() ?? "" };
    }
}