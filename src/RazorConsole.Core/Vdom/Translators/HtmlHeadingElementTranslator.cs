using System;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlHeadingElementTranslator : IVdomElementTranslator
{
    public int Priority => 150;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        var tagName = node.TagName?.ToLowerInvariant();
        if (!IsHeadingTag(tagName))
        {
            return false;
        }

        var innerText = VdomSpectreTranslator.CollectInnerText(node);
        if (string.IsNullOrWhiteSpace(innerText))
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        var style = GetHeadingStyle(tagName);
        var prefix = GetHeadingPrefix(tagName);
        var markup = Markup.Escape($"{prefix}{innerText}");
        renderable = new Markup(markup, style);
        return true;
    }

    private static string GetHeadingPrefix(string? tagName)
    {
        return tagName switch
        {
            "h1" => "# ",
            "h2" => "## ",
            "h3" => "### ",
            "h4" => "#### ",
            "h5" => "##### ",
            "h6" => "###### ",
            _ => string.Empty
        };
    }

    private static bool IsHeadingTag(string? tagName)
    {
        return tagName switch
        {
            "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => true,
            _ => false
        };
    }

    private static Style GetHeadingStyle(string? tagName)
    {
        return tagName switch
        {
            "h1" => new Style(Color.Yellow, decoration: Decoration.Bold),
            "h2" => new Style(Color.Cyan1, decoration: Decoration.Bold),
            "h3" => new Style(Color.Green, decoration: Decoration.Bold),
            "h4" => new Style(Color.Blue, decoration: Decoration.Bold),
            "h5" => new Style(Color.Magenta1, decoration: Decoration.Bold),
            "h6" => new Style(Color.Grey, decoration: Decoration.Bold),
            _ => new Style(Color.White, decoration: Decoration.Bold)
        };
    }
}
