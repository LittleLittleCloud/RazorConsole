using System;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlBlockquoteElementTranslator : IVdomElementTranslator
{
    public int Priority => 160;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        var tagName = node.TagName?.ToLowerInvariant();
        if (tagName != "blockquote")
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

        var content = VdomSpectreTranslator.ComposeChildContent(children);
        var panel = new Panel(content)
        {
            Border = BoxBorder.None,
            Padding = new Padding(2, 0, 0, 0)
        };

        renderable = panel;
        return true;
    }
}
