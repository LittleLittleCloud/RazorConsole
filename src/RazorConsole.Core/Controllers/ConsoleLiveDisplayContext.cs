using System;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Provides a simplified facade over Spectre.Console live display updates for console controllers.
/// </summary>
public sealed class ConsoleLiveDisplayContext
{
    private readonly ILiveDisplayCanvas _canvas;
    private string? _lastHtml;

    internal ConsoleLiveDisplayContext(ILiveDisplayCanvas canvas, string? initialHtml)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        _lastHtml = initialHtml;
    }

    /// <summary>
    /// Updates the live display with a new view when the HTML differs from the last rendered HTML.
    /// </summary>
    /// <param name="view">The view to display.</param>
    /// <returns><see langword="true"/> when the view was updated; otherwise <see langword="false"/>.</returns>
    public bool UpdateView(ConsoleViewResult view)
    {
        if (view is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        if (string.Equals(view.Html, _lastHtml, StringComparison.Ordinal))
        {
            return false;
        }

        _canvas.UpdateTarget(view.Renderable);
        _lastHtml = view.Html;
        return true;
    }

    /// <summary>
    /// Replaces the live display target with a custom renderable.
    /// </summary>
    /// <param name="renderable">The renderable to display.</param>
    public void UpdateRenderable(IRenderable? renderable)
    {
        _canvas.UpdateTarget(renderable);
        _lastHtml = null;
    }

    /// <summary>
    /// Forces a refresh of the live display.
    /// </summary>
    public void Refresh() => _canvas.Refresh();

    internal static ConsoleLiveDisplayContext Create(Spectre.Console.LiveDisplayContext context, string? initialHtml)
        => new(new LiveDisplayCanvasAdapter(context), initialHtml);

    internal static ConsoleLiveDisplayContext CreateForTesting(ILiveDisplayCanvas canvas, string? initialHtml)
        => new(canvas, initialHtml);

    internal interface ILiveDisplayCanvas
    {
        void UpdateTarget(IRenderable? renderable);

        void Refresh();
    }

    private sealed class LiveDisplayCanvasAdapter : ILiveDisplayCanvas
    {
        private readonly Spectre.Console.LiveDisplayContext _context;

        public LiveDisplayCanvasAdapter(Spectre.Console.LiveDisplayContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void UpdateTarget(IRenderable? renderable)
            => _context.UpdateTarget(renderable);

        public void Refresh()
            => _context.Refresh();
    }
}
