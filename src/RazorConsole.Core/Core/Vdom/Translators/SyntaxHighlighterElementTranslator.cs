// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Syntax;
using RazorConsole.Core.Vdom;

using Spectre.Console.Rendering;

namespace RazorConsole.Core.Vdom.Translators;

public sealed class SyntaxHighlighterElementTranslator : IVdomElementTranslator
{
    public int Priority => 90;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("class", out var @class) || !string.Equals(@class, "syntax-highlighter", StringComparison.OrdinalIgnoreCase))
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
