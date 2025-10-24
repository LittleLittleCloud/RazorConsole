using System;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlDivElementTranslator : IVdomElementTranslator
{
    public int Priority => 190;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element || !string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (node.Children.Count == 0)
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        if (VdomSpectreTranslator.TryConvertChildrenToBlockInlineRenderable(node.Children, context, out var bir))
        {
            renderable = bir;
            return true;
        }

        renderable = null;
        return false;
    }
}
