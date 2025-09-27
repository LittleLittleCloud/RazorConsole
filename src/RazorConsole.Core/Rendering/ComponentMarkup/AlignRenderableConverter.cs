using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class AlignRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsAlignElement(element))
        {
            renderable = default!;
            return false;
        }

        var childRenderables = LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()).ToList();
        var content = ComposeChildContent(childRenderables);

        var horizontal = ParseHorizontalAlignment(element.Attribute("data-horizontal")?.Value);
        var vertical = ParseVerticalAlignment(element.Attribute("data-vertical")?.Value);
        var width = ParseSize(element.Attribute("data-width")?.Value);
        var height = ParseSize(element.Attribute("data-height")?.Value);

        var align = new Align(content, horizontal, vertical)
        {
            Width = width,
            Height = height,
        };

        renderable = align;
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

    private static int? ParseSize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var size) && size > 0)
        {
            return size;
        }

        return null;
    }

    private static bool IsAlignElement(XElement element)
        => string.Equals(element.Attribute("data-align")?.Value, "true", StringComparison.OrdinalIgnoreCase);
}
