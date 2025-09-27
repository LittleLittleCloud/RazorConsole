using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Input;
using RazorConsole.Core.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Base class that coordinates rendering Razor components into Spectre.Console output.
/// </summary>
public abstract class ConsoleController
{
    private readonly RazorComponentRenderer _renderer;

    protected ConsoleController(RazorComponentRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    /// <summary>
    /// Executes the controller and returns the next navigation intent.
    /// Override this in derived controllers to implement interaction loops.
    /// </summary>
    public abstract Task<NavigationIntent> ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a Razor component to HTML and returns a rich view result including Spectre markup and renderable content.
    /// </summary>
    /// <typeparam name="TComponent">Component type.</typeparam>
    /// <param name="parameters">Optional parameters to pass to the component.</param>
    protected async Task<ConsoleViewResult> RenderViewAsync<TComponent>(object? parameters = null)
        where TComponent : IComponent
    {
        var html = await _renderer.RenderAsync<TComponent>(parameters).ConfigureAwait(false);
        return CreateViewResult(html);
    }

    /// <summary>
    /// Builds a <see cref="ConsoleViewResult"/> from a pre-rendered HTML fragment.
    /// </summary>
    /// <param name="html">HTML fragment returned by the Razor renderer.</param>
    protected ConsoleViewResult CreateViewResult(string html)
    {
        if (html is null)
        {
            throw new ArgumentNullException(nameof(html));
        }

        if (SpectreRenderableFactory.TryCreateRenderable(html, out var renderable) && renderable is not null)
        {
            return ConsoleViewResult.Create(html, renderable);
        }
        else
        {
            // Fallback to show a html fragment and let users know rendering failed.
            var fallbackRenderable = new Panel(new Markup(html))
            .Expand()
            .Header(" [red]Rendering Error[/] ")
            .SquareBorder()
            .BorderColor(Color.Grey53);

            return ConsoleViewResult.Create(html, fallbackRenderable);
        }
    }

    /// <summary>
    /// Writes the provided view to the console output.
    /// </summary>
    /// <param name="view">Rendered view.</param>
    protected void WriteView(ConsoleViewResult view)
    {
        if (view is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        view.WriteTo(ConsoleOutput);
    }

    /// <summary>
    /// Clears the console output.
    /// </summary>
    protected virtual void ClearOutput() => AnsiConsole.Clear();

    /// <summary>
    /// Writes markup to the console.
    /// </summary>
    protected void WriteMarkupLine(string markup)
        => ConsoleOutput.MarkupLine(markup ?? string.Empty);

    /// <summary>
    /// Writes an empty line to the console.
    /// </summary>
    protected void WriteLine()
        => ConsoleOutput.WriteLine();

    /// <summary>
    /// Writes the view when the HTML differs from the last rendered HTML.
    /// </summary>
    protected bool WriteViewIfChanged(ConsoleViewResult view, ref string? lastHtml)
    {
        if (view is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        if (string.Equals(view.Html, lastHtml, StringComparison.Ordinal))
        {
            return false;
        }

        ClearOutput();
        WriteView(view);
        lastHtml = view.Html;
        return true;
    }

    /// <summary>
    /// Reads a line from the console and returns a normalized <see cref="ConsoleInputContext"/>.
    /// </summary>
    protected ConsoleInputContext ReadLineInput(string prompt)
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            ConsoleOutput.Markup(prompt);
        }

        var input = System.Console.ReadLine();
        return ConsoleInputContext.FromText(input).Normalize();
    }

    /// <summary>
    /// Returns the Spectre console used for output operations.
    /// </summary>
    protected virtual IAnsiConsole ConsoleOutput => AnsiConsole.Console;

    /// <summary>
    /// Returns the renderer used for advanced scenarios.
    /// </summary>
    protected RazorComponentRenderer Renderer => _renderer;
}
