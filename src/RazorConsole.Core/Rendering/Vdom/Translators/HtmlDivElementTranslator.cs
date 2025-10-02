using System;
using System.Composition;
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

            var rows = new Rows(children);

            if (ShouldExpand(node))
            {
                rows.Expand();
            }

            renderable = rows;
            return true;
        }

        private static bool ShouldExpand(VNode node)
        {
            if (TryGetBoolAttribute(node, "data-expand", out var expand) && expand)
            {
                return true;
            }

            return HasClass(node, "row") || HasClass(node, "w-100") || HasClass(node, "d-block");
        }
    }
}
