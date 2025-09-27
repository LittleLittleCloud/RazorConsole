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
    private static readonly TextRenderableConverter TextConverter = new();
    private static readonly NewlineRenderableConverter NewlineConverter = new();
    private static readonly SpacerRenderableConverter SpacerConverter = new();
    private static readonly SpinnerRenderableConverter SpinnerConverter = new();

    private static readonly IReadOnlyList<IMarkupConverter> MarkupConverters = new IMarkupConverter[]
    {
        TextConverter,
        NewlineConverter,
        SpacerConverter,
        SpinnerConverter,
    };

    private static readonly Type[] ConverterOrder =
    {
        typeof(PanelRenderableConverter),
        typeof(RowsRenderableConverter),
        typeof(ColumnsRenderableConverter),
        typeof(GridRenderableConverter),
    typeof(PadderRenderableConverter),
    typeof(RazorConsole.Components.Align.RenderableConverter),
        typeof(TextRenderableConverter),
        typeof(NewlineRenderableConverter),
        typeof(SpacerRenderableConverter),
        typeof(SpinnerRenderableConverter),
    };

    private static readonly Lazy<ConverterCatalogData> ConverterCatalog = new(DiscoverConverters);

    private static IReadOnlyList<IRenderableConverter> RenderableConverters => ConverterCatalog.Value.Converters;

    private static IReadOnlyDictionary<string, IRenderableConverter> ConverterLookup => ConverterCatalog.Value.Lookup;

    public static string Convert(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var wrapped = $"<root>{html}</root>";
        var document = XDocument.Parse(wrapped, LoadOptions.None);

        return ConvertNodes(document.Root!.Nodes());
    }

    public static string ConvertNodes(IEnumerable<XNode> nodes)
    {
        var builder = new StringBuilder();
        foreach (var node in nodes)
        {
            AppendNode(node, builder);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendNode(XNode node, StringBuilder builder)
    {
        switch (node)
        {
            case XText text:
                AppendText(text, builder);
                break;
            case XElement element:
                AppendElement(element, builder);
                break;
        }
    }

    private static void AppendText(XText text, StringBuilder builder)
    {
        var value = text.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append(Markup.Escape(value));
    }

    private static void AppendElement(XElement element, StringBuilder builder)
    {
        if (TryConvertUsingMarkupConverters(element, builder))
        {
            return;
        }

        var name = element.Name.LocalName.ToLowerInvariant();

        switch (name)
        {
            case "h1":
                builder.Append("[bold underline]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                builder.AppendLine();
                break;
            case "h2":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                builder.AppendLine();
                break;
            case "h3":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                builder.AppendLine();
                break;
            case "label":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]: ");
                break;
            case "p":
                string text = element.Value;
                text = text.Replace(Environment.NewLine, " ").Trim();
                element.Value = text;
                AppendChildren(element, builder);
                break;
            case "input":
                var value = element.Attribute("value")?.Value ?? string.Empty;
                var placeholder = element.Attribute("placeholder")?.Value ?? string.Empty;
                var display = !string.IsNullOrWhiteSpace(value)
                    ? Markup.Escape(value)
                    : (!string.IsNullOrWhiteSpace(placeholder) ? Markup.Escape(placeholder) : string.Empty);

                builder.Append("[grey53]<");
                builder.Append("input");
                if (!string.IsNullOrEmpty(display))
                {
                    builder.Append($" : {display}");
                }
                builder.AppendLine(">[/]");
                break;
            case "time":
                AppendChildren(element, builder);
                break;
            case "strong":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                break;
            case "em":
                builder.Append("[italic]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                break;
            case "ul":
                foreach (var child in element.Elements("li"))
                {
                    builder.Append(" [green]•[/] ");
                    AppendChildren(child, builder);
                    builder.AppendLine();
                }
                builder.AppendLine();
                break;
            case "li":
                builder.Append(" [green]•[/] ");
                AppendChildren(element, builder);
                builder.AppendLine();
                break;
            case "br":
                builder.AppendLine();
                break;
            case "div":
                AppendChildrenWithSpacing(element, builder);
                break;
            case "span":
                AppendChildrenWithSpacing(element, builder);
                break;
            case "section":
            case "header":
            case "main":
            case "article":
            case "footer":
                AppendChildrenWithSpacing(element, builder);
                break;
            default:
                AppendChildren(element, builder);
                break;
        }
    }

    private static void AppendChildren(XElement element, StringBuilder builder)
    {
        foreach (var child in element.Nodes())
        {
            AppendNode(child, builder);
        }
    }

    private static void AppendChildrenWithSpacing(XElement element, StringBuilder builder)
    {
        var children = element.Nodes().ToList();
        for (var i = 0; i < children.Count; i++)
        {
            AppendNode(children[i], builder);
            if (i < children.Count - 1)
            {
                builder.AppendLine();
            }
        }
    }

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

    private static bool TryConvertUsingMarkupConverters(XElement element, StringBuilder builder)
    {
        foreach (var converter in MarkupConverters)
        {
            if (converter.TryConvert(element, out var markup))
            {
                builder.Append(markup);
                return true;
            }
        }

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
