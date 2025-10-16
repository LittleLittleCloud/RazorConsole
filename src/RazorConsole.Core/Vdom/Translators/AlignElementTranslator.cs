using System;
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

        if (!node.Attributes.TryGetValue("class", out var value) || !string.Equals(value, "align", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        if (children is not { Count: 1 })
        {
            return false;
        }

        var content = children[0];
        var horizontal = VdomSpectreTranslator.ParseHorizontalAlignment(VdomSpectreTranslator.GetAttribute(node, "data-horizontal"));
        var vertical = VdomSpectreTranslator.ParseVerticalAlignment(VdomSpectreTranslator.GetAttribute(node, "data-vertical"));
        var width = VdomSpectreTranslator.ParseOptionalPositiveInt(VdomSpectreTranslator.GetAttribute(node, "data-width"));
        var height = VdomSpectreTranslator.ParseOptionalPositiveInt(VdomSpectreTranslator.GetAttribute(node, "data-height"));

        var align = new MeasuredAlign(content, horizontal, vertical)
        {
            Width = width,
            Height = height,
        };

        renderable = align;
        return true;
    }
}
