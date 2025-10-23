using System;
using System.Linq;
using RazorConsole.Core.Rendering.Syntax;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlCodeBlockElementTranslator : IVdomElementTranslator
{
    private readonly SyntaxHighlightingService _syntaxService;

    public HtmlCodeBlockElementTranslator(SyntaxHighlightingService syntaxService)
    {
        _syntaxService = syntaxService ?? throw new ArgumentNullException(nameof(syntaxService));
    }

    public int Priority => 85;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        var tagName = node.TagName?.ToLowerInvariant();
        if (tagName != "pre")
        {
            return false;
        }

        // Look for a code child element
        var codeNode = node.Children.FirstOrDefault(c =>
            c.Kind == VNodeKind.Element &&
            string.Equals(c.TagName, "code", StringComparison.OrdinalIgnoreCase));

        if (codeNode == null)
        {
            // No code element, just render as text
            var text = VdomSpectreTranslator.CollectInnerText(node);
            renderable = new Markup(Markup.Escape(text ?? string.Empty));
            return true;
        }

        // Extract the code content
        var code = VdomSpectreTranslator.CollectInnerText(codeNode);
        if (string.IsNullOrWhiteSpace(code))
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        // Try to detect language from class attribute (e.g., "language-csharp")
        string? language = null;
        if (codeNode.Attributes.TryGetValue("class", out var classAttr) && !string.IsNullOrEmpty(classAttr))
        {
            var classes = classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var langClass = classes.FirstOrDefault(c => c.StartsWith("language-", StringComparison.OrdinalIgnoreCase));
            if (langClass != null)
            {
                language = langClass.Substring("language-".Length);
            }
        }

        // Use syntax highlighting if available
        try
        {
            var request = new SyntaxHighlightRequest(code, language, null, true, SyntaxOptions.Default);
            var model = _syntaxService.Highlight(request);
            var body = new SyntaxRenderable(model);
            renderable = new Rows([new Markup(" "), body, new Markup(" ")]);
            return true;
        }
        catch
        {
            // Fall back to plain text if syntax highlighting fails
            renderable = new Markup(Markup.Escape(code));
            return true;
        }
    }
}
