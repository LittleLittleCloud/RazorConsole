using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(GridRenderableConverter))]
public sealed class GridRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsGridElement(element))
        {
            renderable = default!;
            return false;
        }

        var columnCount = Math.Clamp(ComponentMarkupUtilities.GetIntAttribute(element, "data-columns", 2), 1, 4);
        var widthValue = element.Attribute("data-grid-width")?.Value ?? element.Attribute("data-width")?.Value;
        var expand = string.Equals(element.Attribute("data-grid-expand")?.Value ?? element.Attribute("data-expand")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var showHeaders = string.Equals(element.Attribute("data-show-headers")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        _ = showHeaders;

        var cells = LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()).ToList();

        var grid = new Grid();
        for (var i = 0; i < columnCount; i++)
        {
            grid.AddColumn();
        }

        if (cells.Count == 0)
        {
            renderable = grid;
            return true;
        }

        var rows = ChunkCells(cells, columnCount).ToList();

        foreach (var row in rows)
        {
            grid.AddRow(row.ToArray());
        }

        if (expand)
        {
            grid.Expand();
        }

        if (TryParsePositiveInt(widthValue, out var width))
        {
            grid.Width = width;
        }

        renderable = grid;
        return true;
    }

    private static IEnumerable<IReadOnlyList<IRenderable>> ChunkCells(IReadOnlyList<IRenderable> cells, int columnCount)
    {
        var buffer = new List<IRenderable>(columnCount);
        for (var i = 0; i < cells.Count; i++)
        {
            buffer.Add(cells[i]);
            if (buffer.Count == columnCount)
            {
                yield return buffer.ToArray();
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
        {
            while (buffer.Count < columnCount)
            {
                buffer.Add(new Markup(string.Empty));
            }

            yield return buffer.ToArray();
        }
    }

    private static bool IsGridElement(XElement element)
        => string.Equals(element.Attribute("data-grid")?.Value, "true", StringComparison.OrdinalIgnoreCase);

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
