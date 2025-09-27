using System;
using System.Collections.Generic;
using System.Xml.Linq;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public static class SpectreRenderableFactory
{
    public static bool TryCreateRenderable(string html, out IRenderable? renderable)
        => TryCreateRenderable(html, out renderable, out _);

    internal static bool TryCreateRenderable(string html, out IRenderable? renderable, out IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
    {
        renderable = null;
        animatedRenderables = Array.Empty<IAnimatedConsoleRenderable>();

        if (string.IsNullOrWhiteSpace(html))
        {
            return false;
        }

        try
        {
            var document = XDocument.Parse(html, LoadOptions.None);
            var root = document.Root;
            if (root is null)
            {
                return false;
            }

            var animations = new List<IAnimatedConsoleRenderable>();
            using (AnimatedRenderableRegistry.PushScope(animations))
            {
                if (HtmlToSpectreRenderableConverter.TryConvertToRenderable(root, out var candidate))
                {
                    renderable = candidate;
                    animatedRenderables = animations;
                    return true;
                }
            }

            return false;
        }
        catch
        {
            renderable = null;
            animatedRenderables = Array.Empty<IAnimatedConsoleRenderable>();
            return false;
        }
    }
}
