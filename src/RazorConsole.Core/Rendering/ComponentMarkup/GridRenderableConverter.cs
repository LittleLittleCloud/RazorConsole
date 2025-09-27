using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class GridRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsGridElement(element))
        {
            renderable = default!;
            return false;
        }

        var columnCount = Math.Clamp(ComponentMarkupUtilities.GetIntAttribute(element, "data-columns", 2), 1, 4);
        var spacing = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-spacing", 1), 0);
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

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            grid.AddRow(row.ToArray());

            if (spacing > 0 && index < rows.Count - 1)
            {
                grid.AddRow(CreateSpacerRow(columnCount, spacing));
            }
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

    private static IRenderable[] CreateSpacerRow(int columnCount, int spacing)
    {
        var spacer = new string(' ', Math.Max(spacing, 1));
        var markup = new Markup(spacer);
        return Enumerable.Repeat<IRenderable>(markup, columnCount).ToArray();
    }

    private static bool IsGridElement(XElement element)
        => string.Equals(element.Attribute("data-grid")?.Value, "true", StringComparison.OrdinalIgnoreCase);
}
