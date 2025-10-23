using System;
using System.Linq;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class ParagraphElementTranslator : IVdomElementTranslator
{
    public int Priority => 30;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element || !string.Equals(node.TagName, "p", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Try to convert children to renderables (handles mixed text + inline elements)
        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        if (children.Count == 0)
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        renderable = VdomSpectreTranslator.ComposeChildContent(children);
        return true;
    }
}
