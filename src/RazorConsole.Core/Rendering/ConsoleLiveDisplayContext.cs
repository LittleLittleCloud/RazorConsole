using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace RazorConsole.Core.Rendering;

/// <summary>
/// Provides a simplified facade over Spectre.Console live display updates.
/// </summary>
public sealed class ConsoleLiveDisplayContext : IDisposable, IObserver<ConsoleRenderer.RenderSnapshot>
{
    private readonly ILiveDisplayCanvas _canvas;
    private readonly object _sync = new();
    private readonly ConsoleRenderer? _ownedRenderer;
    private readonly VdomDiffService _diffService;
    private readonly Func<object?, CancellationToken, Task<ConsoleViewResult>>? _renderCallback;
    private bool _disposed;
    private ConsoleViewResult? _currentView;
    private IDisposable? _snapshotSubscription;
    private List<AnimationSubscription>? _animationSubscriptions;
    private object? _lastParameters;

    internal ConsoleLiveDisplayContext(
        ILiveDisplayCanvas canvas,
        ConsoleViewResult? initialView,
        ConsoleRenderer renderer,
        bool ownsRenderer = false,
        VdomDiffService? diffService = null,
        Func<object?, CancellationToken, Task<ConsoleViewResult>>? renderCallback = null,
        object? initialParameters = null)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        if (initialView is not null)
        {
            _currentView = initialView;
        }

        _snapshotSubscription = renderer.Subscribe(this);
        if (ownsRenderer)
        {
            _ownedRenderer = renderer;
        }

        _diffService = diffService ?? new VdomDiffService();
        _renderCallback = renderCallback;
        _lastParameters = initialParameters;

