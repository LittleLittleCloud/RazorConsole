using System;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlHrElementTranslator : IVdomElementTranslator
{
    public int Priority => 170;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        var tagName = node.TagName?.ToLowerInvariant();
        if (tagName != "hr")
        {
            return false;
        }

        renderable = new Rule
        {
            Style = new Style(Color.Grey)
        };

        return true;
    }
}
