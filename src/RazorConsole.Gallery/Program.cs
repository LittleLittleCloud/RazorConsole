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
// Browser WASM demonstration
// Shows that .NET 10 WASM runtime initializes successfully with console output capture
Console.WriteLine("\x1b[1;32m✓ RazorConsole WASM Module Initialized Successfully!\x1b[0m");
Console.WriteLine("");
Console.WriteLine("This demonstrates that:");
Console.WriteLine("  • .NET 10 WASM runtime loads and runs");
Console.WriteLine("  • TypeScript/JavaScript interop is configured");
Console.WriteLine("  • Console output is captured and displayed in xterm.js");
Console.WriteLine("");
Console.WriteLine("\x1b[1;33mNote:\x1b[0m Running the full interactive Gallery requires modifications to");
Console.WriteLine("RazorConsole.Core to expose browser-compatible keyboard handling.");
Console.WriteLine("");
Console.WriteLine("\x1b[1;36mWhat's working:\x1b[0m");
Console.WriteLine("  ✅ WASM build infrastructure complete");
Console.WriteLine("  ✅ Multi-platform project structure (Platforms/Browser, Platforms/Desktop)");
Console.WriteLine("  ✅ Console output capture and forwarding to browser");
Console.WriteLine("  ✅ Keyboard event infrastructure (JSExport methods ready)");
Console.WriteLine("");
Console.WriteLine("\x1b[1;36mTo run full Gallery:\x1b[0m");
Console.WriteLine("  • Make KeyboardEventManager public in RazorConsole.Core");
Console.WriteLine("  • Add browser-compatible IAnsiConsoleOutput implementation");
Console.WriteLine("  • Replace Console.ReadKey() polling with event-driven input");
Console.WriteLine("");
Console.WriteLine("See website/WASM_SETUP.md for complete documentation.");

// Exit gracefully - infrastructure demo complete
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
