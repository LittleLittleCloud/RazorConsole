#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;
using RazorConsole.Core.Input;

namespace RazorConsole.Gallery.Platforms.Browser;

/// <summary>
/// Extensions for configuring browser-based interop in RazorConsole Gallery.
/// </summary>
[SupportedOSPlatform("browser")]
public static class BrowserInteropExtensions
{
    /// <summary>
    /// Configures RazorConsole to use browser-based interop for keyboard input.
    /// Replaces the default Console.ReadKey() based KeyboardEventManager with a browser-compatible version.
    /// </summary>
    public static IHostApplicationBuilder UseBrowserInterop(this IHostApplicationBuilder builder)
    {
        // Replace default KeyboardEventManager with browser-specific version that doesn't use Console.ReadKey()
        builder.Services.RemoveAll<KeyboardEventManager>();
        builder.Services.AddSingleton<BrowserKeyboardEventManager>();

        // Register console output capture for forwarding to xterm.js
        builder.Services.AddSingleton<BrowserConsoleOutput>();
        builder.Services.AddSingleton<BrowserConsoleWriter>();
        
        return builder;
    }
}
#endif

