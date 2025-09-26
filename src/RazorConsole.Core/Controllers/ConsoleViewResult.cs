using System;
using Spectre.Console;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Represents the outcome of rendering a Razor component for the console.
/// </summary>
public sealed class ConsoleViewResult
{
    private ConsoleViewResult(string html, string markup, Panel panel)
    {
        Html = html;
        Markup = markup;
        Panel = panel;
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
    /// Gets the Spectre.Console panel representation of the rendered view.
    /// </summary>
    public Panel Panel { get; }

    /// <summary>
    /// Writes the panel to the provided console.
    /// </summary>
    /// <param name="console">Spectre console instance.</param>
    public void WriteTo(IAnsiConsole console)
    {
        if (console is null)
        {
            throw new ArgumentNullException(nameof(console));
        }

        console.Write(Panel);
    }

    internal static ConsoleViewResult Create(string html, string markup, Panel panel)
        => new(html, markup, panel);

    internal static ConsoleViewResult Empty { get; } = new(
        string.Empty,
        string.Empty,
        new Panel(new Markup(string.Empty))
            .Border(BoxBorder.None)
            .Expand());
}
