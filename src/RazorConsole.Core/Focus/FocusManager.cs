using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Focus;

/// <summary>
/// Tracks focusable elements within the current virtual DOM and coordinates focus changes.
/// </summary>
public sealed class FocusManager : IObserver<ConsoleRenderer.RenderSnapshot>
{
    private readonly IFocusEventDispatcher? _eventDispatcher;
    private readonly object _sync = new();
    private List<FocusTarget> _targets = new();
    private int _currentIndex = -1;
    private ConsoleLiveDisplayContext? _context;

    /// <summary>
    /// Raised when the focused element changes.
    /// </summary>
    public event EventHandler<FocusChangedEventArgs>? FocusChanged;

    public FocusManager()
        : this(null)
    {
    }

    internal FocusManager(IFocusEventDispatcher? eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    /// <summary>
    /// Gets the key of the currently focused element.
    /// </summary>
    public string? CurrentFocusKey { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the manager has active focusable targets.
    /// </summary>
    public bool HasFocusables
    {
        get
        {
            lock (_sync)
            {
                return _targets.Count > 0;
            }
        }
    }

    /// <summary>
    /// Determines whether the supplied focus key corresponds to the active focus target.
    /// </summary>
    /// <param name="key">Focus key to compare.</param>
    /// <returns><see langword="true"/> when the key matches the active focus target; otherwise <see langword="false"/>.</returns>
    public bool IsFocused(string? key)
        => key is not null && string.Equals(CurrentFocusKey, key, StringComparison.Ordinal);

    /// <summary>
    /// Attempts to retrieve metadata associated with the currently focused target.
    /// </summary>
    /// <param name="snapshot">Populated with focus target details when available.</param>
    /// <returns><see langword="true"/> when a focus target is active; otherwise <see langword="false"/>.</returns>
    internal bool TryGetFocusedTarget(out FocusTargetSnapshot snapshot)
    {
        lock (_sync)
        {
            if (_currentIndex < 0 || _currentIndex >= _targets.Count)
            {
                snapshot = default;
                return false;
            }

            snapshot = _targets[_currentIndex].ToSnapshot();
            return true;
        }
    }

    /// <summary>
    /// Begins a new focus session that tracks updates on the provided live display context.
    /// </summary>
    /// <param name="context">Live display context associated with the current console render.</param>
    /// <param name="initialView">The initial view rendered to the console.</param>
    /// <param name="shutdownToken">Token that signals when the session should be cancelled.</param>
    /// <returns>A disposable scope that tears down focus tracking when disposed.</returns>
    public FocusSession BeginSession(ConsoleLiveDisplayContext context, ConsoleViewResult initialView, CancellationToken shutdownToken)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (initialView is null)
        {
            throw new ArgumentNullException(nameof(initialView));
        }

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);

        FocusTarget? initialFocus;

        lock (_sync)
        {
            ResetState_NoLock();

            _context = context;

            if (initialView.VdomRoot is not null)
            {
                var snapshot = new ConsoleRenderer.RenderSnapshot(
                    initialView.VdomRoot,
                    initialView.Renderable,
                    initialView.AnimatedRenderables);

                initialFocus = UpdateFocusTargets_NoLock(snapshot);
            }
            else
            {
                initialFocus = null;
            }
        }

        var initializationTask = initialFocus is not null
            ? TriggerFocusChangedAsync(null, initialFocus!, linkedCts.Token)
            : Task.CompletedTask;

        return new FocusSession(this, context, linkedCts, initializationTask);
    }

