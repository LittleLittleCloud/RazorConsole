using System;
using System.Composition;
using System.Linq;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class HtmlListElementTranslator : IVdomElementTranslator
    {
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
                if (!TryConvertChildrenToRenderables(listItem.Children, context, out var itemChildren))
                {
                    return false;
                }

                // Create the list item with prefix
                IRenderable itemContent;
                if (itemChildren.Count == 0)
                {
                    itemContent = new Markup(prefix);
                }
                else if (itemChildren.Count == 1)
                {
                    itemContent = new Columns(new IRenderable[] { new Markup(prefix), itemChildren[0] })
                    {
                        Expand = false,
                        Padding = new Padding(0, 0, 0, 0),
                    };
                }
                else
                {
                    itemContent = new Columns(new IRenderable[] { new Markup(prefix), new Rows(itemChildren) { Expand = false } })
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
}
