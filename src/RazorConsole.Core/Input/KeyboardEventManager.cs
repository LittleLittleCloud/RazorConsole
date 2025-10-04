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
using RazorConsole.Core.Focus;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Vdom;

namespace RazorConsole.Core.Input;

internal interface IKeyboardEventDispatcher
{
    Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken);
}

internal sealed class RendererKeyboardEventDispatcher : IKeyboardEventDispatcher, IFocusEventDispatcher
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
    private readonly object _bufferSync = new();
    private string? _activeFocusKey;

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

    internal async Task HandleKeyAsync(ConsoleKeyInfo keyInfo, CancellationToken token)
    {
        var hasInitialTarget = _focusManager.TryGetFocusedTarget(out var initialTarget);

        if (hasInitialTarget)
        {
            await DispatchKeyboardEventAsync(initialTarget, "onkeydown", keyInfo, token).ConfigureAwait(false);
        }

        switch (keyInfo.Key)
        {
            case ConsoleKey.Tab:
                await HandleTabAsync(keyInfo, token).ConfigureAwait(false);
                break;
            case ConsoleKey.Enter:
                await TriggerActivationAsync(token).ConfigureAwait(false);
                break;
            default:
                if (ShouldRaiseKeyPress(keyInfo))
                {
                    var dispatched = hasInitialTarget
                        ? await DispatchKeyboardEventAsync(initialTarget, "onkeypress", keyInfo, token).ConfigureAwait(false)
                        : false;

                    if (!dispatched)
                    {
                        await DispatchKeyboardEventAsync("onkeypress", keyInfo, token).ConfigureAwait(false);
                    }
                }

                await HandleTextInputAsync(keyInfo, token).ConfigureAwait(false);
                break;
        }

        var keyUpDispatched = await DispatchKeyboardEventAsync("onkeyup", keyInfo, token).ConfigureAwait(false);

        if (!keyUpDispatched && hasInitialTarget)
        {
            await DispatchKeyboardEventAsync(initialTarget, "onkeyup", keyInfo, token).ConfigureAwait(false);
        }
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

    private Task<bool> DispatchKeyboardEventAsync(string eventName, ConsoleKeyInfo keyInfo, CancellationToken token)
    {
        if (!_focusManager.TryGetFocusedTarget(out var target))
        {
            return Task.FromResult(false);
        }

        return DispatchKeyboardEventAsync(target, eventName, keyInfo, token);
    }

    private async Task<bool> DispatchKeyboardEventAsync(FocusManager.FocusTargetSnapshot target, string eventName, ConsoleKeyInfo keyInfo, CancellationToken token)
    {
        if (!TryGetEvent(target, eventName, out var nodeEvent))
        {
            return false;
        }

        var args = CreateKeyboardEventArgs(keyInfo, eventName);
        await DispatchAsync(nodeEvent, args, token).ConfigureAwait(false);
        return true;
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

    private static KeyboardEventArgs CreateKeyboardEventArgs(ConsoleKeyInfo keyInfo, string eventName)
    {
        var type = eventName.StartsWith("on", StringComparison.OrdinalIgnoreCase)
            ? eventName[2..]
            : eventName;

        type = type.ToLowerInvariant();

        return new KeyboardEventArgs
        {
            Type = type,
            Key = ResolveKeyValue(keyInfo),
            Code = ResolveKeyCode(keyInfo),
            Location = 0,
            Repeat = false,
            AltKey = (keyInfo.Modifiers & ConsoleModifiers.Alt) != 0,
            CtrlKey = (keyInfo.Modifiers & ConsoleModifiers.Control) != 0,
            MetaKey = false,
            ShiftKey = (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0,
        };
    }

    private static string ResolveKeyValue(ConsoleKeyInfo keyInfo)
    {
        if (!char.IsControl(keyInfo.KeyChar) && keyInfo.KeyChar != '\0')
        {
            return keyInfo.KeyChar.ToString();
        }

        return keyInfo.Key switch
        {
            ConsoleKey.Enter => "Enter",
            ConsoleKey.Tab => "Tab",
            ConsoleKey.Backspace => "Backspace",
            ConsoleKey.Escape => "Escape",
            _ => keyInfo.Key.ToString(),
        };
    }

    private static string ResolveKeyCode(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key >= ConsoleKey.A && keyInfo.Key <= ConsoleKey.Z)
        {
            return $"Key{keyInfo.Key}";
        }

        if (keyInfo.Key >= ConsoleKey.D0 && keyInfo.Key <= ConsoleKey.D9)
        {
            var digit = (int)keyInfo.Key - (int)ConsoleKey.D0;
            return $"Digit{digit}";
        }

        return keyInfo.Key.ToString();
    }

    private static bool ShouldRaiseKeyPress(ConsoleKeyInfo keyInfo)
    {
        return !char.IsControl(keyInfo.KeyChar) && keyInfo.KeyChar != '\0';
    }

    private void OnFocusChanged(object? sender, FocusChangedEventArgs e)
    {
        if (e is null)
        {
            return;
        }

        lock (_bufferSync)
        {
            if (_activeFocusKey is not null && _buffers.TryGetValue(_activeFocusKey, out var previousBuffer))
            {
                previousBuffer.Clear();
                _buffers.Remove(_activeFocusKey);
            }
        }

        if (!_focusManager.TryGetFocusedTarget(out var target))
        {
            lock (_bufferSync)
            {
                _activeFocusKey = null;
            }

            return;
        }

        lock (_bufferSync)
        {
            _activeFocusKey = target.Key;
            var buffer = GetOrCreateBuffer_NoLock(target);
            buffer.Clear();
            buffer.Append(ResolveInitialValue(target));
        }
    }
}
