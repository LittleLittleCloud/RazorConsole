using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class SpinnerRenderableConverter : IRenderableConverter, IMarkupConverter
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
        if (!IsSpinnerComponent(element))
        {
            markup = string.Empty;
            return false;
        }

        var message = element.Attribute("data-message")?.Value ?? string.Empty;
        var style = element.Attribute("data-style")?.Value;
        var spinnerType = element.Attribute("data-spinner-type")?.Value;

        var glyph = ComponentMarkupUtilities.ResolveSpinnerGlyph(spinnerType);
        var content = string.IsNullOrWhiteSpace(message)
            ? glyph
            : string.Concat(glyph, " ", message);

        markup = ComponentMarkupUtilities.CreateStyledMarkup(style, content, requiresEscape: true);
        return true;
    }

    private static bool IsSpinnerComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-spinner") is not null;
}
