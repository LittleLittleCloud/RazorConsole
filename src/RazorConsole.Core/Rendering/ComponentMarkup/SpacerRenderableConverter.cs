using System;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(SpacerRenderableConverter))]
public sealed class SpacerRenderableConverter : IRenderableConverter, IMarkupConverter
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

    public bool TryConvert(XElement element, out string markup)
        => TryGetMarkup(element, out markup);

    private static bool TryGetMarkup(XElement element, out string markup)
    {
        if (!IsSpacerComponent(element))
        {
            markup = string.Empty;
            return false;
        }

        var lines = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-lines", 1), 0);
        if (lines == 0)
        {
            markup = string.Empty;
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

        markup = spacerBuilder.ToString();
        return true;
    }

    private static bool IsSpacerComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-spacer") is not null;
}
