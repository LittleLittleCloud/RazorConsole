using System;
using System.Linq;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class TextElementTranslator : IVdomElementTranslator
{
    public int Priority => 10;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!string.Equals(node.TagName, "span", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-text", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string? text;
        if (node.Attributes.TryGetValue("data-content", out var inlineContent) && inlineContent is not null)
        {
            if (node.Children.Any())
            {
                // Prefer explicit content attribute when present and require no additional children.
                return false;
            }

            text = inlineContent;
        }
        else
        {
            text = VdomSpectreTranslator.CollectInnerText(node);
            if (string.IsNullOrWhiteSpace(text))
            {
                // Missing required text content.
                return false;
            }
        }

        var styleAttributes = VdomSpectreTranslator.GetAttribute(node, "data-style");
        if (string.IsNullOrEmpty(styleAttributes))
        {
            renderable = new Markup(text);
        }
        else
        {
            var style = Style.Parse(styleAttributes ?? string.Empty);
            renderable = new Markup(text, style);
        }

        return true;
    }
}
