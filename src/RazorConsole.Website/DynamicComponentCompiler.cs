// Copyright (c) RazorConsole. All rights reserved.

using System.Runtime.Loader;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

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
        // We need to use absolute URLs in WASM environment
        var baseUri = GetBaseUri();
        Console.WriteLine($"Base URI: {baseUri}");

        using var httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };

        // Define essential assemblies that must be loaded for compilation
        var essentialAssemblies = new[]
        {
            // Core System assemblies
            "System.Private.CoreLib",
            "System.Runtime",
            "System.Runtime.InteropServices",
            "System.Collections",
            "System.Collections.Immutable",
            "System.Linq",
            "System.Threading.Tasks",
            "System.ComponentModel",
            "System.ComponentModel.Primitives",
            "System.ObjectModel",
            "System.Memory",
            "System.Text.Encodings.Web",
            "System.Text.Json",
            "netstandard",
            "mscorlib",

            // ASP.NET Components
            "Microsoft.AspNetCore.Components",
            "Microsoft.AspNetCore.Components.Web",
            "Microsoft.Extensions.DependencyInjection.Abstractions",
            "Microsoft.Extensions.DependencyInjection",

            // RazorConsole
            "RazorConsole.Core",
            "Spectre.Console",
        };

        // First, try to load essential assemblies
        foreach (var assemblyName in essentialAssemblies)
        {
            await TryLoadAssemblyAsync(httpClient, assemblyName, references);
        }

        // Then try to load any other loaded assemblies that we might need
        foreach (var assemblyName in assemblyNames.Where(n => !essentialAssemblies.Contains(n)))
        {
            await TryLoadAssemblyAsync(httpClient, assemblyName, references);
        }

        Console.WriteLine($"Successfully loaded {references.Count} assembly references");

        s_references = references;
        return references;
    }

    private static string GetBaseUri()
    {
        // In WASM environment, we need to determine the base URI
        // This is typically done via JavaScript interop, but we can also use a fallback
        // For now, we'll use a relative path approach that works with the HttpClient in WASM
        return "./";
    }

    private static async Task TryLoadAssemblyAsync(HttpClient httpClient, string assemblyName, List<MetadataReference> references)
    {
        try
        {
            var assemblyPath = $"wasm/dlls/{assemblyName}.dll";
            Console.WriteLine($"Attempting to load: {assemblyPath}");

            var response = await httpClient.GetAsync(assemblyPath);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (bytes != null && bytes.Length > 0)
                {
                    var reference = MetadataReference.CreateFromImage(bytes);
                    references.Add(reference);
                    Console.WriteLine($"✓ Loaded: {assemblyName} ({bytes.Length} bytes)");
                }
            }
            else
            {
                Console.WriteLine($"⚠ HTTP {(int)response.StatusCode} loading {assemblyName}");
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
                return (null, "Failed to load assembly references from /wasm/dlls/. Please ensure the DLL files are available for dynamic compilation.");
            }

            Console.WriteLine($"Loaded {references.Count} assembly references for compilation");

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
