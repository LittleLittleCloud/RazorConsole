using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Rendering;
using Spectre.Console;

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
    /// Renders a Razor component to HTML and returns a rich view result including Spectre markup and panel.
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

        if (SpectrePanelFactory.TryCreatePanel(html, out var panel) && panel is not null)
        {
            var markupFromPanel = HtmlToSpectreMarkupConverter.Convert(html);
            return ConsoleViewResult.Create(html, markupFromPanel, panel);
        }

        var markup = HtmlToSpectreMarkupConverter.Convert(html);
        if (string.IsNullOrWhiteSpace(markup))
        {
            markup = "[grey53](no content)[/]";
        }

        var fallbackPanel = new Panel(new Markup(markup))
            .Expand()
            .SquareBorder()
            .BorderColor(Color.Grey53);

        return ConsoleViewResult.Create(html, markup, fallbackPanel);
    }

    /// <summary>
    /// Returns the renderer used for advanced scenarios.
    /// </summary>
    protected RazorComponentRenderer Renderer => _renderer;
}
