using System;
using System.Text;
using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class NewlineComponentMarkupConverter : IComponentMarkupConverter
{
    public bool TryConvert(XElement element, StringBuilder builder)
    {
        if (!IsNewlineComponent(element))
        {
            return false;
        }

        var count = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-count", 1), 0);
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine();
        }

        return true;
    }

    private static bool IsNewlineComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-newline") is not null;
}
