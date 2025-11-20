// Copyright (c) RazorConsole. All rights reserved.

using System.Text;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class SpacerElementTranslator : IVdomElementTranslator
{
    public int Priority => 40;

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

        if (!node.Attributes.ContainsKey("data-spacer"))
        {
            return false;
        }

        var lines = Math.Max(VdomSpectreTranslator.TryGetIntAttribute(node, "data-lines", 1), 0);
        if (lines == 0)
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        var fill = VdomSpectreTranslator.GetAttribute(node, "data-fill");
        var builder = new StringBuilder();

        if (string.IsNullOrEmpty(fill))
        {
            for (var i = 0; i < lines; i++)
            {
                builder.AppendLine();
            }
        }
        else
        {
            var glyph = Markup.Escape(fill[0].ToString());
            for (var i = 0; i < lines; i++)
            {
                builder.Append(glyph);
                builder.AppendLine();
            }
        }

        renderable = new Markup(builder.ToString());
        return true;
    }
}
