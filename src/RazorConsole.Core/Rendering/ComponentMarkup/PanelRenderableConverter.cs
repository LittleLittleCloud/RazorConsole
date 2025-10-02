using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RazorConsole.Core.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(PanelRenderableConverter))]
public sealed class PanelRenderableConverter : IRenderableConverter
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

    private static readonly char[] PaddingSeparators = [',', ' '];

    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsPanelElement(element))
        {
            renderable = default!;
            return false;
        }

        renderable = CreatePanel(element);
        return true;
    }

    private static bool IsPanelElement(XElement element)
        => string.Equals(element.Attribute("data-border")?.Value, "panel", StringComparison.OrdinalIgnoreCase)
           || string.Equals(element.Attribute("data-panel")?.Value, "true", StringComparison.OrdinalIgnoreCase);

    private static Panel CreatePanel(XElement element)
    {
        var expandValue = element.Attribute("data-panel-expand")?.Value ?? element.Attribute("data-expand")?.Value;
        var expand = string.Equals(expandValue, "true", StringComparison.OrdinalIgnoreCase);
        var borderName = element.Attribute("data-panel-border")?.Value;
        var paddingValue = element.Attribute("data-panel-padding")?.Value;
        var heightValue = element.Attribute("data-panel-height")?.Value;
        var widthValue = element.Attribute("data-panel-width")?.Value;
        var content = BuildPanelContent(element.Nodes());
        var panel = new Panel(content);

        if (expand)
        {
            panel = panel.Expand();
        }

        if (TryParseBorder(borderName, out var border))
        {
            panel.Border = border;
        }
        else
        {
            panel.Border = BoxBorder.Square;
        }

        if (TryParsePadding(paddingValue, out var padding))
        {
            panel.Padding = padding;
        }

        if (TryParsePositiveInt(heightValue, out var height))
        {
            panel.Height = height;
        }

        if (TryParsePositiveInt(widthValue, out var width))
        {
            panel.Width = width;
        }

        var header = element.Attribute("data-header")?.Value;
        if (!string.IsNullOrWhiteSpace(header))
        {
            var headerColor = element.Attribute("data-header-color")?.Value;
            var headerMarkup = !string.IsNullOrWhiteSpace(headerColor)
                ? $"[{headerColor}]{Markup.Escape(header)}[/]"
                : Markup.Escape(header);

            panel.Header = new PanelHeader(headerMarkup);
        }

        var borderColorValue = element.Attribute("data-border-color")?.Value;
        if (!string.IsNullOrWhiteSpace(borderColorValue))
        {
            try
            {
                var style = Style.Parse(borderColorValue);
                panel.BorderStyle(style);
            }
            catch (Exception)
            {
                // Ignore invalid color specifications and keep default styling.
            }
        }

        return panel;
    }

    private static IRenderable BuildPanelContent(IEnumerable<XNode> nodes, string? orientation = null)
    {
        var renderables = LayoutRenderableUtilities.ConvertChildNodesToRenderables(nodes).ToList();

        if (renderables.Count == 0)
        {
            return new Markup(string.Empty);
        }

        if (renderables.Count == 1)
        {
            return renderables[0];
        }

        var useHorizontal = string.Equals(orientation, "horizontal", StringComparison.OrdinalIgnoreCase);

        if (useHorizontal)
        {
            return new Columns(renderables);
        }

        return new Rows(renderables);
    }

    private static bool TryParseBorder(string? value, out BoxBorder border)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            border = BoxBorder.Square;
            return false;
        }

        if (BorderLookup.TryGetValue(value, out var resolved))
        {
            border = resolved;
            return true;
        }

        border = BoxBorder.Square;
        return false;
    }

    private static bool TryParsePadding(string? raw, out Padding padding)
    {
        padding = default;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

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
