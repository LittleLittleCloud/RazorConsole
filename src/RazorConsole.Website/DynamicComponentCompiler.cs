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
    private static readonly List<MetadataReference> s_references = GetReferences();
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

    private static List<MetadataReference> GetReferences()
    {
        var references = new List<MetadataReference>();

        // Get all loaded assemblies that are relevant
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .ToList();

        foreach (var assembly in assemblies)
        {
            try
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            catch
            {
                // Some assemblies may not be accessible
                Console.WriteLine($"Could not create reference for assembly: {assembly.FullName}");
            }
        }

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
                s_references,
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
