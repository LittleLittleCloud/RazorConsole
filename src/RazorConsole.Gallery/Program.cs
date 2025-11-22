using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Services;

#if BROWSER
using RazorConsole.Gallery.Platforms.Browser;
#endif

var builder = Host.CreateApplicationBuilder(args);

#if BROWSER
// For browser, we show a simple success message instead of running the full Gallery
// The full Gallery requires keyboard polling which uses Console.ReadKey() - not available in browser
Console.WriteLine("\x1b[1;32m✓ RazorConsole WASM Module Initialized Successfully!\x1b[0m");
Console.WriteLine("");
Console.WriteLine("This demonstrates that:");
Console.WriteLine("  • .NET 10 WASM runtime loads and runs");
Console.WriteLine("  • TypeScript/JavaScript interop is configured");
Console.WriteLine("  • Console output is captured and displayed");
Console.WriteLine("");
Console.WriteLine("\x1b[1;33mNote:\x1b[0m The full interactive Gallery requires additional work to replace");
Console.WriteLine("Console.ReadKey() polling with browser-compatible keyboard handling.");
Console.WriteLine("");
Console.WriteLine("\x1b[1;36mNext steps for full MVP:\x1b[0m");
Console.WriteLine("  1. Implement IAnsiConsoleOutput replacement for Spectre.Console");
Console.WriteLine("  2. Complete BrowserKeyboardInterop for interactive components");
Console.WriteLine("  3. Forward all console output to xterm.js");
Console.WriteLine("");
Console.WriteLine("Press any key in the terminal to test keyboard events (coming soon)...");

// Exit gracefully - don't try to run the app which would fail on Console.ReadKey()
return;

#else
builder.UseRazorConsole<App>();
builder.Services.AddHttpClient<INuGetUpgradeChecker, NuGetUpgradeChecker>(client =>
{
    client.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/", UriKind.Absolute);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("RazorConsoleGallery/1.0");
});

await builder.Build().RunAsync();
#endif
