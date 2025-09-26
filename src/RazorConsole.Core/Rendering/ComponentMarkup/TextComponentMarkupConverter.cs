using System.Text;
using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class TextComponentMarkupConverter : IComponentMarkupConverter
{
    public bool TryConvert(XElement element, StringBuilder builder)
    {
        if (!IsTextComponent(element))
        {
            return false;
        }

        var style = element.Attribute("data-style")?.Value;
        var isMarkupValue = element.Attribute("data-ismarkup")?.Value;
        var isMarkup = bool.TryParse(isMarkupValue, out var parsed) && parsed;

        var content = element.Value ?? string.Empty;
        ComponentMarkupUtilities.AppendStyledContent(builder, style, content, requiresEscape: !isMarkup);

        return true;
    }

    private static bool IsTextComponent(XElement element)
        => string.Equals(element.Name.LocalName, "span", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-text") is not null;
}
