using System;
using System.Composition;
using RazorConsole.Core.Rendering.Syntax;
using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class SyntaxHighlighterElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-syntax-highlighter", out var marker) || !string.Equals(marker, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var payload = GetAttribute(node, "data-payload");
            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            try
            {
                var model = SyntaxHighlightingService.DecodePayload(payload);
                renderable = new SyntaxRenderable(model);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
