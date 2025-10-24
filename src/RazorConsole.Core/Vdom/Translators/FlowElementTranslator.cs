using System;
using System.Collections.Generic;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

/// <summary>
/// Translator for flow elements that intelligently mix block and inline content.
/// Elements with data-flow="block" or data-flow="inline" control their layout behavior.
/// </summary>
public sealed class FlowElementTranslator : IVdomElementTranslator
{
    public int Priority => 45;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-flow", out var flowType) || string.IsNullOrEmpty(flowType))
        {
            return false;
        }

        // Convert children to renderable items with block/inline designation
        var items = new List<BlockInlineRenderable.RenderableItem>();

        foreach (var child in node.Children)
        {
            if (!context.TryTranslate(child, out var childRenderable) || childRenderable is null)
            {
                continue;
            }

            // Determine if this child should be block or inline based on the element type
            var isBlock = ShouldBeBlock(child);
            
            if (isBlock)
            {
                items.Add(BlockInlineRenderable.Block(childRenderable));
            }
            else
            {
                items.Add(BlockInlineRenderable.Inline(childRenderable));
            }
        }

        if (items.Count == 0)
        {
            return false;
        }

        renderable = new BlockInlineRenderable(items);
        return true;
    }

    private static bool ShouldBeBlock(VNode node)
    {
        // Check for explicit data-display attribute
        if (node.Attributes.TryGetValue("data-display", out var display))
        {
            if (string.Equals(display, "block", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(display, "inline", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Default block elements based on tag name
        if (node.TagName is not null)
        {
            return node.TagName.ToLowerInvariant() switch
            {
                // Block-level HTML elements
                "div" => true,
                "p" => true,
                "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => true,
                "panel" => true,
                "table" => true,
                "ul" or "ol" => true,
                "pre" => true,
                "blockquote" => true,
                
                // Inline elements
                "span" => false,
                "strong" or "b" => false,
                "em" or "i" => false,
                "code" => false,
                "a" => false,
                "mark" => false,
                
                // Default to inline for unknown elements
                _ => false,
            };
        }

        // Text nodes are inline
        return false;
    }
}
