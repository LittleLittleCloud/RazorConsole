using System;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class SpinnerElementTranslator : IVdomElementTranslator
{
    public int Priority => 60;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.ContainsKey("data-spinner"))
        {
            return false;
        }

        var spinnerType = VdomSpectreTranslator.GetAttribute(node, "data-spinner-type");
        var spinner = ComponentMarkupUtilities.ResolveSpinner(spinnerType);
        var message = VdomSpectreTranslator.GetAttribute(node, "data-message") ?? string.Empty;
        var style = VdomSpectreTranslator.GetAttribute(node, "data-style");
        var autoDismiss = VdomSpectreTranslator.TryGetBoolAttribute(node, "data-auto-dismiss", out var parsed) && parsed;

        var animated = new AnimatedSpinnerRenderable(spinner, message, style, autoDismiss);
        AnimatedRenderableRegistry.Register(animated);

        renderable = animated;
        return true;
    }
}
