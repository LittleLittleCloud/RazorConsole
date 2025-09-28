using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering.Vdom;

namespace RazorConsole.Core.Rendering.Focus;

/// <summary>
/// Tracks focusable elements within the current virtual DOM and coordinates focus changes.
/// </summary>
public sealed class FocusManager
{
    private readonly object _sync = new();
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private List<FocusTarget> _targets = new();
    private int _currentIndex = -1;
    private ConsoleLiveDisplayContext? _context;
    private Func<CancellationToken, Task>? _refreshCallback;
    private CancellationToken _sessionToken;

    /// <summary>
    /// Raised when the focused element changes.
    /// </summary>
    public event EventHandler<FocusChangedEventArgs>? FocusChanged;

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
            _refreshCallback = async token =>
            {
                await context.UpdateModelAsync((object?)null, token).ConfigureAwait(false);
            };
            _sessionToken = linkedCts.Token;

            context.ViewUpdated += OnViewUpdated;

            initialFocus = UpdateFocusTargets_NoLock(initialView);
        }

        var initializationTask = initialFocus is not null
            ? TriggerFocusChangedAsync(initialFocus, linkedCts.Token)
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
        FocusTarget? nextTarget;

        lock (_sync)
        {
            if (_targets.Count == 0)
            {
                return false;
            }

            var previousKey = CurrentFocusKey;
            var nextIndex = _currentIndex < 0
                ? 0
                : (_currentIndex + 1) % _targets.Count;

            _currentIndex = nextIndex;
            CurrentFocusKey = _targets[nextIndex].Key;

            if (string.Equals(previousKey, CurrentFocusKey, StringComparison.Ordinal))
            {
                return false;
            }

            nextTarget = _targets[nextIndex];
        }

        await TriggerFocusChangedAsync(nextTarget!, token).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Moves focus to the previous focusable target in traversal order.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns><see langword="true"/> when focus changed; otherwise <see langword="false"/>.</returns>
    public async Task<bool> FocusPreviousAsync(CancellationToken token = default)
    {
        FocusTarget? previousTarget;

        lock (_sync)
        {
            if (_targets.Count == 0)
            {
                return false;
            }

            var previousKey = CurrentFocusKey;
            var nextIndex = _currentIndex < 0
                ? _targets.Count - 1
                : (_currentIndex - 1 + _targets.Count) % _targets.Count;

            _currentIndex = nextIndex;
            CurrentFocusKey = _targets[nextIndex].Key;

            if (string.Equals(previousKey, CurrentFocusKey, StringComparison.Ordinal))
            {
                return false;
            }

            previousTarget = _targets[nextIndex];
        }

        await TriggerFocusChangedAsync(previousTarget!, token).ConfigureAwait(false);
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

            _currentIndex = index;
            CurrentFocusKey = _targets[index].Key;
            target = _targets[index];
        }

        await TriggerFocusChangedAsync(target!, token).ConfigureAwait(false);
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

            _context.ViewUpdated -= OnViewUpdated;
            ResetState_NoLock();
        }
    }

    private async Task TriggerFocusChangedAsync(FocusTarget target, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }

        FocusChanged?.Invoke(this, new FocusChangedEventArgs(target.Key));

        var callback = _refreshCallback;
        if (callback is null)
        {
            return;
        }

        await _refreshLock.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await callback(token).ConfigureAwait(false);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private void OnViewUpdated(object? sender, ConsoleViewResult view)
    {
        FocusTarget? newFocus;

        lock (_sync)
        {
            newFocus = UpdateFocusTargets_NoLock(view);
        }

        if (newFocus is not null)
        {
            _ = TriggerFocusChangedAsync(newFocus, _sessionToken);
        }
    }

    private void ResetState_NoLock()
    {
        _context = null;
        _refreshCallback = null;
        _targets = new List<FocusTarget>();
        _currentIndex = -1;
        CurrentFocusKey = null;
        _sessionToken = CancellationToken.None;
    }

    private FocusTarget? UpdateFocusTargets_NoLock(ConsoleViewResult view)
    {
        var targets = view.VdomRoot is null
            ? new List<FocusTarget>()
            : CollectTargets(view.VdomRoot);

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
        if (node is VElementNode element)
        {
            if (IsFocusable(element))
            {
                var key = ResolveKey(element, path);
                var currentSequence = sequence++;
                var order = ResolveOrder(element, currentSequence);
                targets.Add(new FocusTarget(key, order, currentSequence, path.ToArray()));
            }

            for (var i = 0; i < element.Children.Count; i++)
            {
                path.Add(i);
                CollectRecursive(element.Children[i], path, targets, ref sequence);
                path.RemoveAt(path.Count - 1);
            }
        }
        else if (node is VTextNode)
        {
            // No-op for text nodes.
        }
    }

    private static bool IsFocusable(VElementNode element)
    {
        if (!element.Attributes.TryGetValue("data-focusable", out var focusableValue))
        {
            return false;
        }

        return bool.TryParse(focusableValue, out var focusable) && focusable;
    }

    private static string ResolveKey(VElementNode element, IReadOnlyList<int> path)
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

        return string.Join('.', path);
    }

    private static int ResolveOrder(VElementNode element, int sequence)
    {
        if (element.Attributes.TryGetValue("data-focus-order", out var orderValue)
            && int.TryParse(orderValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var order))
        {
            return order;
        }

        return sequence;
    }

    private sealed record FocusTarget(string Key, int Order, int Sequence, IReadOnlyList<int> Path);

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