        if (initialView is not null)
        {
            UpdateAnimations(initialView.AnimatedRenderables);
        }
    }

    internal ConsoleViewResult? CurrentView
    {
        get
        {
            lock (_sync)
            {
                return _currentView;
            }
        }
    }

    /// <summary>
    /// Updates the live display with a new view when the HTML differs from the last rendered HTML.
    /// </summary>
    /// <param name="view">The view to display.</param>
    /// <returns><see langword="true"/> when the view was updated; otherwise <see langword="false"/>.</returns>
    public bool UpdateView(ConsoleViewResult view)
    {
        if (view is null || view.Renderable is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        lock (_sync)
        {
            var previous = _currentView;
            var previousRoot = previous?.VdomRoot;
            var currentRoot = view.VdomRoot;

            bool updated;

            if (previousRoot is not null && currentRoot is not null)
            {
                var diff = _diffService.Diff(previousRoot, currentRoot);
                if (!diff.HasChanges)
                {
                    updated = false;
                }
                else
                {
                    var applied = TryApplyMutations(diff);
                    if (!applied)
                    {
                        _canvas.UpdateTarget(view.Renderable);
                    }

                    updated = true;
                }
            }
            else
            {
                _canvas.UpdateTarget(view.Renderable);
                updated = true;
            }

            _currentView = view;
            UpdateAnimations(view.AnimatedRenderables);
            return updated;
        }
    }

    /// <summary>
    /// Replaces the live display target with a custom renderable.
    /// </summary>
    /// <param name="renderable">The renderable to display.</param>
    public void UpdateRenderable(IRenderable? renderable)
    {
        _canvas.UpdateTarget(renderable);
        lock (_sync)
        {
            _currentView = null;
            DisposeAnimations();
        }
    }

    /// <summary>
    /// Forces a refresh of the live display.
    /// </summary>
    public void Refresh() => _canvas.Refresh();

    /// <summary>
    /// Re-renders the underlying component using the provided parameters and updates the live display.
    /// </summary>
    /// <param name="parameters">Parameter object passed to the component. Passing <see langword="null"/> reuses the previous parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the view was updated; otherwise <see langword="false"/>.</returns>
    public async Task<bool> UpdateModelAsync(object? parameters, CancellationToken cancellationToken = default)
    {
        var callback = _renderCallback;
        if (callback is null)
        {
            return false;
        }

        var effectiveParameters = parameters ?? _lastParameters;

        var view = await callback(effectiveParameters, cancellationToken).ConfigureAwait(false);
        if (view is null)
        {
            return false;
        }

        _lastParameters = effectiveParameters;
        return UpdateView(view);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        DisposeAnimations();
        _snapshotSubscription?.Dispose();
#pragma warning disable BL0006
        _ownedRenderer?.Dispose();
#pragma warning restore BL0006

        _disposed = true;
    }

    internal static ConsoleLiveDisplayContext Create<TComponent>(
        Spectre.Console.LiveDisplayContext context,
        ConsoleViewResult initialView,
        ConsoleRenderer renderer)
        where TComponent : IComponent
        => new(
            new LiveDisplayCanvasAdapter(context),
            initialView,
            renderer);

    internal static ConsoleLiveDisplayContext CreateForTesting(
        ILiveDisplayCanvas canvas,
        ConsoleRenderer renderer,
        ConsoleViewResult? initialView = null)
        => new(canvas, initialView, renderer);

    internal static ConsoleLiveDisplayContext CreateForTesting(
        ILiveDisplayCanvas canvas,
        ConsoleViewResult? initialView = null)
    {
        var renderer = CreateRenderer();
        return new ConsoleLiveDisplayContext(canvas, initialView, renderer, ownsRenderer: true);
    }

    internal static ConsoleLiveDisplayContext CreateForTesting<TComponent>(
        ILiveDisplayCanvas canvas,
        ConsoleRenderer renderer,
        ConsoleViewResult? initialView)
        where TComponent : IComponent
        => new(
            canvas,
            initialView,
            renderer);

    internal static ConsoleLiveDisplayContext CreateForTesting(
        ILiveDisplayCanvas canvas,
        ConsoleViewResult? initialView,
        VdomDiffService diffService,
        Func<object?, CancellationToken, Task<ConsoleViewResult>> renderCallback,
        object? initialParameters = null)
    {
        if (diffService is null)
        {
            throw new ArgumentNullException(nameof(diffService));
        }

        if (renderCallback is null)
        {
            throw new ArgumentNullException(nameof(renderCallback));
        }

        var renderer = CreateRenderer();
        return new ConsoleLiveDisplayContext(canvas, initialView, renderer, ownsRenderer: true, diffService: diffService, renderCallback: renderCallback, initialParameters: initialParameters);
    }

    internal static ConsoleLiveDisplayContext CreateForTesting<TComponent>(
        ILiveDisplayCanvas canvas,
        ConsoleViewResult? initialView,
        VdomDiffService diffService,
        Func<object?, CancellationToken, Task<ConsoleViewResult>> renderCallback,
        object? initialParameters = null)
        where TComponent : IComponent
    {
        if (diffService is null)
        {
            throw new ArgumentNullException(nameof(diffService));
        }

        if (renderCallback is null)
        {
            throw new ArgumentNullException(nameof(renderCallback));
        }

        var renderer = CreateRenderer();
        return new ConsoleLiveDisplayContext(canvas, initialView, renderer, ownsRenderer: true, diffService: diffService, renderCallback: renderCallback, initialParameters: initialParameters);
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    void IObserver<ConsoleRenderer.RenderSnapshot>.OnNext(ConsoleRenderer.RenderSnapshot value)
    {
        try
        {
            var view = ConsoleViewResult.FromSnapshot(value, null);
            UpdateView(view);
        }
        catch
        {
            UpdateRenderable(value.Renderable);
        }
    }

    private bool TryApplyMutations(VdomDiffResult diff)
    {
        if (!diff.HasChanges)
        {
            return false;
        }

        foreach (var mutation in diff.Mutations)
        {
            var applied = mutation.Kind switch
            {
                VdomMutationKind.UpdateText => _canvas.TryUpdateText(mutation.Path, mutation.Text),
                VdomMutationKind.UpdateAttributes => _canvas.TryUpdateAttributes(mutation.Path, mutation.Attributes ?? EmptyAttributes),
                VdomMutationKind.ReplaceNode => TryReplaceNode(mutation),
                _ => false,
            };

            if (!applied)
            {
                return false;
            }
        }

        return true;
    }

    private bool TryReplaceNode(VdomMutation mutation)
    {
        if (mutation.Node is null)
        {
            return false;
        }

        if (!SpectreRenderableFactory.TryCreateRenderable(mutation.Node, out var renderable, out _) || renderable is null)
        {
            return false;
        }

        return _canvas.TryReplaceNode(mutation.Path, renderable);
    }

    private void UpdateAnimations(IReadOnlyCollection<IAnimatedConsoleRenderable> animatedRenderables)
    {
        DisposeAnimations();

        if (animatedRenderables is null || animatedRenderables.Count == 0)
        {
            return;
        }

        var subscriptions = new List<AnimationSubscription>(animatedRenderables.Count);
        foreach (var animated in animatedRenderables)
        {
            subscriptions.Add(new AnimationSubscription(animated.RefreshInterval, _canvas));
        }

        _animationSubscriptions = subscriptions;
    }

    private void DisposeAnimations()
    {
        if (_animationSubscriptions is null)
        {
            return;
        }

        foreach (var subscription in _animationSubscriptions)
        {
            subscription.Dispose();
        }

        _animationSubscriptions = null;
    }

    private static ConsoleRenderer CreateRenderer()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        return new ConsoleRenderer(serviceProvider, NullLoggerFactory.Instance);
    }

    private static readonly IReadOnlyDictionary<string, string?> EmptyAttributes =
        new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>(0, StringComparer.Ordinal));

    internal interface ILiveDisplayCanvas
    {
        void UpdateTarget(IRenderable? renderable);

        bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable);

        bool TryUpdateText(IReadOnlyList<int> path, string? text);

        bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes);

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

        public bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable)
            => false;

        public bool TryUpdateText(IReadOnlyList<int> path, string? text)
            => false;

        public bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes)
            => false;

        public void Refresh()
            => _context.Refresh();
    }
}
