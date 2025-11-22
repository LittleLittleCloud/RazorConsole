#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;

namespace RazorConsole.Gallery.Platforms.Browser;

/// <summary>
/// Extensions for configuring browser-based interop in RazorConsole Gallery.
/// </summary>
[SupportedOSPlatform("browser")]
public static class BrowserInteropExtensions
{
    /// <summary>
    /// Configures RazorConsole to use browser-based interop for keyboard input.
    /// Registers BrowserKeyboardInterop as a singleton that JavaScript can call into.
    /// </summary>
    public static IHostApplicationBuilder UseBrowserInterop(this IHostApplicationBuilder builder)
    {
        // Register the browser keyboard input provider
        builder.Services.AddSingleton<BrowserKeyboardInputProvider>();
        
        // Register the browser keyboard interop service
        builder.Services.AddSingleton<BrowserKeyboardInterop>();
        
        // Note: We don't register BrowserKeyboardHostedService because the browser version
        // of Program.cs exits early with a success message instead of running the full Gallery.
        // This is a demonstration/infrastructure MVP showing WASM loading works.
        
        return builder;
    }
}

/// <summary>
/// Handles browser keyboard events from JavaScript via JSExport.
/// This class provides JS-callable methods for forwarding keyboard input from xterm.js.
/// </summary>
[SupportedOSPlatform("browser")]
public partial class BrowserKeyboardInterop
{
    private static BrowserKeyboardInterop? _instance;
    private readonly BrowserKeyboardInputProvider _inputProvider;

    public BrowserKeyboardInterop(BrowserKeyboardInputProvider inputProvider)
    {
        _inputProvider = inputProvider ?? throw new ArgumentNullException(nameof(inputProvider));
        _instance = this;
    }

    /// <summary>
    /// Called from JavaScript when a key is pressed in the terminal.
    /// Converts the key information to ConsoleKeyInfo and dispatches it.
    /// </summary>
    [JSExport]
    public static void HandleKeyFromJS(string key, bool shift, bool ctrl, bool alt)
    {
        if (_instance == null)
        {
            return;
        }

        var keyInfo = ConvertToConsoleKeyInfo(key, shift, ctrl, alt);
        
        // Enqueue the key for processing by KeyboardEventManager
        _instance._inputProvider.EnqueueKey(keyInfo);
    }

    private static ConsoleKeyInfo ConvertToConsoleKeyInfo(string key, bool shift, bool ctrl, bool alt)
    {
        var modifiers = ConsoleModifiers.None;
        if (shift) modifiers |= ConsoleModifiers.Shift;
        if (ctrl) modifiers |= ConsoleModifiers.Control;
        if (alt) modifiers |= ConsoleModifiers.Alt;

        // Map common keys from JavaScript event.key to ConsoleKey
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
            _ when key.Length == 1 && char.IsLetter(key[0]) && char.IsUpper(key[0]) 
                && Enum.TryParse<ConsoleKey>(key, out var parsed) => parsed,
            _ => ConsoleKey.NoName
        };

        var keyChar = key.Length == 1 ? key[0] : '\0';
        
        return new ConsoleKeyInfo(keyChar, consoleKey, shift, alt, ctrl);
    }
}
#endif

