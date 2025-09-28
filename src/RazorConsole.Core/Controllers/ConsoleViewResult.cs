using System;
using System.Collections.Generic;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Represents the outcome of rendering a Razor component for the console.
/// </summary>
public sealed class ConsoleViewResult
{
    private ConsoleViewResult(string html, IRenderable renderable, IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables, VNode? vdomRoot)
    {
        Html = html;
        Renderable = renderable;
        AnimatedRenderables = animatedRenderables;
        VdomRoot = vdomRoot;
    }

    /// <summary>
    /// Gets the raw HTML produced by the Razor renderer.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the Spectre.Console renderable representation of the rendered view.
    /// </summary>
    public IRenderable Renderable { get; }

    /// <summary>
    /// Gets animated renderables that require live display refreshes.
    /// </summary>
    internal IReadOnlyCollection<IAnimatedConsoleRenderable> AnimatedRenderables { get; }

    /// <summary>
    /// Gets the virtual DOM representation of the rendered view.
    /// </summary>
    internal VNode? VdomRoot { get; }

    /// <summary>
    /// Writes the renderable to the provided console.
    /// </summary>
    /// <param name="console">Spectre console instance.</param>
    public void WriteTo(IAnsiConsole console)
    {
        if (console is null)
        {
            throw new ArgumentNullException(nameof(console));
        }

        console.Write(Renderable);
    }

    internal static ConsoleViewResult Create(string html, IRenderable renderable, IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
    {
        HtmlVdomConverter.TryConvert(html, out var root);
        return Create(html, root, renderable, animatedRenderables);
    }

    internal static ConsoleViewResult Create(
        string html,
        VNode? vdomRoot,
        IRenderable renderable,
        IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
        => new(html, renderable, animatedRenderables, vdomRoot);
}
