// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Extensions;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class PadderElementTranslator : IVdomElementTranslator
{
    public int Priority => 140;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(RazorConsole.Components.Padder))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        var content = VdomSpectreTranslator.ComposeChildContent(children);
        var padding = node.GetAttributeValue(nameof(RazorConsole.Components.Padder.Padding), new Padding(0, 0, 0, 0));
        var padder = new Padder(content, padding);

        renderable = padder;
        return true;
    }
}
