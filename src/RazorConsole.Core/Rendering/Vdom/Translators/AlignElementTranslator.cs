using System;
using System.Composition;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class AlignElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-align", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var content = ComposeChildContent(children);
            var horizontal = ParseHorizontalAlignment(GetAttribute(node, "data-horizontal"));
            var vertical = ParseVerticalAlignment(GetAttribute(node, "data-vertical"));
            var width = ParseOptionalPositiveInt(GetAttribute(node, "data-width"));
            var height = ParseOptionalPositiveInt(GetAttribute(node, "data-height"));

            var align = new Align(content, horizontal, vertical)
            {
                Width = width,
                Height = height,
            };

            renderable = align;
            return true;
        }
    }
}
