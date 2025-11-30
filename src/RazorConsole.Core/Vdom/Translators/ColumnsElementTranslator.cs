// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Extensions;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class ColumnsElementTranslator : IVdomElementTranslator
{
    public int Priority => 120;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(RazorConsole.Components.Columns))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        var expand = node.GetAttributeValue(nameof(RazorConsole.Components.Columns.Expand), false);
        renderable = new Columns(children)
        {
            Expand = expand,
        };

        return true;
    }
}
