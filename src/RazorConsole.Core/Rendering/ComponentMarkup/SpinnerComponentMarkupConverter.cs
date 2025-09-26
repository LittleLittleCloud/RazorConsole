using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class SpinnerComponentMarkupConverter : IComponentMarkupConverter
{
    public bool TryConvert(XElement element, out ComponentRenderable renderable)
    {
        if (!IsSpinnerComponent(element))
        {
            renderable = default;
            return false;
        }

        var message = element.Attribute("data-message")?.Value ?? string.Empty;
        var style = element.Attribute("data-style")?.Value;
        var spinnerType = element.Attribute("data-spinner-type")?.Value;

        var glyph = ComponentMarkupUtilities.ResolveSpinnerGlyph(spinnerType);
        var content = string.IsNullOrWhiteSpace(message)
            ? glyph
            : string.Concat(glyph, " ", message);

        renderable = ComponentMarkupUtilities.CreateStyledRenderable(style, content, requiresEscape: true);
        return true;
    }

    private static bool IsSpinnerComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-spinner") is not null;
}
