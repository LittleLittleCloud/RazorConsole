// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Extensions;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class CanvasElementTranslator : IVdomElementTranslator
{
    public int Priority => 100;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(RazorConsole.Components.SpectreCanvas))
        {
            return false;
        }

        if (!node.TryGetAttributeValue<int>(nameof(RazorConsole.Components.SpectreCanvas.CanvasWidth), out var width))
        {
            return false;
        }

        if (!node.TryGetAttributeValue<int>(nameof(RazorConsole.Components.SpectreCanvas.CanvasHeight), out var height))
        {
            return false;
        }

        var pixelWidth = node.GetAttributeValue(nameof(RazorConsole.Components.SpectreCanvas.PixelWidth), 2);
        var scale = node.GetAttributeValue(nameof(RazorConsole.Components.SpectreCanvas.Scale), false);
        var maxWidth = node.GetAttributeValue<int?>(nameof(RazorConsole.Components.SpectreCanvas.MaxWidth));

        if (!node.TryGetAttributeValue<(int x, int y, Color color)[]>(nameof(RazorConsole.Components.SpectreCanvas.Pixels), out var pixels) || pixels is null || pixels.Length == 0)
        {
            return false;
        }

        var canvas = new Canvas(width, height);

        if (maxWidth.HasValue)
        {
            canvas.MaxWidth = maxWidth.Value;
        }

        canvas.PixelWidth = pixelWidth;
        canvas.Scale = scale;

        foreach (var p in pixels)
        {
            canvas.SetPixel(p.x, p.y, p.color);
        }

        renderable = canvas;
        return true;
    }
}
