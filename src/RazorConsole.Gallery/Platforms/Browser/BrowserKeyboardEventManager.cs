using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Input;

namespace RazorConsole.Gallery.Platforms.Browser;

/// <summary>
/// Browser-specific keyboard event manager that receives events from JavaScript instead of polling Console.ReadKey().
/// </summary>
[SupportedOSPlatform("browser")]
public sealed class BrowserKeyboardEventManager : KeyboardEventManager
{
    private static BrowserKeyboardEventManager? _instance;
    private static readonly ConcurrentQueue<ConsoleKeyInfo> _keyQueue = new();
    private readonly ILogger<BrowserKeyboardEventManager> _logger;

    public BrowserKeyboardEventManager(
        FocusManager focusManager,
        IKeyboardEventDispatcher dispatcher,
        ILogger<BrowserKeyboardEventManager>? logger = null)
        : base(focusManager, dispatcher, logger)
    {
        _logger = logger ?? NullLogger<BrowserKeyboardEventManager>.Instance;
        _instance = this;
    }

    /// <summary>
    /// Runs the keyboard event loop, processing events from JavaScript instead of Console.ReadKey().
    /// </summary>
    public override async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_keyQueue.TryDequeue(out var keyInfo))
                {
                    await HandleKeyAsync(keyInfo, token).ConfigureAwait(false);
                }
                else
                {
                    // Wait a bit if no keys available
                    await Task.Delay(16, token).ConfigureAwait(false); // ~60fps
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error processing keyboard event from JavaScript.");
                await Task.Delay(100, token).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Called from JavaScript to handle a keyboard event.
    /// </summary>
    [JSExport]
    public static void HandleKeyFromJS(string key, bool shift, bool ctrl, bool alt)
    {
        try
        {
            var keyInfo = ConvertToConsoleKeyInfo(key, shift, ctrl, alt);
            _keyQueue.Enqueue(keyInfo);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error handling key from JS: {ex.Message}");
        }
    }

    private static ConsoleKeyInfo ConvertToConsoleKeyInfo(string key, bool shift, bool ctrl, bool alt)
    {
        var modifiers = ConsoleModifiers.None;
        if (shift) modifiers |= ConsoleModifiers.Shift;
        if (ctrl) modifiers |= ConsoleModifiers.Control;
        if (alt) modifiers |= ConsoleModifiers.Alt;

        // Handle special keys
        var consoleKey = key switch
        {
            "Enter" => ConsoleKey.Enter,
            "Tab" => ConsoleKey.Tab,
            "Backspace" => ConsoleKey.Backspace,
            "Escape" => ConsoleKey.Escape,
            "ArrowUp" => ConsoleKey.UpArrow,
            "ArrowDown" => ConsoleKey.DownArrow,
            "ArrowLeft" => ConsoleKey.LeftArrow,
            "ArrowRight" => ConsoleKey.RightArrow,
            "Home" => ConsoleKey.Home,
            "End" => ConsoleKey.End,
            "PageUp" => ConsoleKey.PageUp,
            "PageDown" => ConsoleKey.PageDown,
            "Delete" => ConsoleKey.Delete,
            "Insert" => ConsoleKey.Insert,
            _ => ParseKey(key),
        };

        var keyChar = key.Length == 1 ? key[0] : '\0';
        return new ConsoleKeyInfo(keyChar, consoleKey, shift, alt, ctrl);
    }

    private static ConsoleKey ParseKey(string key)
    {
        // Try to parse single character keys
        if (key.Length == 1)
        {
            var ch = char.ToUpperInvariant(key[0]);
            if (ch >= 'A' && ch <= 'Z')
            {
                return ConsoleKey.A + (ch - 'A');
            }
            if (ch >= '0' && ch <= '9')
            {
                return ConsoleKey.D0 + (ch - '0');
            }

            // Handle special characters
            return ch switch
            {
                ' ' => ConsoleKey.Spacebar,
                _ => ConsoleKey.None,
            };
        }

        // Try function keys
        if (key.StartsWith("F", StringComparison.Ordinal) && int.TryParse(key.Substring(1), out var fNum))
        {
            if (fNum >= 1 && fNum <= 24)
            {
                return ConsoleKey.F1 + (fNum - 1);
            }
        }

        return ConsoleKey.None;
    }
}
