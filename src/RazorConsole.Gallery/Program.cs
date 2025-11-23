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
// Browser WASM - Run the full Gallery with browser-compatible services
builder.UseRazorConsole<App>(b => b.UseBrowserInterop());

// Use mock NuGet checker for browser (no HTTP client needed)
builder.Services.AddSingleton<INuGetUpgradeChecker, MockNuGetUpgradeChecker>();

Console.WriteLine("\x1b[1;32m🚀 RazorConsole.Gallery starting in browser mode\x1b[0m");

await builder.Build().RunAsync();

#else
// Desktop - Run with standard console I/O
builder.UseRazorConsole<App>();
builder.Services.AddHttpClient<INuGetUpgradeChecker, NuGetUpgradeChecker>(client =>
{
    client.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/", UriKind.Absolute);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("RazorConsoleGallery/1.0");
});

await builder.Build().RunAsync();
#endif
