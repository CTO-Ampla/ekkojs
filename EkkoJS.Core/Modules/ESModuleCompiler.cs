using System.Text.RegularExpressions;
using System.Text;

namespace EkkoJS.Core.Modules;

public class ESModuleCompiler
{
    private static readonly Regex ImportRegex = new Regex(
        @"^\s*import\s+(?:(?<default>\w+)|(?:\{(?<named>[^}]+)\})|(?<default2>\w+)\s*,\s*\{(?<named2>[^}]+)\})\s+from\s+['""](?<module>[^'""]+)['""];?\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled
    );
    
    private static readonly Regex ExportRegex = new Regex(
        @"^\s*export\s+(?:(?<declaration>(?:const|let|var|function|class)\s+)|(?<default>default\s+)|(?:\{(?<named>[^}]+)\}))",
        RegexOptions.Multiline | RegexOptions.Compiled
    );

    public static string TransformESModule(string code, string moduleUrl)
    {
        var sb = new StringBuilder();
        
        // Store the module promise in a global
        sb.AppendLine("globalThis.__currentModulePromise = (async function() {");
        sb.AppendLine($"const import_meta = __createImportMeta('{moduleUrl}');");
        sb.AppendLine();
        
        // Process imports
        var importMatches = ImportRegex.Matches(code);
        var lastImportEnd = 0;
        
        foreach (Match match in importMatches)
        {
            // Add code before this import
            if (match.Index > lastImportEnd)
            {
                sb.Append(code.Substring(lastImportEnd, match.Index - lastImportEnd));
            }
            
            var moduleSpecifier = match.Groups["module"].Value;
            var defaultImport = match.Groups["default"].Success ? match.Groups["default"].Value : 
                               match.Groups["default2"].Success ? match.Groups["default2"].Value : null;
            var namedImports = match.Groups["named"].Success ? match.Groups["named"].Value :
                              match.Groups["named2"].Success ? match.Groups["named2"].Value : null;
            
            // Generate import code with unique identifier
            var moduleVarName = $"__module_{moduleSpecifier.Replace(':', '_').Replace('/', '_').Replace('-', '_').Replace('.', '_')}_{match.Index}";
            sb.AppendLine($"const {moduleVarName} = await globalThis.import('{moduleSpecifier}');");
            
            if (defaultImport != null)
            {
                sb.AppendLine($"const {defaultImport} = {moduleVarName}.default;");
            }
            
            if (namedImports != null)
            {
                var imports = namedImports.Split(',').Select(s => s.Trim());
                foreach (var imp in imports)
                {
                    var parts = imp.Split(new[] { " as " }, StringSplitOptions.None);
                    var originalName = parts[0].Trim();
                    var localName = parts.Length > 1 ? parts[1].Trim() : originalName;
                    sb.AppendLine($"const {localName} = {moduleVarName}.{originalName};");
                }
            }
            
            lastImportEnd = match.Index + match.Length;
        }
        
        // Add remaining code
        if (lastImportEnd < code.Length)
        {
            sb.Append(code.Substring(lastImportEnd));
        }
        
        // Process exports (simplified for now - just track them)
        var exports = new List<string>();
        var exportMatches = ExportRegex.Matches(sb.ToString());
        
        foreach (Match match in exportMatches)
        {
            if (match.Groups["default"].Success)
            {
                exports.Add("default");
            }
            else if (match.Groups["named"].Success)
            {
                var namedExports = match.Groups["named"].Value.Split(',').Select(s => s.Trim());
                exports.AddRange(namedExports);
            }
        }
        
        // Close the async wrapper and return the promise
        sb.AppendLine();
        sb.AppendLine("})();");
        
        // Return the promise so it can be awaited
        return sb.ToString();
    }
}