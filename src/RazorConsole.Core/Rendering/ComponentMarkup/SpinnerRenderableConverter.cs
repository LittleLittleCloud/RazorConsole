using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(SpinnerRenderableConverter))]
public sealed class SpinnerRenderableConverter : IRenderableConverter, IMarkupConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!TryGetSpinnerParameters(element, out var spinner, out var message, out var style, out var autoDismiss))
        {
            renderable = default!;
            return false;
        }

        var animatedRenderable = new AnimatedSpinnerRenderable(spinner, message, style, autoDismiss);
        AnimatedRenderableRegistry.Register(animatedRenderable);

        renderable = animatedRenderable;
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

    private static bool TryGetSpinnerParameters(XElement element, out Spinner spinner, out string? message, out string? style, out bool autoDismiss)
    {
        if (!IsSpinnerComponent(element))
        {
            spinner = Spinner.Known.Dots;
            message = null;
            style = null;
            autoDismiss = false;
            return false;
        }

        message = element.Attribute("data-message")?.Value ?? string.Empty;
        style = element.Attribute("data-style")?.Value;
        var spinnerType = element.Attribute("data-spinner-type")?.Value;
        spinner = ComponentMarkupUtilities.ResolveSpinner(spinnerType);
        autoDismiss = bool.TryParse(element.Attribute("data-auto-dismiss")?.Value, out var parsed) && parsed;
        return true;
    }

    private static bool IsSpinnerComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && element.Attribute("data-spinner") is not null;
}
