using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using RazorConsole.Components;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public static class HtmlToSpectreRenderableConverter
{
    private static readonly Type[] ConverterOrder =
    {
        typeof(PanelRenderableConverter),
        typeof(RowsRenderableConverter),
        typeof(ColumnsRenderableConverter),
        typeof(GridRenderableConverter),
        typeof(PadderRenderableConverter),
    typeof(ButtonRenderableConverter),
        typeof(RazorConsole.Components.Align.RenderableConverter),
        typeof(TextRenderableConverter),
        typeof(NewlineRenderableConverter),
        typeof(SpacerRenderableConverter),
        typeof(SpinnerRenderableConverter),
    };

    private static readonly Lazy<ConverterCatalogData> ConverterCatalog = new(DiscoverConverters);

    private static IReadOnlyList<IRenderableConverter> RenderableConverters => ConverterCatalog.Value.Converters;

    private static IReadOnlyDictionary<string, IRenderableConverter> ConverterLookup => ConverterCatalog.Value.Lookup;

    internal static bool TryConvertToRenderable(XElement element, out IRenderable? renderable)
    {
        if (TryConvertUsingAttribute(element, out renderable))
        {
            return true;
        }

        if (TryConvertUsingRegisteredConverters(element, out renderable))
        {
            return true;
        }

        renderable = null;
        return false;
    }

    private static bool TryConvertUsingAttribute(XElement element, out IRenderable? renderable)
    {
        var key = element.Attribute("data-converter")?.Value;
        if (string.IsNullOrWhiteSpace(key))
        {
            renderable = null;
            return false;
        }

        if (!ConverterLookup.TryGetValue(key, out var converter))
        {
            renderable = null;
            return false;
        }

        if (converter.TryConvert(element, out var candidate))
        {
            renderable = candidate;
            return true;
        }

        renderable = null;
        return false;
    }

    private static bool TryConvertUsingRegisteredConverters(XElement element, out IRenderable? renderable)
    {
        foreach (var converter in RenderableConverters)
        {
            if (converter.TryConvert(element, out var candidate))
            {
                renderable = candidate;
                return true;
            }
        }

        renderable = null;
        return false;
    }

    private static ConverterCatalogData DiscoverConverters()
    {
        var rootAssembly = typeof(HtmlToSpectreRenderableConverter).Assembly;
        var processedAssemblies = new HashSet<Assembly> { rootAssembly };

        var configuration = new ContainerConfiguration().WithAssembly(rootAssembly);

        foreach (var assembly in GetCandidateAssemblies())
        {
            if (!processedAssemblies.Add(assembly))
            {
                continue;
            }

            try
            {
                configuration = configuration.WithAssembly(assembly);
            }
            catch (Exception)
            {
                // pass
            }
        }
        try
        {
            using var container = configuration.CreateContainer();
            var import = container.GetExport<ConverterImport>();
            var converters = new List<IRenderableConverter>();
            var lookup = new Dictionary<string, IRenderableConverter>(StringComparer.Ordinal);
            foreach (var lazy in import.Converters)
            {
                var converter = lazy.Value;
                converters.Add(converter);
                var converterType = converter.GetType();
                RegisterConverterKey(lookup, converterType.FullName, converter);
            }

            if (converters.Count == 0)
            {
                throw new InvalidOperationException("No renderable converters were discovered via MEF composition.");
            }

            var orderedConverters = converters
                .OrderBy(converter => GetOrderIndex(converter.GetType()))
                .ThenBy(converter => converter.GetType().FullName, StringComparer.Ordinal)
                .ToList();

            return new ConverterCatalogData(orderedConverters, lookup);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get export from MEF container.");
            Console.WriteLine(ex);
            throw;
        }
    }

    private static IEnumerable<Assembly> GetCandidateAssemblies()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            var name = assembly.GetName().Name;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (name.StartsWith("Microsoft", StringComparison.Ordinal) ||
                name.StartsWith("System", StringComparison.Ordinal))
            {
                continue;
            }

            yield return assembly;
        }
    }

    private static void RegisterConverterKey(IDictionary<string, IRenderableConverter> lookup, string? key, IRenderableConverter converter)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        lookup[key] = converter;
    }

    private static int GetOrderIndex(Type converterType)
    {
        for (var i = 0; i < ConverterOrder.Length; i++)
        {
            if (ConverterOrder[i].IsAssignableFrom(converterType))
            {
                return i;
            }
        }

        return int.MaxValue;
    }



    private sealed record ConverterCatalogData(IReadOnlyList<IRenderableConverter> Converters, IReadOnlyDictionary<string, IRenderableConverter> Lookup);
}

[Export(typeof(ConverterImport))]
internal sealed class ConverterImport
{
    [ImportMany]
    public IEnumerable<Lazy<IRenderableConverter, RenderableConverterMetadata>> Converters { get; set; } = Array.Empty<Lazy<IRenderableConverter, RenderableConverterMetadata>>();
}
