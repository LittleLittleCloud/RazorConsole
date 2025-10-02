using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(ColumnsRenderableConverter))]
public sealed class ColumnsRenderableConverter : IRenderableConverter
{
    private static readonly char[] PaddingSeparators = [',', ' '];

    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsColumnsElement(element))
        {
            renderable = default!;
            return false;
        }

        var expand = string.Equals(element.Attribute("data-columns-expand")?.Value ?? element.Attribute("data-expand")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var paddingValue = element.Attribute("data-columns-padding")?.Value ?? element.Attribute("data-padding")?.Value;
        var spacing = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-spacing", 0), 0);

        var items = new List<IRenderable>(LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()));

        var columns = new Columns(items);

        if (expand)
        {
            columns = columns.Expand();
        }

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

    private static bool IsColumnsElement(XElement element)
        => string.Equals(element.Attribute("data-columns-layout")?.Value, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(element.Attribute("data-columns")?.Value, "true", StringComparison.OrdinalIgnoreCase);

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
