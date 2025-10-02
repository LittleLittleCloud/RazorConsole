using System;
using System.Composition;
using System.Linq;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class TextElementTranslator : IVdomElementTranslator
    {
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

            if (node.Children.Any(child => child.Kind == VNodeKind.Element))
            {
                return false;
            }

            var style = GetAttribute(node, "data-style");
            var isMarkup = TryGetBoolAttribute(node, "data-ismarkup", out var boolValue) && boolValue;
            var content = string.Concat(node.Children.Select(child => child.Kind == VNodeKind.Text ? child.Text : string.Empty)) ?? string.Empty;
            var markup = ComponentMarkupUtilities.CreateStyledMarkup(style, content, requiresEscape: !isMarkup);
            renderable = new Markup(markup);
            return true;
        }
    }
}
