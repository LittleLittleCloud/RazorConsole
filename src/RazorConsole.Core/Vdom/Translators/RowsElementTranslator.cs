using System;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class RowsElementTranslator : IVdomElementTranslator
{
    public int Priority => 110;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("class", out var value) || !string.Equals(value, "rows", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        var expand = VdomSpectreTranslator.GetAttribute(node, "data-expand") == "true";

        renderable = new Rows(children)
        {
            Expand = expand,
        };

        return true;
    }
}
