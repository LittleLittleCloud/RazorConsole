using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(PadderRenderableConverter))]
public sealed class PadderRenderableConverter : IRenderableConverter
{
    private static readonly char[] PaddingSeparators = [',', ' '];

    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsPadderElement(element))
        {
            renderable = default!;
            return false;
        }

        var childRenderables = LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()).ToList();
        var content = ComposeChildContent(childRenderables);
        var padding = ParsePadding(element.Attribute("data-padding")?.Value);

        var padder = new Padder(content, padding);
        if (ShouldExpand(element))
        {
            padder = padder.Expand();
        }

        renderable = padder;
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

    private static bool ShouldExpand(XElement element)
        => string.Equals(element.Attribute("data-expand")?.Value, "true", StringComparison.OrdinalIgnoreCase);

    private static bool IsPadderElement(XElement element)
        => string.Equals(element.Attribute("data-padder")?.Value, "true", StringComparison.OrdinalIgnoreCase);
}
