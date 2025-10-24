using System;
using System.Linq;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlListElementTranslator : IVdomElementTranslator
{
    public int Priority => 180;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        var tagName = node.TagName?.ToLowerInvariant();
        if (tagName != "ul" && tagName != "ol")
        {
            return false;
        }

        var isOrdered = tagName == "ol";

        // Get list items (only process li elements)
        var listItems = node.Children.Where(c => c.Kind == VNodeKind.Element &&
                                                 string.Equals(c.TagName, "li", StringComparison.OrdinalIgnoreCase))
                                      .ToList();

        if (listItems.Count == 0)
        {
            renderable = new Markup(string.Empty);
            return true;
        }

        var itemRenderables = new System.Collections.Generic.List<IRenderable>();

        for (int i = 0; i < listItems.Count; i++)
        {
            var listItem = listItems[i];
            var prefix = isOrdered ? $"{i + 1}. " : "• ";

            // Convert list item children to renderables
            if (!VdomSpectreTranslator.TryConvertChildrenToBlockInlineRenderable(listItem.Children, context, out var itemChildRenderable))
            {
                return false;
            }

            // Create the list item with prefix
            IRenderable itemContent;
            if (itemChildRenderable is null)
            {
                itemContent = new Markup(prefix);
            }
            else 
            {
                itemContent = new Columns(new IRenderable[] { new Markup(prefix), itemChildRenderable })
                {
                    Expand = false,
                    Padding = new Padding(0, 0, 0, 0),
                };
            }

            itemRenderables.Add(itemContent);
        }

        renderable = new Rows(itemRenderables)
        {
            Expand = false,
        };

        return true;
    }
}
