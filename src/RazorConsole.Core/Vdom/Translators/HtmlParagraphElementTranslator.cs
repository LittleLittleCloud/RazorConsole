using System;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlParagraphElementTranslator : IVdomElementTranslator
{
    public int Priority => 195;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        var tagName = node.TagName?.ToLowerInvariant();
        if (tagName != "p")
        {
            return false;
        }

        // Convert children to renderables
        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        if (children.Count == 0)
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        // If single child, just return it
        if (children.Count == 1)
        {
            renderable = children[0];
            return true;
        }

        // Multiple children - compose them as column
        renderable = new Columns(children)
        {
            Expand = false,
            Padding = new Padding(0, 0, 0, 0),
        };

        return true;
    }
}
