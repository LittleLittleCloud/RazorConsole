// Copyright (c) RazorConsole. All rights reserved.

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

        if (node.Kind != VNodeKind.Element)
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

        node.Attrs.TryGetValue(nameof(Components.Align.Horizontal), out var ha);
        var horizontal = (HorizontalAlignment?)ha ?? HorizontalAlignment.Left;
        node.Attrs.TryGetValue(nameof(Components.Align.Vertical), out var va);
        var vertical = (VerticalAlignment?)va;
        node.Attrs.TryGetValue(nameof(Components.Align.Width), out var w);
        var width = (int?)w;
        node.Attrs.TryGetValue(nameof(Components.Align.Height), out var h);
        var height = (int?)h;

        var align = new MeasuredAlign(children, horizontal, vertical)
        {
            Width = width,
            Height = height,
        };

        renderable = align;
        return true;
    }
}
