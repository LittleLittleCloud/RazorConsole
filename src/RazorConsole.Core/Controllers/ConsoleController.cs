using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.HotReload;
using RazorConsole.Core.Input;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
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

        if (SpectreRenderableFactory.TryCreateRenderable(html, out var renderable, out var animatedRenderables) && renderable is not null)
        {
            return ConsoleViewResult.Create(html, renderable, animatedRenderables);
        }
        else
        {
            // Fallback to show a html fragment and let users know rendering failed.
            var fallbackRenderable = new Panel(new Markup(html))
            .Expand()
            .Header(" [red]Rendering Error[/] ")
            .SquareBorder()
            .BorderColor(Color.Grey53);

            return ConsoleViewResult.Create(html, fallbackRenderable, Array.Empty<IAnimatedConsoleRenderable>());
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
    /// Runs a Spectre.Console live display using an initial view and invokes a callback for updates.
    /// </summary>
    /// <param name="initialView">The initial view to render.</param>
    /// <param name="handler">Callback invoked within the live display scope.</param>
    /// <param name="options">Optional live display configuration.</param>
    /// <param name="cancellationToken">Cancellation token that is propagated to the handler.</param>
    protected Task RunLiveDisplayAsync(ConsoleViewResult initialView, Func<ConsoleLiveDisplayContext, CancellationToken, Task> handler, ConsoleLiveDisplayOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        return RunLiveDisplayAsync<object?>(initialView, async (context, token) =>
        {
            await handler(context, token).ConfigureAwait(false);
            return null;
        }, options, cancellationToken);
    }

    /// <summary>
    /// Runs a Spectre.Console live display using an initial view and invokes a callback for updates.
    /// </summary>
    /// <typeparam name="TResult">Result type returned by the callback.</typeparam>
    /// <param name="initialView">The initial view to render.</param>
    /// <param name="handler">Callback invoked within the live display scope.</param>
    /// <param name="options">Optional live display configuration.</param>
    /// <param name="cancellationToken">Cancellation token that is propagated to the handler.</param>
    protected Task<TResult> RunLiveDisplayAsync<TResult>(ConsoleViewResult initialView, Func<ConsoleLiveDisplayContext, CancellationToken, Task<TResult>> handler, ConsoleLiveDisplayOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (initialView is null)
        {
            throw new ArgumentNullException(nameof(initialView));
        }

        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var displayOptions = (options ?? ConsoleLiveDisplayOptions.Default).Clone();

        var liveDisplay = AnsiConsole.Live(initialView.Renderable);
        liveDisplay.AutoClear = displayOptions.AutoClear;
        liveDisplay.Overflow = displayOptions.Overflow;
        liveDisplay.Cropping = displayOptions.Cropping;


        return liveDisplay.StartAsync(async liveContext =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var controllerContext = ConsoleLiveDisplayContext.Create(liveContext, initialView);
#if DEBUG
            HotReloadService.UpdateApplicationEvent += (_) =>
            {
                Console.WriteLine("Hot reload event received, refreshing live display...");
                controllerContext.Refresh();
            };
#endif

            return await handler(controllerContext, cancellationToken).ConfigureAwait(false);
        });
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
