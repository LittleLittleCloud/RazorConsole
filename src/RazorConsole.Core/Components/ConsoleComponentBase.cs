using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Rendering;
using Spectre.Console.Rendering;

namespace RazorConsole.Components;

/// <summary>
/// Base component that exposes helpers for interacting with the console rendering pipeline.
/// </summary>
public abstract class ConsoleComponentBase : ComponentBase
{

    /// <summary>
    /// Gets or sets the accessor used to interact with the active live display context.
    /// </summary>
    [Inject]
    protected internal LiveDisplayContextAccessor? LiveDisplayContextAccessor { get; set; }

    /// <summary>
    /// Gets or sets the focus manager used to track console focus state.
    /// </summary>
    [Inject]
    protected internal FocusManager? FocusManager { get; set; }

    /// <summary>
    /// Gets the active live display context, when one is available.
    /// </summary>
    protected ConsoleLiveDisplayContext? LiveDisplayContext => LiveDisplayContextAccessor?.Current;

    /// <summary>
    /// Attempts to retrieve the most recently rendered view.
    /// </summary>
    /// <param name="view">Returns the current console view when available.</param>
    /// <returns><see langword="true"/> when a view is available; otherwise <see langword="false"/>.</returns>
    protected bool TryGetCurrentView([NotNullWhen(true)] out ConsoleViewResult? view)
    {
        view = LiveDisplayContext?.CurrentView;
        return view is not null;
    }

    /// <summary>
    /// Requests a re-render of the root component using the supplied parameters.
    /// Passing <see langword="null"/> reuses the previously supplied parameters.
    /// </summary>
    /// <param name="parameters">Parameters passed to the component.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when an update was applied; otherwise <see langword="false"/>.</returns>
    protected Task<bool> UpdateConsoleAsync(object? parameters, CancellationToken cancellationToken = default)
        => LiveDisplayContextAccessor is null
            ? Task.FromResult(false)
            : LiveDisplayContextAccessor.UpdateModelAsync(parameters, cancellationToken);

    /// <summary>
    /// Requests a re-render of the root component using the previously supplied parameters.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected Task<bool> RefreshConsoleAsync(CancellationToken cancellationToken = default)
        => UpdateConsoleAsync(null, cancellationToken);

    /// <summary>
    /// Attempts to refresh the live display without re-rendering the component.
    /// </summary>
    /// <returns><see langword="true"/> when a live display context was available; otherwise <see langword="false"/>.</returns>
    protected bool TryRefreshLiveDisplay()
    {
        var context = LiveDisplayContext;
        if (context is null)
        {
            return false;
        }

        context.Refresh();
        return true;
    }

    /// <summary>
    /// Attempts to replace the live display content with a custom renderable.
    /// </summary>
    /// <param name="renderable">Renderable to display.</param>
    /// <returns><see langword="true"/> when a live display context was available; otherwise <see langword="false"/>.</returns>
    protected bool TryUpdateRenderable(IRenderable renderable)
    {
        if (renderable is null)
        {
            throw new ArgumentNullException(nameof(renderable));
        }

        var context = LiveDisplayContext;
        if (context is null)
        {
            return false;
        }

        context.UpdateRenderable(renderable);
        return true;
    }

    /// <summary>
    /// Determines whether the supplied focus key matches the active focus target.
    /// </summary>
    /// <param name="focusKey">Focus key to compare.</param>
    /// <returns><see langword="true"/> when the focus key is currently focused; otherwise <see langword="false"/>.</returns>
    protected bool IsFocused(string? focusKey)
        => focusKey is not null && FocusManager?.IsFocused(focusKey) == true;
}
