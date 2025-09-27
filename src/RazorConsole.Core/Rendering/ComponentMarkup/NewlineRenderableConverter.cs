using System;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(NewlineRenderableConverter))]
public sealed class NewlineRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!TryGetMarkup(element, out var markup))
        {
            renderable = default!;
            return false;
        }

        renderable = new Markup(markup);
        return true;
    }

    internal bool TryConvertMarkup(XElement element, out string markup)
        => TryGetMarkup(element, out markup);

    private static bool TryGetMarkup(XElement element, out string markup)
    {
        if (!IsNewlineComponent(element))
        {
            markup = string.Empty;
            return false;
        }

        var count = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-count", 1), 0);
        var newlineBuilder = new System.Text.StringBuilder();
        for (var i = 0; i < count; i++)
        {
            newlineBuilder.AppendLine();
        }

        markup = newlineBuilder.ToString();
        return true;
    }

    private static bool IsNewlineComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-newline") is not null;
}
