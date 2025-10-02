using System;
using System.Composition;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class RowsElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-rows", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var rows = new Rows(children);

            if (string.Equals(GetAttribute(node, "data-expand"), "true", StringComparison.OrdinalIgnoreCase))
            {
                rows.Expand();
            }

            renderable = rows;
            return true;
        }
    }
}
