using System;
using System.Composition;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class HtmlDivElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element || !string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            if (children.Count == 0)
            {
                renderable = new Markup(string.Empty);
                return true;
            }

            if (children.Count == 1)
            {
                renderable = children[0];
                return true;
            }

            renderable = new Columns(children)
            {
                Expand = false,
                Padding = new Padding(0, 0, 0, 0),
            };

            return true;
        }
    }
}
