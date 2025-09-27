using System;
using System.Collections.Generic;
using System.Threading;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Provides a simplified facade over Spectre.Console live display updates for console controllers.
/// </summary>
public sealed class ConsoleLiveDisplayContext : IDisposable
{
    private readonly ILiveDisplayCanvas _canvas;
    private readonly List<AnimationSubscription> _animations = new();
    private readonly object _sync = new();
    private string? _lastHtml;
    private bool _disposed;

    internal ConsoleLiveDisplayContext(ILiveDisplayCanvas canvas, string? initialHtml, IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        _lastHtml = initialHtml;
        ApplyAnimations(animatedRenderables);
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
        ApplyAnimations(view.AnimatedRenderables);
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
        ResetAnimations();
    }

    /// <summary>
    /// Forces a refresh of the live display.
    /// </summary>
    public void Refresh() => _canvas.Refresh();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        ResetAnimations();
        _disposed = true;
    }

    internal static ConsoleLiveDisplayContext Create(Spectre.Console.LiveDisplayContext context, ConsoleViewResult initialView)
        => new(new LiveDisplayCanvasAdapter(context), initialView.Html, initialView.AnimatedRenderables);

    internal static ConsoleLiveDisplayContext CreateForTesting(ILiveDisplayCanvas canvas, string? initialHtml, IReadOnlyCollection<IAnimatedConsoleRenderable>? animatedRenderables = null)
        => new(canvas, initialHtml, animatedRenderables ?? Array.Empty<IAnimatedConsoleRenderable>());

    private void ApplyAnimations(IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
    {
        ResetAnimations();

        if (animatedRenderables is null || animatedRenderables.Count == 0)
        {
            return;
        }

        lock (_sync)
        {
            foreach (var animated in animatedRenderables)
            {
                if (animated.RefreshInterval <= TimeSpan.Zero)
                {
                    continue;
                }

                _animations.Add(new AnimationSubscription(animated.RefreshInterval, _canvas));
            }
        }
    }

    private void ResetAnimations()
    {
        lock (_sync)
        {
            foreach (var animation in _animations)
            {
                animation.Dispose();
            }

            _animations.Clear();
        }
    }

    internal interface ILiveDisplayCanvas
    {
        void UpdateTarget(IRenderable? renderable);

        void Refresh();
    }

    private sealed class AnimationSubscription : IDisposable
    {
        private readonly Timer _timer;

        public AnimationSubscription(TimeSpan interval, ILiveDisplayCanvas canvas)
        {
            _timer = new Timer(_ => SafeRefresh(canvas), null, interval, interval);
        }

        private static void SafeRefresh(ILiveDisplayCanvas canvas)
        {
            try
            {
                canvas.Refresh();
            }
            catch
            {
                // Ignore rendering exceptions from background updates.
            }
        }

        public void Dispose()
            => _timer.Dispose();
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
