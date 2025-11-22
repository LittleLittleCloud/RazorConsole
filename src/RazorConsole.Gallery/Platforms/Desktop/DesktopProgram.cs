#if !BROWSER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Services;

namespace RazorConsole.Gallery.Platforms.Desktop;

public static class DesktopProgram
{
    public static async Task<int> Run(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.UseRazorConsole<App>();

        builder.Services.AddHttpClient<INuGetUpgradeChecker, NuGetUpgradeChecker>(client =>
        {
            client.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/", UriKind.Absolute);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RazorConsoleGallery/1.0");
        });

        await builder.Build().RunAsync();
        return 0;
    }
}
#endif
