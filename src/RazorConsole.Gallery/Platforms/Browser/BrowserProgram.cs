#if BROWSER
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Services;

namespace RazorConsole.Gallery.Platforms.Browser;

[SupportedOSPlatform("browser")]
public partial class BrowserProgram
{
    public static async Task<int> Run(string[] args)
    {
        // Initialize JavaScript interop
        // The module path is relative to the WASM bundle location
        await JSHost.ImportAsync("main.js", "./main.js");

        var builder = Host.CreateApplicationBuilder(args);

        builder.UseRazorConsole<App>();

        // Use a mock NuGet checker for browser (no network access typically needed)
        builder.Services.AddSingleton<INuGetUpgradeChecker, MockNuGetUpgradeChecker>();

        var host = builder.Build();
        
        // Notify JavaScript that the app is ready
        NotifyAppReady();
        
        await host.RunAsync();
        return 0;
    }

    [JSExport]
    internal static void NotifyAppReady()
    {
        Console.WriteLine("RazorConsole Gallery WASM is ready");
    }
}
#endif
