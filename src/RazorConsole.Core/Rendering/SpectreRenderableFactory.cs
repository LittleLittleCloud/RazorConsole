using System;
using System.Collections.Generic;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public static class SpectreRenderableFactory
{
    private static readonly VdomSpectreTranslator Translator = new();

    internal static bool TryCreateRenderable(
        VNode? vdomRoot,
        out IRenderable? renderable,
        out IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
    {
        if (vdomRoot is null)
        {
            throw new InvalidOperationException("A virtual DOM root is required to create Spectre renderables.");
        }

        if (!Translator.TryTranslate(vdomRoot, out renderable, out animatedRenderables) || renderable is null)
        {
            throw new InvalidOperationException("Unable to translate virtual DOM node into a Spectre.Console renderable.");
        }

        return true;
    }
}
