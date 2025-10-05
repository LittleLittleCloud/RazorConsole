using System;
using System.Composition;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class ParagraphElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element || !string.Equals(node.TagName, "p", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // the child node of <p> must be text nodes only
            if (!node.Children.All(c => c.Kind == VNodeKind.Text))
            {
                return false;
            }

            var children = node.Children.Select(c => c.Text).ToList();

            if (children.Count == 0)
            {
                renderable = new Markup(string.Empty);
                return true;
            }
            else
            {
                renderable = new Markup(string.Join(string.Empty, children));
                return true;
            }
        }
    }
}