    /// <summary>
    /// Moves focus to the next focusable target in traversal order.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns><see langword="true"/> when focus changed; otherwise <see langword="false"/>.</returns>
    public async Task<bool> FocusNextAsync(CancellationToken token = default)
    {
        FocusTarget? previousTarget = null;
        FocusTarget? nextTarget;

        lock (_sync)
        {
            if (_targets.Count == 0)
            {
                return false;
            }

            if (_currentIndex >= 0 && _currentIndex < _targets.Count)
            {
                previousTarget = _targets[_currentIndex];
            }

            var previousIndex = _currentIndex;
            var nextIndex = _currentIndex < 0
                ? 0
                : (_currentIndex + 1) % _targets.Count;

            _currentIndex = nextIndex;
            CurrentFocusKey = _targets[nextIndex].Key;

            if (previousIndex == nextIndex)
            {
                return false;
            }

            nextTarget = _targets[nextIndex];
        }

        await TriggerFocusChangedAsync(previousTarget, nextTarget!, token).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Moves focus to the previous focusable target in traversal order.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns><see langword="true"/> when focus changed; otherwise <see langword="false"/>.</returns>
    public async Task<bool> FocusPreviousAsync(CancellationToken token = default)
    {
        FocusTarget? priorTarget = null;
        FocusTarget? nextTarget;

        lock (_sync)
        {
            if (_targets.Count == 0)
            {
                return false;
            }

            if (_currentIndex >= 0 && _currentIndex < _targets.Count)
            {
                priorTarget = _targets[_currentIndex];
            }

            var previousIndex = _currentIndex;
            var nextIndex = _currentIndex < 0
                ? _targets.Count - 1
                : (_currentIndex - 1 + _targets.Count) % _targets.Count;

            _currentIndex = nextIndex;
            CurrentFocusKey = _targets[nextIndex].Key;

            if (previousIndex == nextIndex)
            {
                return false;
            }

            nextTarget = _targets[nextIndex];
        }

        await TriggerFocusChangedAsync(priorTarget, nextTarget!, token).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Attempts to focus the target that matches the supplied key.
    /// </summary>
    /// <param name="key">Focus key to activate.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns><see langword="true"/> when focus changed; otherwise <see langword="false"/>.</returns>
    public async Task<bool> FocusAsync(string key, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Focus key cannot be null or whitespace.", nameof(key));
        }

        FocusTarget? previousTarget = null;
        FocusTarget? target;

        lock (_sync)
        {
            if (_targets.Count == 0)
            {
                return false;
            }

            var index = _targets.FindIndex(t => string.Equals(t.Key, key, StringComparison.Ordinal));
            if (index < 0)
            {
                return false;
            }

            if (_currentIndex == index)
            {
                return false;
            }

            if (_currentIndex >= 0 && _currentIndex < _targets.Count)
            {
                previousTarget = _targets[_currentIndex];
            }

            _currentIndex = index;
            CurrentFocusKey = _targets[index].Key;
            target = _targets[index];
        }

        await TriggerFocusChangedAsync(previousTarget, target!, token).ConfigureAwait(false);
        return true;
    }

    internal void EndSession(ConsoleLiveDisplayContext context)
    {
        lock (_sync)
        {
            if (!ReferenceEquals(_context, context))
            {
                return;
            }

            ResetState_NoLock();
        }
    }

    private async Task TriggerFocusChangedAsync(FocusTarget? previousTarget, FocusTarget target, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }

        FocusChanged?.Invoke(this, new FocusChangedEventArgs(target.Key));

        await DispatchFocusEventsAsync(previousTarget, target, token).ConfigureAwait(false);
    }

    private async Task DispatchFocusEventsAsync(FocusTarget? previousTarget, FocusTarget target, CancellationToken token)
    {
        var dispatcher = _eventDispatcher;
        if (dispatcher is null)
        {
            return;
        }

        token.ThrowIfCancellationRequested();

        if (previousTarget is not null && TryGetEvent(previousTarget, "onfocusout", out var focusOutEvent))
        {
            await dispatcher.DispatchAsync(focusOutEvent.HandlerId, new FocusEventArgs { Type = "focusout" }, token).ConfigureAwait(false);
        }

        if (TryGetEvent(target, "onfocusin", out var focusInEvent))
        {
            await dispatcher.DispatchAsync(focusInEvent.HandlerId, new FocusEventArgs { Type = "focusin" }, token).ConfigureAwait(false);
        }

        if (TryGetEvent(target, "onfocus", out var focusEvent))
        {
            await dispatcher.DispatchAsync(focusEvent.HandlerId, new FocusEventArgs { Type = "focus" }, token).ConfigureAwait(false);
        }
    }

    private static bool TryGetEvent(FocusTarget target, string name, out VNodeEvent nodeEvent)
    {
        foreach (var candidate in target.Events)
        {
            if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                nodeEvent = candidate;
                return true;
            }
        }

