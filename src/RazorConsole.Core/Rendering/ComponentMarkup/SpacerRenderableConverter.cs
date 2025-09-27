using System;
using System.Xml.Linq;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class SpacerRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out ComponentRenderable renderable)
    {
        if (!IsSpacerComponent(element))
        {
            renderable = default;
            return false;
        }

        var lines = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-lines", 1), 0);
        if (lines == 0)
        {
            renderable = new ComponentRenderable(string.Empty, new Markup(string.Empty));
            return true;
        }

        var fill = element.Attribute("data-fill")?.Value;
        var spacerBuilder = new System.Text.StringBuilder();

        if (string.IsNullOrEmpty(fill))
        {
            for (var i = 0; i < lines; i++)
            {
                spacerBuilder.AppendLine();
            }
        }
        else
        {
            var fillChar = fill[0].ToString();
            var escaped = Markup.Escape(fillChar);
            for (var i = 0; i < lines; i++)
            {
                spacerBuilder.Append(escaped);
                spacerBuilder.AppendLine();
            }
        }

        var markupText = spacerBuilder.ToString();
        renderable = new ComponentRenderable(markupText, new Markup(markupText));
        return true;
    }

    private static bool IsSpacerComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-spacer") is not null;
}
