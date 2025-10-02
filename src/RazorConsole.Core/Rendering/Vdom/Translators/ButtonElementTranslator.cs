using System;
using System.Composition;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class ButtonElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(node.TagName, "button", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-button", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var descriptor = ButtonRenderableDescriptorFactory.Create(name => GetAttribute(node, name));
            renderable = ButtonRenderableBuilder.Build(descriptor, children);
            return true;
        }
    }
}
