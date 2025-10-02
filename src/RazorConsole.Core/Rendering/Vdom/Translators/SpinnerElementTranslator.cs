using System;
using System.Composition;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class SpinnerElementTranslator : IVdomElementTranslator
    {
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

            var spinnerType = GetAttribute(node, "data-spinner-type");
            var spinner = ComponentMarkupUtilities.ResolveSpinner(spinnerType);
            var message = GetAttribute(node, "data-message") ?? string.Empty;
            var style = GetAttribute(node, "data-style");
            var autoDismiss = TryGetBoolAttribute(node, "data-auto-dismiss", out var parsed) && parsed;

            var animated = new AnimatedSpinnerRenderable(spinner, message, style, autoDismiss);
            AnimatedRenderableRegistry.Register(animated);

            renderable = animated;
            return true;
        }
    }
}
