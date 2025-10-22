using System;
using System.Collections.Generic;
using System.Linq;
using RazorConsole.Core.Rendering.Markdown;
using RazorConsole.Core.Rendering.Syntax;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class MarkdownElementTranslator : IVdomElementTranslator
{
    private readonly SyntaxHighlightingService _syntaxService;

    public MarkdownElementTranslator(SyntaxHighlightingService syntaxService)
    {
        _syntaxService = syntaxService ?? throw new ArgumentNullException(nameof(syntaxService));
    }

    public int Priority => 90;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("class", out var @class) || !string.Equals(@class, "markdown", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var payload = VdomSpectreTranslator.GetAttribute(node, "data-payload");
        if (string.IsNullOrEmpty(payload))
        {
            return false;
        }

        try
        {
            var model = MarkdownRenderingService.DecodePayload(payload);
            var renderables = new List<IRenderable>();

            foreach (var element in model.Elements)
            {
                var elementRenderable = ConvertMarkdownElement(element);
                if (elementRenderable != null)
                {
                    renderables.Add(elementRenderable);
                }
            }

            renderable = renderables.Count == 1 ? renderables[0] : new Rows(renderables);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private IRenderable? ConvertMarkdownElement(MarkdownElement element)
    {
        switch (element)
        {
            case HeadingElement heading:
                var headingStyle = heading.Level switch
                {
                    1 => new Style(Color.Yellow, decoration: Decoration.Bold),
                    2 => new Style(Color.Cyan1, decoration: Decoration.Bold),
                    3 => new Style(Color.Green, decoration: Decoration.Bold),
                    _ => new Style(Color.White, decoration: Decoration.Bold)
                };
                return new Markup(Markup.Escape(heading.Content), headingStyle);

            case ParagraphElement paragraph:
                return new Markup(Markup.Escape(paragraph.Content));

            case CodeBlockElement codeBlock:
                var syntaxRequest = new SyntaxHighlightRequest(
                    codeBlock.Code,
                    codeBlock.Language,
                    null,
                    codeBlock.ShowLineNumbers,
                    SyntaxOptions.Default);

                var syntaxModel = _syntaxService.Highlight(syntaxRequest);
                return new SyntaxRenderable(syntaxModel);

            case ListElement list:
                if (list.IsOrdered)
                {
                    var numberedItems = new List<IRenderable>();
                    for (int i = 0; i < list.Items.Count; i++)
                    {
                        numberedItems.Add(new Markup($"{i + 1}. {Markup.Escape(list.Items[i])}"));
                    }
                    return new Rows(numberedItems);
                }
                else
                {
                    var bulletItems = list.Items.Select(item => new Markup($"• {Markup.Escape(item)}")).ToList<IRenderable>();
                    return new Rows(bulletItems);
                }

            case QuoteElement quote:
                var quotePanel = new Panel(new Markup(Markup.Escape(quote.Content)))
                {
                    Border = BoxBorder.None,
                    Padding = new Padding(2, 0, 0, 0)
                };
                quotePanel.BorderStyle(new Style(Color.Grey));
                return quotePanel;

            case ThematicBreakElement:
                return new Rule
                {
                    Style = new Style(Color.Grey)
                };

            default:
                return null;
        }
    }
}
