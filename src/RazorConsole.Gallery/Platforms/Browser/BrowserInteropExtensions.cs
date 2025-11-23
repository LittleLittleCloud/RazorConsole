#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core.Input;

namespace RazorConsole.Gallery.Platforms.Browser;

/// <summary>
/// Extensions for configuring browser-based interop in RazorConsole Gallery.
/// </summary>
[SupportedOSPlatform("browser")]
public static class BrowserInteropExtensions
{
    /// <summary>
    /// Configures RazorConsole to use browser-based interop for keyboard and console I/O.
    /// Replaces the default Console.ReadKey() keyboard manager with a browser-compatible version.
    /// </summary>
    public static void UseBrowserInterop(this IHostApplicationBuilder builder)
    {
        // Replace the default KeyboardEventManager (which polls Console.ReadKey())
        // with our BrowserKeyboardEventManager (which receives events from JavaScript)
        // Since BrowserKeyboardEventManager now extends KeyboardEventManager, it can be
        // injected directly wherever KeyboardEventManager is expected
        builder.Services.Replace(ServiceDescriptor.Singleton<KeyboardEventManager, BrowserKeyboardEventManager>());

        // Register browser console output for capturing and forwarding to JavaScript
        builder.Services.AddSingleton<BrowserConsoleOutput>();
    }
}
#endif

