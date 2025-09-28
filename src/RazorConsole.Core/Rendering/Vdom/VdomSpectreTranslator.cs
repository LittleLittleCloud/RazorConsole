using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed class VdomSpectreTranslator
{
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
        switch (node)
        {
            case VTextNode textNode:
                renderable = new Text(textNode.Text ?? string.Empty);
                return true;
            case VElementNode elementNode:
                return TryTranslateElement(elementNode, context, out renderable);
            default:
                renderable = null;
                return false;
        }
    }

    private bool TryTranslateElement(VElementNode node, TranslationContext context, out IRenderable? renderable)
    {
        foreach (var translator in _elementTranslators)
        {
            if (translator.TryTranslate(node, context, out var candidate) && candidate is not null)
            {
                renderable = candidate;
                return true;
            }
        }

        renderable = null;
        return false;
    }

    private static IReadOnlyList<IVdomElementTranslator> CreateDefaultTranslators()
        => new IVdomElementTranslator[]
        {
            new TextElementTranslator(),
            new SpacerElementTranslator(),
            new NewlineElementTranslator(),
            new SpinnerElementTranslator(),
            new ButtonElementTranslator(),
            new PanelElementTranslator(),
            new RowsElementTranslator(),
            new ColumnsElementTranslator(),
            new GridElementTranslator(),
            new PadderElementTranslator(),
            new AlignElementTranslator(),
        };
    private sealed class PanelElementTranslator : IVdomElementTranslator
    {
        private static readonly IReadOnlyDictionary<string, BoxBorder> BorderLookup = new Dictionary<string, BoxBorder>(StringComparer.OrdinalIgnoreCase)
        {
            { "square", BoxBorder.Square },
            { "rounded", BoxBorder.Rounded },
            { "double", BoxBorder.Double },
            { "heavy", BoxBorder.Heavy },
            { "ascii", BoxBorder.Ascii },
            { "none", BoxBorder.None },
        };

        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!IsPanelNode(node))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var content = ComposeChildContent(children);
            var panel = new Panel(content);

            if (ShouldExpand(node))
            {
                panel = panel.Expand();
            }

            panel.Border = ResolveBorder(GetAttribute(node, "data-panel-border"));

            if (TryParsePadding(GetAttribute(node, "data-panel-padding"), out var padding))
            {
                panel.Padding = padding;
            }

            if (TryParsePositiveInt(GetAttribute(node, "data-panel-height"), out var height))
            {
                panel.Height = height;
            }

            if (TryParsePositiveInt(GetAttribute(node, "data-panel-width"), out var width))
            {
                panel.Width = width;
            }

            ApplyHeader(node, panel);
            ApplyBorderColor(node, panel);

            renderable = panel;
            return true;
        }

        private static bool IsPanelNode(VElementNode node)
        {
            if (node.Attributes.TryGetValue("data-panel", out var value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (node.Attributes.TryGetValue("data-border", out var borderValue) && string.Equals(borderValue, "panel", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool ShouldExpand(VElementNode node)
        {
            var value = GetAttribute(node, "data-panel-expand") ?? GetAttribute(node, "data-expand");
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static BoxBorder ResolveBorder(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return BoxBorder.Square;
            }

            return BorderLookup.TryGetValue(value, out var border) ? border : BoxBorder.Square;
        }

        private static void ApplyHeader(VElementNode node, Panel panel)
        {
            var header = GetAttribute(node, "data-header");
            if (string.IsNullOrWhiteSpace(header))
            {
                return;
            }

            var headerColor = GetAttribute(node, "data-header-color");
            var markup = string.IsNullOrWhiteSpace(headerColor)
                ? Markup.Escape(header)
                : ComponentMarkupUtilities.CreateStyledMarkup(headerColor, header, requiresEscape: true);

            panel.Header = new PanelHeader(markup);
        }

        private static void ApplyBorderColor(VElementNode node, Panel panel)
        {
            var borderColorValue = GetAttribute(node, "data-border-color");
            if (string.IsNullOrWhiteSpace(borderColorValue))
            {
                return;
            }

            try
            {
                var style = Style.Parse(borderColorValue);
                panel.BorderStyle(style);
            }
            catch (Exception)
            {
                // Ignore invalid style specifications.
            }
        }
    }

    private sealed class RowsElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!node.Attributes.TryGetValue("data-rows", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var rows = new Rows(children);

            if (string.Equals(GetAttribute(node, "data-expand"), "true", StringComparison.OrdinalIgnoreCase))
            {
                rows.Expand();
            }

            renderable = rows;
            return true;
        }
    }

    private sealed class ColumnsElementTranslator : IVdomElementTranslator
    {
        private static readonly char[] PaddingSeparators = [',', ' '];

        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!IsColumnsNode(node))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var columns = new Columns(children);

            if (string.Equals(GetAttribute(node, "data-columns-expand") ?? GetAttribute(node, "data-expand"), "true", StringComparison.OrdinalIgnoreCase))
            {
                columns = columns.Expand();
            }

            var paddingValue = GetAttribute(node, "data-columns-padding") ?? GetAttribute(node, "data-padding");
            var spacing = Math.Max(TryGetIntAttribute(node, "data-spacing", 0), 0);

            IRenderable result = columns;
            if (!string.IsNullOrWhiteSpace(paddingValue) && TryParsePadding(paddingValue, out var padding))
            {
                result = new Padder(columns, padding);
            }
            else if (spacing > 0)
            {
                result = new Padder(columns, new Padding(spacing, 0, spacing, 0));
            }

            renderable = result;
            return true;
        }

        private static bool IsColumnsNode(VElementNode node)
        {
            if (node.Attributes.TryGetValue("data-columns-layout", out var layoutValue) && string.Equals(layoutValue, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (node.Attributes.TryGetValue("data-columns", out var value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool TryParsePadding(string raw, out Padding padding)
        {
            var parts = raw.Split(PaddingSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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
    }

    private sealed class GridElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!node.Attributes.TryGetValue("data-grid", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var columnCount = Math.Clamp(TryGetIntAttribute(node, "data-columns", 2), 1, 4);
            var grid = new Grid();
            for (var i = 0; i < columnCount; i++)
            {
                grid.AddColumn();
            }

            if (children.Count > 0)
            {
                foreach (var row in Chunk(children, columnCount))
                {
                    grid.AddRow(row);
                }
            }

            if (string.Equals(GetAttribute(node, "data-grid-expand") ?? GetAttribute(node, "data-expand"), "true", StringComparison.OrdinalIgnoreCase))
            {
                grid.Expand();
            }

            if (TryParsePositiveInt(GetAttribute(node, "data-grid-width") ?? GetAttribute(node, "data-width"), out var width))
            {
                grid.Width = width;
            }

            renderable = grid;
            return true;
        }

        private static IEnumerable<IRenderable[]> Chunk(IReadOnlyList<IRenderable> items, int size)
        {
            var buffer = new List<IRenderable>(size);
            foreach (var item in items)
            {
                buffer.Add(item);
                if (buffer.Count == size)
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                while (buffer.Count < size)
                {
                    buffer.Add(new Markup(string.Empty));
                }

                yield return buffer.ToArray();
            }
        }
    }

    private sealed class PadderElementTranslator : IVdomElementTranslator
    {
        private static readonly char[] PaddingSeparators = [',', ' '];

        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!node.Attributes.TryGetValue("data-padder", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var content = ComposeChildContent(children);
            var padding = ParsePadding(GetAttribute(node, "data-padding"));
            var padder = new Padder(content, padding);

            if (string.Equals(GetAttribute(node, "data-expand"), "true", StringComparison.OrdinalIgnoreCase))
            {
                padder = padder.Expand();
            }

            renderable = padder;
            return true;
        }

        private static Padding ParsePadding(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new Padding(0, 0, 0, 0);
            }

            var parts = raw.Split(PaddingSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var values = parts
                .Select(part => int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? Math.Max(number, 0) : 0)
                .Take(4)
                .ToArray();

            return values.Length switch
            {
                0 => new Padding(0, 0, 0, 0),
                1 => new Padding(values[0], values[0], values[0], values[0]),
                2 => new Padding(values[0], values[1], values[0], values[1]),
                3 => new Padding(values[0], values[1], values[2], values[1]),
                4 => new Padding(values[0], values[1], values[2], values[3]),
                _ => new Padding(0, 0, 0, 0),
            };
        }
    }

    private sealed class AlignElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!node.Attributes.TryGetValue("data-align", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var content = ComposeChildContent(children);
            var horizontal = ParseHorizontalAlignment(GetAttribute(node, "data-horizontal"));
            var vertical = ParseVerticalAlignment(GetAttribute(node, "data-vertical"));
            var width = ParseOptionalPositiveInt(GetAttribute(node, "data-width"));
            var height = ParseOptionalPositiveInt(GetAttribute(node, "data-height"));

            var align = new Align(content, horizontal, vertical)
            {
                Width = width,
                Height = height,
            };

            renderable = align;
            return true;
        }
    }

    internal interface IVdomElementTranslator
    {
        bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable);
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

    private sealed class TextElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!string.Equals(node.TagName, "span", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-text", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (node.Children.OfType<VElementNode>().Any())
            {
                return false;
            }

            var style = GetAttribute(node, "data-style");
            var isMarkup = TryGetBoolAttribute(node, "data-ismarkup", out var boolValue) && boolValue;
            var content = string.Concat(node.Children.Select(child => child is VTextNode text ? text.Text : string.Empty)) ?? string.Empty;
            var markup = ComponentMarkupUtilities.CreateStyledMarkup(style, content, requiresEscape: !isMarkup);
            renderable = new Markup(markup);
            return true;
        }
    }

    private sealed class SpacerElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!node.Attributes.ContainsKey("data-spacer"))
            {
                return false;
            }

            var lines = Math.Max(TryGetIntAttribute(node, "data-lines", 1), 0);
            if (lines == 0)
            {
                renderable = new Markup(string.Empty);
                return true;
            }

            var fill = GetAttribute(node, "data-fill");
            var builder = new System.Text.StringBuilder();

            if (string.IsNullOrEmpty(fill))
            {
                for (var i = 0; i < lines; i++)
                {
                    builder.AppendLine();
                }
            }
            else
            {
                var glyph = Markup.Escape(fill[0].ToString());
                for (var i = 0; i < lines; i++)
                {
                    builder.Append(glyph);
                    builder.AppendLine();
                }
            }

            renderable = new Markup(builder.ToString());
            return true;
        }
    }

    private sealed class NewlineElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!node.Attributes.ContainsKey("data-newline"))
            {
                return false;
            }

            var count = Math.Max(TryGetIntAttribute(node, "data-count", 1), 0);
            if (count == 0)
            {
                renderable = new Markup(string.Empty);
                return true;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                builder.AppendLine();
            }

            renderable = new Markup(builder.ToString());
            return true;
        }
    }

    private sealed class SpinnerElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!node.Attributes.ContainsKey("data-spinner"))
            {
                return false;
            }

            var spinnerType = GetAttribute(node, "data-spinner-type");
            var spinner = ComponentMarkupUtilities.ResolveSpinner(spinnerType);
            var message = GetAttribute(node, "data-message") ?? string.Empty;
            var style = GetAttribute(node, "data-style");
            var autoDismiss = TryGetBoolAttribute(node, "data-auto-dismiss", out var parsed) && parsed;

            var animated = new AnimatedSpinnerRenderable(spinner, message, style, autoDismiss);
            AnimatedRenderableRegistry.Register(animated);

            renderable = animated;
            return true;
        }
    }

    private sealed class ButtonElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VElementNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(node.TagName, "button", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-button", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var descriptor = ButtonRenderableDescriptorFactory.Create(name => GetAttribute(node, name));
            renderable = ButtonRenderableBuilder.Build(descriptor, children);
            return true;
        }
    }

    private static string? GetAttribute(VElementNode node, string name)
        => node.Attributes.TryGetValue(name, out var value) ? value : null;

    private static bool TryConvertChildrenToRenderables(IReadOnlyList<VNode> children, TranslationContext context, out List<IRenderable> renderables)
    {
        renderables = new List<IRenderable>();

        foreach (var child in children)
        {
            switch (child)
            {
                case VTextNode textNode:
                    if (!string.IsNullOrWhiteSpace(textNode.Text))
                    {
                        renderables.Add(new Markup(Markup.Escape(textNode.Text)));
                    }
                    break;
                case VElementNode elementNode:
                    if (!context.TryTranslate(elementNode, out var childRenderable) || childRenderable is null)
                    {
                        renderables = new List<IRenderable>();
                        return false;
                    }

                    renderables.Add(childRenderable);
                    break;
                default:
                    break;
            }
        }

        return true;
    }

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

    private static bool TryGetBoolAttribute(VElementNode node, string name, out bool value)
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

    private static int TryGetIntAttribute(VElementNode node, string name, int fallback)
    {
        var raw = GetAttribute(node, name);
        if (!string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return fallback;
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
}
