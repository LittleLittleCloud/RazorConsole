using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class ColumnsRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsColumnsElement(element))
        {
            renderable = default!;
            return false;
        }

        var spacing = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-spacing", 1), 0);
        var collapse = string.Equals(element.Attribute("data-collapse")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var alignmentValue = element.Attribute("data-alignment")?.Value;
        var hasAlignment = LayoutRenderableUtilities.TryParseJustify(alignmentValue, out var justify);

        var items = new List<IRenderable>(LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()));

        if (items.Count == 0)
        {
            renderable = new Rows(Array.Empty<IRenderable>());
            return true;
        }

        if (collapse)
        {
            renderable = new Rows(items);
            return true;
        }

        var grid = new Grid();
        for (var i = 0; i < items.Count; i++)
        {
            var column = new GridColumn();

            if (spacing > 0)
            {
                var left = spacing / 2;
                var right = spacing - left;
                column.PadLeft(left).PadRight(right);
            }

            if (hasAlignment)
            {
                column.Alignment(justify);
            }

            grid.AddColumn(column);
        }

        grid.AddRow(items.ToArray());

        renderable = grid;
        return true;
    }

    private static bool IsColumnsElement(XElement element)
        => string.Equals(element.Attribute("data-columns-layout")?.Value, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(element.Attribute("data-columns")?.Value, "true", StringComparison.OrdinalIgnoreCase);
}