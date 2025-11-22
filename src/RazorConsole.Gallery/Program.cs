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
builder.UseRazorConsole<App>(b => b.UseBrowserInterop());
builder.Services.AddSingleton<INuGetUpgradeChecker, RazorConsole.Gallery.Platforms.Browser.MockNuGetUpgradeChecker>();
#else
builder.UseRazorConsole<App>();
builder.Services.AddHttpClient<INuGetUpgradeChecker, NuGetUpgradeChecker>(client =>
{
    client.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/", UriKind.Absolute);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("RazorConsoleGallery/1.0");
});
#endif

await builder.Build().RunAsync();
