using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using EkkoJS.Core.Packages;
using Newtonsoft.Json;

namespace EkkoJS.Package.CLI.Compiler;

public class PackageCompiler
{
    private readonly List<MetadataReference> _baseReferences;

    public PackageCompiler()
    {
        _baseReferences = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IEkkoPackage).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Text.RegularExpressions").Location),
            MetadataReference.CreateFromFile(typeof(JsonConvert).Assembly.Location)
        };
        
        // Add runtime assembly location
        var runtimeAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var systemPrivateCoreLib = Path.Combine(runtimeAssemblyPath, "System.Private.CoreLib.dll");
        if (File.Exists(systemPrivateCoreLib))
        {
            _baseReferences.Add(MetadataReference.CreateFromFile(systemPrivateCoreLib));
        }
    }

    public async Task<CompilationResult> CompilePackageAsync(string packagePath, CompilerOptions options)
    {
        // Read package manifest
        var manifestPath = Path.Combine(packagePath, ".ekko", "ekko.json");
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException($"Package manifest not found at {manifestPath}");
        }

        var manifestJson = await File.ReadAllTextAsync(manifestPath);
        var manifest = JsonConvert.DeserializeObject<PackageManifest>(manifestJson) 
            ?? throw new InvalidOperationException("Failed to parse package manifest");

        // Collect all files in the package
        var files = await CollectPackageFilesAsync(packagePath);

        // Generate the package implementation
        var sourceCode = GeneratePackageSource(manifest, files, Path.GetFileName(packagePath));

        // Create syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Create compilation
        var assemblyName = $"{manifest.Name.Replace("/", ".").Replace("@", "")}.dll";
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            _baseReferences,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: options.Debug ? OptimizationLevel.Debug : OptimizationLevel.Release,
                platform: Platform.AnyCpu
            )
        );

        // Compile to memory
        using var dllStream = new MemoryStream();
        using var pdbStream = options.Debug ? new MemoryStream() : null;
        
        var emitOptions = new EmitOptions(
            debugInformationFormat: options.Debug ? DebugInformationFormat.PortablePdb : DebugInformationFormat.Embedded
        );

        EmitResult result;
        if (pdbStream != null)
        {
            result = compilation.Emit(dllStream, pdbStream, options: emitOptions);
        }
        else
        {
            result = compilation.Emit(dllStream, options: emitOptions);
        }

        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage())
                .ToList();

            return new CompilationResult
            {
                Success = false,
                Errors = errors
            };
        }

        return new CompilationResult
        {
            Success = true,
            Assembly = dllStream.ToArray(),
            DebugSymbols = pdbStream?.ToArray()
        };
    }

    private async Task<Dictionary<string, byte[]>> CollectPackageFilesAsync(string packagePath)
    {
        var files = new Dictionary<string, byte[]>();
        var baseDir = new DirectoryInfo(packagePath);

        // Define folders to include
        var includeFolders = new[] { "src", "lib", "assets", ".ekko" };
        
        foreach (var folder in includeFolders)
        {
            var folderPath = Path.Combine(packagePath, folder);
            if (Directory.Exists(folderPath))
            {
                await CollectFilesRecursiveAsync(folderPath, baseDir, files);
            }
        }

        // Include specific root files
        var rootFiles = new[] { "package.json", "tsconfig.json", "README.md", "LICENSE" };
        foreach (var file in rootFiles)
        {
            var filePath = Path.Combine(packagePath, file);
            if (File.Exists(filePath))
            {
                var relativePath = Path.GetRelativePath(packagePath, filePath).Replace('\\', '/');
                files[relativePath] = await File.ReadAllBytesAsync(filePath);
            }
        }

        return files;
    }

    private async Task CollectFilesRecursiveAsync(string directory, DirectoryInfo baseDir, Dictionary<string, byte[]> files)
    {
        foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(baseDir.FullName, file).Replace('\\', '/');
            files[relativePath] = await File.ReadAllBytesAsync(file);
        }
    }

    private string GeneratePackageSource(PackageManifest manifest, Dictionary<string, byte[]> files, string packageName)
    {
        var sb = new StringBuilder();
        
        // Usings
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.IO;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using EkkoJS.Core.Packages;");
        sb.AppendLine();

        // Namespace
        var namespaceName = manifest.Name.Replace("@", "").Replace("/", ".").Replace("-", "_");
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        // Package class
        sb.AppendLine($"public class Package : IEkkoPackage");
        sb.AppendLine("{");
        
        // Properties
        sb.AppendLine($"    public string Name => \"{manifest.Name}\";");
        sb.AppendLine($"    public string Version => \"{manifest.Version}\";");
        sb.AppendLine();

        // File system field
        sb.AppendLine("    private readonly EmbeddedFileSystem _fileSystem;");
        sb.AppendLine();

        // Constructor
        sb.AppendLine("    public Package()");
        sb.AppendLine("    {");
        sb.AppendLine("        _fileSystem = new EmbeddedFileSystem();");
        sb.AppendLine("        InitializeFiles();");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetManifest method
        sb.AppendLine("    public PackageManifest GetManifest()");
        sb.AppendLine("    {");
        sb.AppendLine("        return new PackageManifest");
        sb.AppendLine("        {");
        sb.AppendLine($"            Name = \"{manifest.Name}\",");
        sb.AppendLine($"            Version = \"{manifest.Version}\",");
        sb.AppendLine($"            Description = \"{manifest.Description}\",");
        sb.AppendLine($"            Author = \"{manifest.Author}\",");
        sb.AppendLine($"            License = \"{manifest.License}\",");
        sb.AppendLine($"            Keywords = new[] {{ {string.Join(", ", manifest.Keywords.Select(k => $"\"{k}\""))} }},");
        sb.AppendLine($"            Main = {(manifest.Main != null ? $"\"{manifest.Main}\"" : "null")}");
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetFileSystem method
        sb.AppendLine("    public IPackageFileSystem GetFileSystem() => _fileSystem;");
        sb.AppendLine();

        // InitializeFiles method
        sb.AppendLine("    private void InitializeFiles()");
        sb.AppendLine("    {");
        foreach (var file in files)
        {
            var fileContent = Convert.ToBase64String(file.Value);
            sb.AppendLine($"        _fileSystem.AddFile(\"{file.Key}\", \"{fileContent}\");");
        }
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Embedded file system class
        sb.AppendLine("internal class EmbeddedFileSystem : IPackageFileSystem");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly Dictionary<string, string> _files = new();");
        sb.AppendLine();
        
        sb.AppendLine("    public void AddFile(string path, string base64Content)");
        sb.AppendLine("    {");
        sb.AppendLine("        _files[path] = base64Content;");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    public bool FileExists(string path) => _files.ContainsKey(path);");
        sb.AppendLine();

        sb.AppendLine("    public string ReadFile(string path)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (!_files.TryGetValue(path, out var base64))");
        sb.AppendLine("            throw new FileNotFoundException($\"File not found: {path}\");");
        sb.AppendLine("        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    public byte[] ReadFileBytes(string path)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (!_files.TryGetValue(path, out var base64))");
        sb.AppendLine("            throw new FileNotFoundException($\"File not found: {path}\");");
        sb.AppendLine("        return Convert.FromBase64String(base64);");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    public string[] GetFiles(string pattern = \"*\")");
        sb.AppendLine("    {");
        sb.AppendLine("        if (pattern == \"*\") return _files.Keys.ToArray();");
        sb.AppendLine("        // Simple pattern matching");
        sb.AppendLine("        var regex = new System.Text.RegularExpressions.Regex(");
        sb.AppendLine("            \"^\" + System.Text.RegularExpressions.Regex.Escape(pattern)");
        sb.AppendLine("                .Replace(\"\\\\*\", \".*\").Replace(\"\\\\?\", \".\") + \"$\");");
        sb.AppendLine("        return _files.Keys.Where(k => regex.IsMatch(k)).ToArray();");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    public string[] GetDirectories(string path = \"\")");
        sb.AppendLine("    {");
        sb.AppendLine("        var dirs = new HashSet<string>();");
        sb.AppendLine("        var prefix = string.IsNullOrEmpty(path) ? \"\" : path + \"/\";");
        sb.AppendLine("        foreach (var file in _files.Keys)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (file.StartsWith(prefix))");
        sb.AppendLine("            {");
        sb.AppendLine("                var relative = file.Substring(prefix.Length);");
        sb.AppendLine("                var slashIndex = relative.IndexOf('/');");
        sb.AppendLine("                if (slashIndex > 0)");
        sb.AppendLine("                {");
        sb.AppendLine("                    dirs.Add(prefix + relative.Substring(0, slashIndex));");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        return dirs.ToArray();");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}

public class CompilerOptions
{
    public bool Debug { get; set; }
    public string? OutputPath { get; set; }
    public bool Sign { get; set; }
    public string? SignKeyFile { get; set; }
}

public class CompilationResult
{
    public bool Success { get; set; }
    public byte[]? Assembly { get; set; }
    public byte[]? DebugSymbols { get; set; }
    public List<string> Errors { get; set; } = new();
}