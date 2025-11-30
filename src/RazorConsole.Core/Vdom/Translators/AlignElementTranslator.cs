// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Extensions;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Vdom;

using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class AlignElementTranslator : IVdomElementTranslator
{
    public int Priority => 150;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(Components.Align))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToBlockInlineRenderable(node.Children, context, out var children) || children is null)
        {
            return false;
        }

        var horizontal = node.GetAttributeValue(nameof(Components.Align.Horizontal), HorizontalAlignment.Left);
        var vertical = node.GetAttributeValue<VerticalAlignment?>(nameof(Components.Align.Vertical));
        var width = node.GetAttributeValue<int?>(nameof(Components.Align.Width));
        var height = node.GetAttributeValue<int?>(nameof(Components.Align.Height));

        var align = new MeasuredAlign(children, horizontal, vertical)
        {
            Width = width,
            Height = height,
        };

        renderable = align;
        return true;
    }
}
