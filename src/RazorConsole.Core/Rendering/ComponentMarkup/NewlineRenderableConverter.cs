using System;
using System.Xml.Linq;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class NewlineRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out ComponentRenderable renderable)
    {
        if (!IsNewlineComponent(element))
        {
            renderable = default;
            return false;
        }

        var count = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-count", 1), 0);
        var newlineBuilder = new System.Text.StringBuilder();
        for (var i = 0; i < count; i++)
        {
            newlineBuilder.AppendLine();
        }

        var markupText = newlineBuilder.ToString();
        renderable = new ComponentRenderable(markupText, new Markup(markupText));
        return true;
    }

    private static bool IsNewlineComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-newline") is not null;
}
