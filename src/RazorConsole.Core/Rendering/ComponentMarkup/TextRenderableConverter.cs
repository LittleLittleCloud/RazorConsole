using System.Xml.Linq;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class TextRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out ComponentRenderable renderable)
    {
        if (!IsTextComponent(element))
        {
            renderable = default;
            return false;
        }

        var style = element.Attribute("data-style")?.Value;
        var isMarkupValue = element.Attribute("data-ismarkup")?.Value;
        var isMarkup = bool.TryParse(isMarkupValue, out var parsed) && parsed;

        var content = element.Value ?? string.Empty;
        renderable = ComponentMarkupUtilities.CreateStyledRenderable(style, content, requiresEscape: !isMarkup);
        return true;
    }

    private static bool IsTextComponent(XElement element)
        => string.Equals(element.Name.LocalName, "span", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-text") is not null;
}
