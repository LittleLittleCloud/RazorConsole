using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Razor;

namespace RazorConsole.Website;

[SupportedOSPlatform("browser")]
public class DynamicComponentCompiler
{
    private static readonly RazorProjectEngine s_razorEngine = CreateRazorEngine();
    private static List<MetadataReference>? s_references = null;
    private static int s_assemblyCounter = 0;

    private static RazorProjectEngine CreateRazorEngine()
    {
        var fileSystem = RazorProjectFileSystem.Create(".");
        var projectEngine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            fileSystem,
            builder =>
            {
                builder.SetNamespace("DynamicComponents");
                builder.SetBaseType("Microsoft.AspNetCore.Components.ComponentBase");
            });

        return projectEngine;
    }

    private static async Task<List<MetadataReference>> GetReferencesAsync()
    {
        if (s_references != null)
        {
            return s_references;
        }

        var references = new List<MetadataReference>();

        Console.WriteLine("Loading assembly references for compilation...");

        // Get all currently loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .ToList();

        Console.WriteLine($"Found {assemblies.Count} loaded assemblies");

        // Collect assembly names to load
        var assemblyNames = new List<string>();
        foreach (var assembly in assemblies)
        {
            var name = assembly.GetName().Name;
            if (!string.IsNullOrEmpty(name))
            {
                assemblyNames.Add(name);
            }
        }

        // Load assemblies from the /wasm/dlls/ folder where we've placed the original .dll files
        // These are the pre-WASM-compilation DLL files that Roslyn can use
        using var httpClient = new HttpClient();
        
        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assemblyPath = $"/wasm/dlls/{assemblyName}.dll";
                Console.WriteLine($"Attempting to load: {assemblyPath}");
                
                var bytes = await httpClient.GetByteArrayAsync(assemblyPath);
                if (bytes != null && bytes.Length > 0)
                {
                    var reference = MetadataReference.CreateFromImage(bytes);
                    references.Add(reference);
                    Console.WriteLine($"✓ Loaded: {assemblyName} ({bytes.Length} bytes)");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"⚠ HTTP error loading {assemblyName}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to load {assemblyName}: {ex.Message}");
            }
        }

        Console.WriteLine($"Successfully loaded {references.Count} assembly references");
        
        s_references = references;
        return references;
    }

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Dynamic compilation requires runtime type loading")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "Dynamic compilation requires runtime type loading")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Dynamic compilation requires runtime type loading")]
    public static async Task<(Type? ComponentType, string? Error)> CompileRazorComponentAsync(string razorCode)
    {
        try
        {
            Console.WriteLine("Starting Razor compilation...");

            // Step 0: Load assembly references (lazy initialization)
            var references = await GetReferencesAsync();
            if (references.Count == 0)
            {
                return (null, "Dynamic compilation in WASM is currently limited. The Razor-to-C# generation works perfectly, but loading assembly references in the browser environment requires additional infrastructure. For now, please use the pre-compiled template examples to see RazorConsole in action.");
            }

            // Step 1: Generate C# code from Razor
            var componentName = $"DynamicComponent_{Interlocked.Increment(ref s_assemblyCounter)}";
            var razorDocument = RazorSourceDocument.Create(razorCode, $"{componentName}.razor");
            var codeDocument = s_razorEngine.Process(
                razorDocument,
                fileKind: null,
                importSources: Array.Empty<RazorSourceDocument>(),
                tagHelpers: Array.Empty<TagHelperDescriptor>());

            var csharpDocument = codeDocument.GetCSharpDocument();

            if (csharpDocument.Diagnostics.Any())
            {
                var errors = string.Join("\n", csharpDocument.Diagnostics.Select(d => d.GetMessage()));
                Console.WriteLine($"Razor compilation errors: {errors}");
                return (null, $"Razor compilation errors:\n{errors}");
            }

            var generatedCSharp = csharpDocument.GeneratedCode;
            Console.WriteLine($"Generated C# code:\n{generatedCSharp}");

            // Step 2: Compile C# code to assembly
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedCSharp);
            var compilation = CSharpCompilation.Create(
                $"DynamicAssembly_{s_assemblyCounter}",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Debug,
                    allowUnsafe: true));

            using var assemblyStream = new MemoryStream();
            using var symbolsStream = new MemoryStream();

            var emitResult = compilation.Emit(
                assemblyStream,
                symbolsStream,
                options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));

            if (!emitResult.Success)
            {
                var errors = string.Join("\n", emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => $"{d.Id}: {d.GetMessage()}"));
                Console.WriteLine($"C# compilation errors: {errors}");
                return (null, $"C# compilation errors:\n{errors}");
            }

            // Step 3: Load assembly and get component type
            assemblyStream.Seek(0, SeekOrigin.Begin);
            symbolsStream.Seek(0, SeekOrigin.Begin);

            var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, symbolsStream);
            var componentType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IComponent).IsAssignableFrom(t));

            if (componentType == null)
            {
                return (null, "No component type found in compiled assembly");
            }

            Console.WriteLine($"Successfully compiled component: {componentType.FullName}");
            return (componentType, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Compilation exception: {ex.Message}\n{ex.StackTrace}");
            return (null, $"Compilation exception: {ex.Message}");
        }
    }
}
