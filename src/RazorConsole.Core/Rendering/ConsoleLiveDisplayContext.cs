using System;
using System.Collections.Generic;
using System.Threading;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

/// <summary>
/// Provides a simplified facade over Spectre.Console live display updates.
/// </summary>
public sealed class ConsoleLiveDisplayContext : IDisposable
{
    private readonly ILiveDisplayCanvas _canvas;
    private readonly VdomDiffService _diffService;
    private readonly List<AnimationSubscription> _animations = new();
    private readonly VdomSpectreTranslator _translator = new();
    private readonly object _sync = new();
    private readonly object _parameterSync = new();
    private string? _lastHtml;
    private VNode? _lastVdom;
    private readonly Func<object?, CancellationToken, Task<ConsoleViewResult>>? _renderAsync;
    private object? _currentParameters;
    private bool _disposed;

    internal ConsoleLiveDisplayContext(
        ILiveDisplayCanvas canvas,
        ConsoleViewResult? initialView,
        VdomDiffService diffService,
        Func<object?, CancellationToken, Task<ConsoleViewResult>>? renderAsync = null,
        object? initialParameters = null)
    {
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
        if (initialView is not null)
        {
            _lastHtml = initialView.Html;
            _lastVdom = initialView.VdomRoot;
            ApplyAnimations(initialView.AnimatedRenderables);
        }
        _renderAsync = renderAsync;
        _currentParameters = initialParameters;
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

        if (view.VdomRoot is not null && _lastVdom is not null)
        {
            var diff = _diffService.Diff(_lastVdom, view.VdomRoot);
            if (!diff.HasChanges)
            {
                _lastVdom = diff.Current ?? view.VdomRoot;
                _lastHtml = view.Html;
                return false;
            }

            if (TryApplyMutations(diff))
            {
                _lastVdom = diff.Current;
                _lastHtml = view.Html;
                ApplyAnimations(view.AnimatedRenderables);
                return true;
            }
        }
        else if (string.Equals(view.Html, _lastHtml, StringComparison.Ordinal))
        {
            _lastHtml = view.Html;
            return false;
        }

        _canvas.UpdateTarget(view.Renderable);
        _lastHtml = view.Html;
        _lastVdom = view.VdomRoot;
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
        _lastVdom = null;
        ResetAnimations();
    }

    /// <summary>
    /// Forces a refresh of the live display.
    /// </summary>
    public void Refresh() => _canvas.Refresh();

    /// <summary>
    /// Re-renders the underlying component using the provided parameter factory and updates the live display.
    /// </summary>
    /// <param name="parameterFactory">Factory that produces the parameter object passed to the component. Returning <see langword="null"/> reuses the previous parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the view was updated; otherwise <see langword="false"/>.</returns>
    public Task<bool> UpdateModelAsync(Func<object?> parameterFactory, CancellationToken cancellationToken = default)
    {
        if (parameterFactory is null)
        {
            throw new ArgumentNullException(nameof(parameterFactory));
        }

        return UpdateModelInternalAsync(parameterFactory, cancellationToken);
    }

    /// <summary>
    /// Re-renders the underlying component using the provided parameters and updates the live display.
    /// </summary>
    /// <param name="parameters">Parameter object passed to the component. Passing <see langword="null"/> reuses the previous parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the view was updated; otherwise <see langword="false"/>.</returns>
    public Task<bool> UpdateModelAsync(object? parameters, CancellationToken cancellationToken = default)
        => UpdateModelInternalAsync(() => parameters, cancellationToken);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        ResetAnimations();
        _disposed = true;
    }

    internal static ConsoleLiveDisplayContext Create(
        Spectre.Console.LiveDisplayContext context,
        ConsoleViewResult initialView,
        VdomDiffService diffService,
        Func<object?, CancellationToken, Task<ConsoleViewResult>>? renderAsync = null,
        object? initialParameters = null)
        => new(new LiveDisplayCanvasAdapter(context), initialView, diffService, renderAsync, initialParameters);

    internal static ConsoleLiveDisplayContext CreateForTesting(
        ILiveDisplayCanvas canvas,
        ConsoleViewResult? initialView = null,
        VdomDiffService? diffService = null,
        Func<object?, CancellationToken, Task<ConsoleViewResult>>? renderAsync = null,
        object? initialParameters = null)
        => new(canvas, initialView, diffService ?? new VdomDiffService(), renderAsync, initialParameters);

    private async Task<bool> UpdateModelInternalAsync(Func<object?> parameterFactory, CancellationToken cancellationToken)
    {
        if (_renderAsync is null)
        {
            throw new InvalidOperationException("This live display context does not support model updates.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var newParameters = parameterFactory();
        var effectiveParameters = newParameters ?? GetCurrentParameters();

        var view = await _renderAsync(effectiveParameters, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (newParameters is not null)
        {
            SetCurrentParameters(newParameters);
        }

        return UpdateView(view);
    }

    private object? GetCurrentParameters()
    {
        lock (_parameterSync)
        {
            return _currentParameters;
        }
    }

    private void SetCurrentParameters(object? parameters)
    {
        lock (_parameterSync)
        {
            _currentParameters = parameters;
        }
    }

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

    private bool TryApplyMutations(VdomDiffResult diff)
    {
        if (diff.Current is null)
        {
            return false;
        }

        foreach (var mutation in diff.Mutations)
        {
            if (!TryApplyMutation(diff.Current, mutation))
            {
                return false;
            }
        }

        return true;
    }

    private bool TryApplyMutation(VNode root, VdomMutation mutation)
        => mutation.Kind switch
        {
            VdomMutationKind.ReplaceNode => TryApplyReplace(root, mutation),
            VdomMutationKind.UpdateText => TryApplyUpdateText(root, mutation),
            VdomMutationKind.UpdateAttributes => TryApplyUpdateAttributes(root, mutation),
            _ => false,
        };

    private bool TryApplyReplace(VNode root, VdomMutation mutation)
    {
        if (mutation.Path.Count == 0)
        {
            return false;
        }

        var node = FindNode(root, mutation.Path);
        if (node is null)
        {
            return false;
        }

        if (!_translator.TryTranslate(node, out var renderable, out var _animated) || renderable is null)
        {
            return false;
        }

        if (!_canvas.TryReplaceNode(mutation.Path, renderable))
        {
            return false;
        }

        return true;
    }

    private bool TryApplyUpdateText(VNode root, VdomMutation mutation)
    {
        var text = mutation.Text ?? string.Empty;
        if (_canvas.TryUpdateText(mutation.Path, text))
        {
            return true;
        }

        return TryApplyReplace(root, mutation);
    }

    private bool TryApplyUpdateAttributes(VNode root, VdomMutation mutation)
    {
        var attributes = mutation.Attributes ?? new Dictionary<string, string?>(StringComparer.Ordinal);
        if (_canvas.TryUpdateAttributes(mutation.Path, attributes))
        {
            return true;
        }

        return TryApplyReplace(root, mutation);
    }

    private static VNode? FindNode(VNode root, IReadOnlyList<int> path)
    {
        if (path.Count == 0)
        {
            return root;
        }

        VNode current = root;
        foreach (var index in path)
        {
            if (current is not VElementNode element || index < 0 || index >= element.Children.Count)
            {
                return null;
            }

            current = element.Children[index];
        }

        return current;
    }

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
