// Copyright (c) RazorConsole. All rights reserved.

using System.Text;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class NewlineElementTranslator : IVdomElementTranslator
{
    public int Priority => 50;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.ContainsKey("data-newline"))
        {
            return false;
        }

        var count = Math.Max(VdomSpectreTranslator.TryGetIntAttribute(node, "data-count", 1), 0);
        if (count == 0)
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            builder.AppendLine();
        }

        renderable = new Markup(builder.ToString());
        return true;
    }
}
