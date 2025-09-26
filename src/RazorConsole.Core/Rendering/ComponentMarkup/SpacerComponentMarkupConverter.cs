using System;
using System.Text;
using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class SpacerComponentMarkupConverter : IComponentMarkupConverter
{
    public bool TryConvert(XElement element, StringBuilder builder)
    {
        if (!IsSpacerComponent(element))
        {
            return false;
        }

        var lines = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-lines", 1), 0);
        if (lines == 0)
        {
            return true;
        }

        var fill = element.Attribute("data-fill")?.Value;
        if (string.IsNullOrEmpty(fill))
        {
            for (var i = 0; i < lines; i++)
            {
                builder.AppendLine();
            }
        }
        else
        {
            var fillChar = fill[0].ToString();
            for (var i = 0; i < lines; i++)
            {
                ComponentMarkupUtilities.AppendStyledContent(builder, style: null, fillChar, requiresEscape: true);
                if (i < lines - 1)
                {
                    builder.AppendLine();
                }
            }
        }

        return true;
    }

    private static bool IsSpacerComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-spacer") is not null;
}
