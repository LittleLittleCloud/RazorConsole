using System.Text;
using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class SpinnerComponentMarkupConverter : IComponentMarkupConverter
{
    public bool TryConvert(XElement element, StringBuilder builder)
    {
        if (!IsSpinnerComponent(element))
        {
            return false;
        }

        var message = element.Attribute("data-message")?.Value ?? string.Empty;
        var style = element.Attribute("data-style")?.Value;
        var spinnerType = element.Attribute("data-spinner-type")?.Value;

        var glyph = ComponentMarkupUtilities.ResolveSpinnerGlyph(spinnerType);
        var content = string.IsNullOrWhiteSpace(message)
            ? glyph
            : string.Concat(glyph, " ", message);

        ComponentMarkupUtilities.AppendStyledContent(builder, style, content, requiresEscape: true);
        return true;
    }

    private static bool IsSpinnerComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-spinner") is not null;
}
