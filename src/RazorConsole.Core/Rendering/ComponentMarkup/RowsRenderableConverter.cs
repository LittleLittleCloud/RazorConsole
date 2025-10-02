using System;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;
[RenderableConverterExport(typeof(RowsRenderableConverter))]
public sealed class RowsRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsRowsElement(element))
        {
            renderable = default!;
            return false;
        }

        var children = LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()).ToList();

        if (children.Count == 0)
        {
            renderable = new Rows(Array.Empty<IRenderable>());
            return true;
        }

        var rows = new Rows(children);

        if (string.Equals(element.Attribute("data-expand")?.Value, "true", StringComparison.OrdinalIgnoreCase))
        {
            rows.Expand();
        }

        renderable = rows;
        return true;
    }

    private static bool IsRowsElement(XElement element)
        => string.Equals(element.Attribute("data-rows")?.Value, "true", StringComparison.OrdinalIgnoreCase);
}
