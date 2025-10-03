using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Rendering.Focus;
using RazorConsole.Core.Rendering.Vdom;

namespace RazorConsole.Core.Rendering.Input;

internal interface IKeyboardEventDispatcher
{
    Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken);
}

internal sealed class RendererKeyboardEventDispatcher : IKeyboardEventDispatcher
{
    private readonly ConsoleRenderer _renderer;

    public RendererKeyboardEventDispatcher(ConsoleRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (handlerId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(handlerId));
        }

        if (eventArgs is null)
        {
            throw new ArgumentNullException(nameof(eventArgs));
        }

        cancellationToken.ThrowIfCancellationRequested();
        return _renderer.DispatchEventAsync(handlerId, eventArgs);
    }
}

internal sealed class KeyboardEventManager
{
    private readonly FocusManager _focusManager;
    private readonly IKeyboardEventDispatcher _dispatcher;
    private readonly ILogger<KeyboardEventManager> _logger;
    private readonly Dictionary<string, StringBuilder> _buffers = new(StringComparer.Ordinal);
    private readonly HashSet<string> _dirtyKeys = new(StringComparer.Ordinal);
    private readonly object _bufferSync = new();

    public KeyboardEventManager(
        FocusManager focusManager,
        IKeyboardEventDispatcher dispatcher,
        ILogger<KeyboardEventManager>? logger = null)
    {
        _focusManager = focusManager ?? throw new ArgumentNullException(nameof(focusManager));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _logger = logger ?? NullLogger<KeyboardEventManager>.Instance;

        _focusManager.FocusChanged += OnFocusChanged;
    }

    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!Console.KeyAvailable)
                {
                    await Task.Delay(50, token).ConfigureAwait(false);
                    continue;
                }

                var keyInfo = Console.ReadKey(intercept: true);
                await HandleKeyAsync(keyInfo, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (InvalidOperationException)
            {
                break;
            }
            catch (IOException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Transient failure while reading keyboard input.");
                try
                {
                    await Task.Delay(200, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    internal Task HandleKeyAsync(ConsoleKeyInfo keyInfo, CancellationToken token)
    {
        return keyInfo.Key switch
        {
            ConsoleKey.Tab => HandleTabAsync(keyInfo, token),
            ConsoleKey.Enter => TriggerActivationAsync(token),
            _ => HandleTextInputAsync(keyInfo, token),
        };
    }

    private async Task HandleTabAsync(ConsoleKeyInfo keyInfo, CancellationToken token)
    {
        try
        {
            if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
            {
                await _focusManager.FocusPreviousAsync(token).ConfigureAwait(false);
            }
            else
            {
                await _focusManager.FocusNextAsync(token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to update focus target.");
        }
    }

    private async Task HandleTextInputAsync(ConsoleKeyInfo keyInfo, CancellationToken token)
    {
        if (!_focusManager.TryGetFocusedTarget(out var target))
        {
            return;
        }

        if (!TryApplyKeyToBuffer(target, keyInfo, out var updatedValue))
        {
            return;
        }

        if (TryGetEvent(target, "oninput", out var inputEvent))
        {
            var args = new ChangeEventArgs { Value = updatedValue };
            await DispatchAsync(inputEvent, args, token).ConfigureAwait(false);
        }
    }

    private async Task TriggerActivationAsync(CancellationToken token)
    {
        if (!_focusManager.TryGetFocusedTarget(out var target))
        {
            return;
        }

        var value = GetCurrentValue(target);

        if (TryGetEvent(target, "onclick", out var clickEvent))
        {
            var clickArgs = new MouseEventArgs
            {
                Type = "click",
                Detail = 1,
                Button = 0,
            };

            await DispatchAsync(clickEvent, clickArgs, token).ConfigureAwait(false);
        }

        if (TryGetEvent(target, "onchange", out var changeEvent))
        {
            var changeArgs = new ChangeEventArgs { Value = value };
            await DispatchAsync(changeEvent, changeArgs, token).ConfigureAwait(false);
        }
    }

    private async Task DispatchAsync(VNodeEvent @event, EventArgs args, CancellationToken token)
    {
        try
        {
            await _dispatcher.DispatchAsync(@event.HandlerId, args, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch {EventName} handler.", @event.Name);
        }
    }

    private bool TryApplyKeyToBuffer(FocusManager.FocusTargetSnapshot target, ConsoleKeyInfo keyInfo, out string value)
    {
        lock (_bufferSync)
        {
            var buffer = GetOrCreateBuffer_NoLock(target);
            var changed = false;

            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    changed = true;
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                buffer.Append(keyInfo.KeyChar);
                changed = true;
            }

            if (changed)
            {
                _dirtyKeys.Add(target.Key);
            }

            value = buffer.ToString();
            return changed;
        }
    }

    private string GetCurrentValue(FocusManager.FocusTargetSnapshot target)
    {
        lock (_bufferSync)
        {
            return GetOrCreateBuffer_NoLock(target).ToString();
        }
    }

    private StringBuilder GetOrCreateBuffer_NoLock(FocusManager.FocusTargetSnapshot target)
    {
        if (!_buffers.TryGetValue(target.Key, out var buffer))
        {
            buffer = new StringBuilder(ResolveInitialValue(target));
            _buffers[target.Key] = buffer;
        }

        return buffer;
    }

    private static bool TryGetEvent(FocusManager.FocusTargetSnapshot target, string name, out VNodeEvent nodeEvent)
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

    private static string ResolveInitialValue(FocusManager.FocusTargetSnapshot target)
    {
        if (target.Attributes.TryGetValue("value", out var value) && value is not null)
        {
            return value;
        }

        return string.Empty;
    }

    private void OnFocusChanged(object? sender, FocusChangedEventArgs e)
    {
        if (e is null)
        {
            return;
        }

        if (!_focusManager.TryGetFocusedTarget(out var target))
        {
            return;
        }

        lock (_bufferSync)
        {
            if (_dirtyKeys.Contains(target.Key))
            {
                return;
            }

            var buffer = GetOrCreateBuffer_NoLock(target);
            buffer.Clear();
            buffer.Append(ResolveInitialValue(target));
        }
    }
}
