using System;
using System.Globalization;
using System.Linq;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class ColumnsElementTranslator : IVdomElementTranslator
{
    public int Priority => 120;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (!IsColumnsNode(node))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        var expand = VdomSpectreTranslator.GetAttribute(node, "data-expand") == "true";
        renderable = new Columns(children)
        {
            Expand = expand,
        };

        return true;
    }

    private static bool IsColumnsNode(VNode node)
    {
        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (node.Attributes.TryGetValue("class", out var value) && string.Equals(value, "columns", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
