using Microsoft.ClearScript.V8;
using System.Text.Json;
using System.IO;
using System.Net.Http;

namespace EkkoJS.Core.TypeScript;

public class TypeScriptCompiler : IDisposable
{
    private readonly V8ScriptEngine _compilerEngine;
    private bool _initialized = false;
    private readonly HttpClient _httpClient;
    private readonly string _cacheDir;

    public TypeScriptCompiler()
    {
        _compilerEngine = new V8ScriptEngine();
        _httpClient = new HttpClient();
        _cacheDir = Path.Combine(Path.GetTempPath(), "ekkojs", "typescript");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        try
        {
            // Try to load TypeScript compiler from cache or CDN
            var typeScriptCode = await LoadTypeScriptCompilerAsync();
            
            // Load TypeScript compiler
            _compilerEngine.Execute(typeScriptCode);
            
            // Create our transpile helper function
            _compilerEngine.Execute(@"
                function transpileTypeScript(source, options) {
                    options = options || {};
                    
                    const compilerOptions = {
                        module: ts.ModuleKind.ESNext,
                        target: ts.ScriptTarget.ES2022,
                        strict: true,
                        esModuleInterop: true,
                        skipLibCheck: true,
                        forceConsistentCasingInFileNames: true,
                        moduleResolution: ts.ModuleResolutionKind.NodeJs,
                        noEmitOnError: false,
                        removeComments: false,
                        sourceMap: false,
                        ...options
                    };
                    
                    const result = ts.transpileModule(source, {
                        compilerOptions: compilerOptions,
                        fileName: options.fileName || 'module.ts'
                    });
                    
                    return {
                        outputText: result.outputText,
                        diagnostics: result.diagnostics || []
                    };
                }
            ");
            
            _initialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize TypeScript compiler", ex);
        }
    }

    private async Task<string> LoadTypeScriptCompilerAsync()
    {
        // Check cache first
        var cacheFile = Path.Combine(_cacheDir, "typescript.js");
        if (File.Exists(cacheFile))
        {
            var cachedContent = await File.ReadAllTextAsync(cacheFile);
            if (!string.IsNullOrWhiteSpace(cachedContent))
            {
                return cachedContent;
            }
        }

        // Download from CDN
        try
        {
            var response = await _httpClient.GetStringAsync(
                "https://cdnjs.cloudflare.com/ajax/libs/typescript/5.3.3/typescript.min.js"
            );
            
            // Cache it
            await File.WriteAllTextAsync(cacheFile, response);
            
            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                "Failed to download TypeScript compiler. Please check your internet connection.", ex);
        }
    }

    public async Task<CompilationResult> CompileAsync(string typeScriptCode, string fileName = "module.ts")
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }

        try
        {
            dynamic result = _compilerEngine.Script.transpileTypeScript(typeScriptCode, new
            {
                fileName = fileName
            });

            var diagnostics = new List<string>();
            if (result.diagnostics != null)
            {
                foreach (var diagnostic in result.diagnostics)
                {
                    diagnostics.Add($"{diagnostic.messageText}");
                }
            }

            return new CompilationResult
            {
                Success = diagnostics.Count == 0,
                JavaScriptCode = result.outputText,
                Diagnostics = diagnostics
            };
        }
        catch (Exception ex)
        {
            return new CompilationResult
            {
                Success = false,
                JavaScriptCode = "",
                Diagnostics = new List<string> { $"Compilation error: {ex.Message}" }
            };
        }
    }

    public void Dispose()
    {
        _compilerEngine?.Dispose();
        _httpClient?.Dispose();
    }
}

public class CompilationResult
{
    public bool Success { get; set; }
    public string JavaScriptCode { get; set; } = "";
    public List<string> Diagnostics { get; set; } = new();
}