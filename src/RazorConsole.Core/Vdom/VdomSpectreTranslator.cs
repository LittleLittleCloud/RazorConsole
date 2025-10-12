using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    private static readonly Type[] TranslatorOrder =
    {
    typeof(TextElementTranslator),
    typeof(HtmlInlineTextElementTranslator),
        typeof(ParagraphElementTranslator),
        typeof(SpacerElementTranslator),
        typeof(NewlineElementTranslator),
        typeof(SpinnerElementTranslator),
        typeof(ButtonElementTranslator),
        typeof(HtmlButtonElementTranslator),
    typeof(SyntaxHighlighterElementTranslator),
        typeof(PanelElementTranslator),
        typeof(RowsElementTranslator),
        typeof(ColumnsElementTranslator),
        typeof(GridElementTranslator),
        typeof(PadderElementTranslator),
        typeof(AlignElementTranslator),
typeof(FigletElementTranslator),
        typeof(TableElementTranslator),
        typeof(HtmlDivElementTranslator),
        typeof(FailToRenderElementTranslator),
    };

    private static readonly Lazy<TranslatorCatalogData> TranslatorCatalog = new(DiscoverTranslators);

    private readonly IReadOnlyList<IVdomElementTranslator> _elementTranslators;

    public VdomSpectreTranslator()
        : this(CreateDefaultTranslators())
    {
    }

    internal VdomSpectreTranslator(IReadOnlyList<IVdomElementTranslator> elementTranslators)
    {
        _elementTranslators = elementTranslators ?? throw new ArgumentNullException(nameof(elementTranslators));
    }

    public bool TryTranslate(
        VNode root,
        out IRenderable? renderable,
        out IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
    {
        renderable = null;
        animatedRenderables = Array.Empty<IAnimatedConsoleRenderable>();

        var animations = new List<IAnimatedConsoleRenderable>();
        using (AnimatedRenderableRegistry.PushScope(animations))
        {
            var context = new TranslationContext(this);
            if (TryTranslateInternal(root, context, out var candidate) && candidate is not null)
            {
                renderable = candidate;
                animatedRenderables = animations;
                return true;
            }
        }

        return false;
    }

    private bool TryTranslateInternal(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        switch (node.Kind)
        {
            case VNodeKind.Text:
                renderable = new Text(node.Text ?? string.Empty);
                return true;
            case VNodeKind.Element:
                return TryTranslateElement(node, context, out renderable);
            case VNodeKind.Component:
            case VNodeKind.Region:
                if (TryConvertChildrenToRenderables(node.Children, context, out var children))
                {
                    renderable = ComposeChildContent(children);
                    return true;
                }

                renderable = null;
                return false;
            default:
                renderable = null;
                return false;
        }
    }

    private bool TryTranslateElement(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        foreach (var translator in _elementTranslators)
        {
            try
            {
                if (translator.TryTranslate(node, context, out var candidate) && candidate is not null)
                {
                    renderable = candidate;
                    return true;
                }
            }
            catch (Exception)
            {
                continue;
            }
        }

        renderable = null;
        return false;
    }

    private static IReadOnlyList<IVdomElementTranslator> CreateDefaultTranslators()
        => TranslatorCatalog.Value.Translators;

    internal interface IVdomElementTranslator
    {
        bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable);
    }

    internal sealed class TranslationContext
    {
        private readonly VdomSpectreTranslator _translator;

        public TranslationContext(VdomSpectreTranslator translator)
        {
            _translator = translator;
        }

        public bool TryTranslate(VNode node, out IRenderable? renderable)
            => _translator.TryTranslateInternal(node, this, out renderable);
    }

    private static string? GetAttribute(VNode node, string name)
    {
        if (node.Kind != VNodeKind.Element)
        {
            return null;
        }

        return node.Attributes.TryGetValue(name, out var value) ? value : null;
    }

    private static bool TryConvertChildrenToRenderables(IReadOnlyList<VNode> children, TranslationContext context, out List<IRenderable> renderables)
    {
        renderables = new List<IRenderable>();

        for (int i = 0; i < children.Count; ++i)
        {
            var child = children[i];
            switch (child.Kind)
            {
                case VNodeKind.Text:
                    var normalized = NormalizeTextNode(child.Text);

                    if (!normalized.HasContent)
                    {
                        break;
                    }

                    renderables.Add(new Markup(Markup.Escape($"{(normalized.LeadingWhitespace && i != 0 ? " " : "")}{normalized.Content}{(normalized.TrailingWhitespace ? " " : "")}")));
                    break;
                default:
                    if (!context.TryTranslate(child, out var childRenderable) || childRenderable is null)
                    {
                        renderables = new List<IRenderable>();
                        return false;
                    }

                    renderables.Add(childRenderable);
                    break;
            }
        }

        return true;
    }

    private static NormalizedText NormalizeTextNode(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return new NormalizedText(string.Empty, false, false, false);
        }

        var span = raw.AsSpan();
        var start = -1;
        var end = -1;

        for (var i = 0; i < span.Length; i++)
        {
            if (!char.IsWhiteSpace(span[i]))
            {
                start = i;
                break;
            }
        }

        if (start == -1)
        {
            return new NormalizedText(string.Empty, false, span.Length > 0, span.Length > 0);
        }

        for (var i = span.Length - 1; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(span[i]))
            {
                end = i;
                break;
            }
        }

        var leadingWhitespace = start > 0;
        var trailingWhitespace = end < span.Length - 1;

        var builder = new StringBuilder(end - start + 1);
        var previousWasWhitespace = false;

        for (var i = start; i <= end; i++)
        {
            var ch = span[i];
            if (char.IsWhiteSpace(ch))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            builder.Append(ch);
            previousWasWhitespace = false;
        }

        var content = builder.ToString();
        var hasContent = content.Length > 0;

        return new NormalizedText(content, hasContent, leadingWhitespace, trailingWhitespace);
    }

    private readonly record struct NormalizedText(string Content, bool HasContent, bool LeadingWhitespace, bool TrailingWhitespace);

    private static IRenderable ComposeChildContent(IReadOnlyList<IRenderable> children)
    {
        if (children.Count == 0)
        {
            return new Markup(string.Empty);
        }

        if (children.Count == 1)
        {
            return children[0];
        }

        return new Rows(children);
    }

    private static HorizontalAlignment ParseHorizontalAlignment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return HorizontalAlignment.Left;
        }

        return value.ToLowerInvariant() switch
        {
            "center" or "centre" => HorizontalAlignment.Center,
            "right" or "end" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Left,
        };
    }

    private static VerticalAlignment ParseVerticalAlignment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return VerticalAlignment.Top;
        }

        return value.ToLowerInvariant() switch
        {
            "middle" or "center" or "centre" => VerticalAlignment.Middle,
            "bottom" or "end" => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Top,
        };
    }

    private static int? ParseOptionalPositiveInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && result > 0)
        {
            return result;
        }

        return null;
    }

    private static bool TryGetBoolAttribute(VNode node, string name, out bool value)
    {
        var raw = GetAttribute(node, name);
        if (!string.IsNullOrWhiteSpace(raw) && bool.TryParse(raw, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = default;
        return false;
    }

    private static int TryGetIntAttribute(VNode node, string name, int fallback)
    {
        var raw = GetAttribute(node, name);
        if (!string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private static string? CollectInnerText(VNode node)
    {
        if (node is null)
        {
            return null;
        }

        var builder = new StringBuilder();
        AppendInnerText(node, builder);
        var value = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void AppendInnerText(VNode node, StringBuilder builder)
    {
        if (node.Kind == VNodeKind.Text)
        {
            if (!string.IsNullOrEmpty(node.Text))
            {
                builder.Append(node.Text);
            }

            return;
        }

        foreach (var child in node.Children)
        {
            AppendInnerText(child, builder);
        }
    }

    private static IEnumerable<string> EnumerateClassNames(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            yield break;
        }

        var parts = raw.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            yield return part;
        }
    }

    private static bool HasClass(VNode node, string className)
    {
        if (!node.Attributes.TryGetValue("class", out var classes))
        {
            return false;
        }

        return EnumerateClassNames(classes).Any(token => string.Equals(token, className, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryParsePadding(string? raw, out Padding padding)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            padding = new Padding(0, 0, 0, 0);
            return false;
        }

        var parts = raw.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var values = parts
            .Select(part => int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? Math.Max(number, 0) : 0)
            .Take(4)
            .ToArray();

        padding = values.Length switch
        {
            0 => new Padding(0, 0, 0, 0),
            1 => new Padding(values[0], values[0], values[0], values[0]),
            2 => new Padding(values[0], values[1], values[0], values[1]),
            3 => new Padding(values[0], values[1], values[2], values[1]),
            4 => new Padding(values[0], values[1], values[2], values[3]),
            _ => new Padding(0, 0, 0, 0),
        };

        return true;
    }

    private static bool TryParsePositiveInt(string? raw, out int result)
    {
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value > 0)
        {
            result = value;
            return true;
        }

        result = default;
        return false;
    }

    private static TranslatorCatalogData DiscoverTranslators()
    {
        var rootAssembly = typeof(VdomSpectreTranslator).Assembly;
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
            catch
            {
                // Ignore assemblies that cannot be loaded into the MEF container.
            }
        }

        try
        {
            using var container = configuration.CreateContainer();
            var import = container.GetExport<TranslatorImport>();
            var translators = import.Translators
                .Select(lazy => lazy.Value)
                .ToList();

            if (translators.Count == 0)
            {
                throw new InvalidOperationException("No VDOM element translators were discovered via MEF composition.");
            }

            var ordered = translators
                .OrderBy(translator => GetOrderIndex(translator.GetType()))
                .ThenBy(translator => translator.GetType().FullName, StringComparer.Ordinal)
                .ToList();

            return new TranslatorCatalogData(ordered);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get VDOM translators from MEF container.");
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

    private static int GetOrderIndex(Type translatorType)
    {
        for (var i = 0; i < TranslatorOrder.Length; i++)
        {
            if (TranslatorOrder[i].IsAssignableFrom(translatorType))
            {
                return i;
            }
        }

        return int.MaxValue;
    }

    private sealed record TranslatorCatalogData(IReadOnlyList<IVdomElementTranslator> Translators);
}

[Export(typeof(TranslatorImport))]
internal sealed class TranslatorImport
{
    [ImportMany]
    public IEnumerable<Lazy<VdomSpectreTranslator.IVdomElementTranslator>> Translators { get; set; } = Array.Empty<Lazy<VdomSpectreTranslator.IVdomElementTranslator>>();
}
