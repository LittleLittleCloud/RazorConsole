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

        var items = new List<IRenderable>(LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()));

        IRenderable columns = new Columns(items);

        if (spacing > 0)
        {
            columns = new Padder(columns, new Padding(spacing, 0, spacing, 0));
        }

        renderable = columns;
        return true;
    }

    private static bool IsColumnsElement(XElement element)
        => string.Equals(element.Attribute("data-columns-layout")?.Value, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(element.Attribute("data-columns")?.Value, "true", StringComparison.OrdinalIgnoreCase);
}