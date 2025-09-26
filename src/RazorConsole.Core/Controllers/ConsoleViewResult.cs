using System;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Represents the outcome of rendering a Razor component for the console.
/// </summary>
public sealed class ConsoleViewResult
{
    private ConsoleViewResult(string html, string markup, IRenderable renderable)
    {
        Html = html;
        Markup = markup;
        Renderable = renderable;
    }

    /// <summary>
    /// Gets the raw HTML produced by the Razor renderer.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the Spectre.Console markup equivalent of the rendered HTML.
    /// </summary>
    public string Markup { get; }

    /// <summary>
    /// Gets the Spectre.Console renderable representation of the rendered view.
    /// </summary>
    public IRenderable Renderable { get; }

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

    internal static ConsoleViewResult Create(string html, string markup, IRenderable renderable)
        => new(html, markup, renderable);

    internal static ConsoleViewResult Empty { get; } = new(
        string.Empty,
        string.Empty,
        new Panel(new Markup(string.Empty))
            .Border(BoxBorder.None)
            .Expand());
}
