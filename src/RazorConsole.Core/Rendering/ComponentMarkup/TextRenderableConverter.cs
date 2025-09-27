using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class TextRenderableConverter : IRenderableConverter, IMarkupConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!TryGetRenderable(element, out var markup, out renderable))
        {
            renderable = default!;
            return false;
        }

        return true;
    }

    public bool TryConvert(XElement element, out string markup)
        => TryGetRenderable(element, out markup, out _);

    private static bool TryGetRenderable(XElement element, out string markup, out IRenderable renderable)
    {
        if (!IsTextComponent(element))
        {
            markup = string.Empty;
            renderable = default!;
            return false;
        }

        var style = element.Attribute("data-style")?.Value;
        var isMarkupValue = element.Attribute("data-ismarkup")?.Value;
        var isMarkup = bool.TryParse(isMarkupValue, out var parsed) && parsed;

        var content = element.Value ?? string.Empty;
        markup = ComponentMarkupUtilities.CreateStyledMarkup(style, content, requiresEscape: !isMarkup);
        renderable = new Markup(markup);
        return true;
    }

    private static bool IsTextComponent(XElement element)
        => string.Equals(element.Name.LocalName, "span", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-text") is not null;
}