        nodeEvent = default;
        return false;
    }

    private void ResetState_NoLock()
    {
        _context = null;
        _targets = new List<FocusTarget>();
        _currentIndex = -1;
        CurrentFocusKey = null;
    }

    private FocusTarget? UpdateFocusTargets_NoLock(ConsoleRenderer.RenderSnapshot view)
    {
        var targets = view.Root is null
            ? new List<FocusTarget>()
            : CollectTargets(view.Root);

        var previousKey = CurrentFocusKey;

        _targets = targets;

        if (_targets.Count == 0)
        {
            _currentIndex = -1;
            CurrentFocusKey = null;
            return null;
        }

        var matchIndex = -1;
        if (!string.IsNullOrEmpty(previousKey))
        {
            matchIndex = _targets.FindIndex(t => string.Equals(t.Key, previousKey, StringComparison.Ordinal));
        }

        if (matchIndex >= 0)
        {
            _currentIndex = matchIndex;
            CurrentFocusKey = _targets[matchIndex].Key;
            return null;
        }

        _currentIndex = 0;
        CurrentFocusKey = _targets[0].Key;
        return _targets[0];
    }

    private static List<FocusTarget> CollectTargets(VNode root)
    {
        var targets = new List<FocusTarget>();
        var path = new List<int>();
        var sequence = 0;
        CollectRecursive(root, path, targets, ref sequence);
        return targets
            .OrderBy(t => t.Order)
            .ThenBy(t => t.Sequence)
            .ToList();
    }

    private static void CollectRecursive(VNode node, List<int> path, List<FocusTarget> targets, ref int sequence)
    {
        if (node.Kind == VNodeKind.Element && IsFocusable(node))
        {
            var key = ResolveKey(node, path);
            var currentSequence = sequence++;
            var order = ResolveOrder(node, currentSequence);
            var attributes = CreateAttributeSnapshot(node.Attributes);
            var events = node.Events.Count == 0
                ? Array.Empty<VNodeEvent>()
                : node.Events.ToArray();

            targets.Add(new FocusTarget(key, order, currentSequence, path.ToArray(), attributes, events));
        }

        var children = node.Children;
        for (var i = 0; i < children.Count; i++)
        {
            path.Add(i);
            CollectRecursive(children[i], path, targets, ref sequence);
            path.RemoveAt(path.Count - 1);
        }
    }

    private static bool IsFocusable(VNode element)
    {
        if (element.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!element.Attributes.TryGetValue("data-focusable", out var focusableValue) || string.IsNullOrWhiteSpace(focusableValue))
        {
            return element.Events.Count > 0;
        }

        if (bool.TryParse(focusableValue, out var focusable))
        {
            return focusable;
        }

        return element.Events.Count > 0;
    }

    private static string ResolveKey(VNode element, IReadOnlyList<int> path)
    {
        if (element.Kind == VNodeKind.Element)
        {
            if (element.Attributes.TryGetValue("data-focus-key", out var key) && !string.IsNullOrWhiteSpace(key))
            {
                return key;
            }

            if (element.Attributes.TryGetValue("id", out var id) && !string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            if (element.Attributes.TryGetValue("data-key", out var dataKey) && !string.IsNullOrWhiteSpace(dataKey))
            {
                return dataKey;
            }

            if (!string.IsNullOrWhiteSpace(element.Key))
            {
                return element.Key!;
            }
        }

        return string.Join('.', path);
    }

    private static int ResolveOrder(VNode element, int sequence)
    {
        if (element.Kind == VNodeKind.Element
            && element.Attributes.TryGetValue("data-focus-order", out var orderValue)
            && !string.IsNullOrWhiteSpace(orderValue)
            && int.TryParse(orderValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var order))
        {
            return order;
        }

        return sequence;
    }

    private static IReadOnlyDictionary<string, string?> CreateAttributeSnapshot(IReadOnlyDictionary<string, string?> source)
    {
        if (source.Count == 0)
        {
            return EmptyAttributes;
        }

        var copy = new Dictionary<string, string?>(source.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in source)
        {
            copy[pair.Key] = pair.Value;
        }

        return new ReadOnlyDictionary<string, string?>(copy);
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    void IObserver<ConsoleRenderer.RenderSnapshot>.OnNext(ConsoleRenderer.RenderSnapshot value)
    {
        lock (_sync)
        {
            UpdateFocusTargets_NoLock(value);
        }
    }

    private sealed record FocusTarget(
        string Key,
        int Order,
        int Sequence,
        IReadOnlyList<int> Path,
        IReadOnlyDictionary<string, string?> Attributes,
        IReadOnlyList<VNodeEvent> Events)
    {
        public FocusTargetSnapshot ToSnapshot()
            => new(Key, Path, Attributes, Events);
    }

    internal readonly record struct FocusTargetSnapshot(
        string Key,
        IReadOnlyList<int> Path,
        IReadOnlyDictionary<string, string?> Attributes,
        IReadOnlyList<VNodeEvent> Events);

    private static readonly IReadOnlyDictionary<string, string?> EmptyAttributes =
        new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>(0, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Disposable scope representing an active focus tracking session.
    /// </summary>
    public sealed class FocusSession : IDisposable
    {
        private readonly FocusManager _manager;
        private readonly ConsoleLiveDisplayContext _context;
        private readonly CancellationTokenSource _cts;
        private readonly Task _initializationTask;
        private bool _disposed;

        internal FocusSession(FocusManager manager, ConsoleLiveDisplayContext context, CancellationTokenSource cts, Task initializationTask)
        {
            _manager = manager;
            _context = context;
            _cts = cts;
            _initializationTask = initializationTask ?? Task.CompletedTask;
        }

        /// <summary>
        /// Gets a token that is cancelled when the session ends.
        /// </summary>
        public CancellationToken Token => _cts.Token;

        /// <summary>
        /// Gets a task that completes once the initial focus state has been propagated.
        /// </summary>
        public Task InitializationTask => _initializationTask;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            try
            {
                _cts.Cancel();
            }
            catch
            {
                // Ignore cancellation exceptions during teardown.
            }
            finally
            {
                _cts.Dispose();
                _manager.EndSession(_context);
            }
        }
    }
}

/// <summary>
/// Event arguments emitted when focus switches between targets.
/// </summary>
public sealed class FocusChangedEventArgs : EventArgs
{
    internal FocusChangedEventArgs(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the focus key associated with the active target.
    /// </summary>
    public string Key { get; }
}
