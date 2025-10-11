using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Services;
using Spectre.Console;

await AppHost.RunAsync<App>(null, builder =>
{
    builder.ConfigureServices(services =>
    {
        services.AddHttpClient<INuGetUpgradeChecker, NuGetUpgradeChecker>(client =>
        {
            client.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/", UriKind.Absolute);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RazorConsoleGallery/1.0");
        });
    });

    builder.Configure(options =>
    {
        options.AutoClearConsole = false;
        options.ConsoleLiveDisplayOptions.Overflow = Spectre.Console.VerticalOverflow.Visible;
    });
});
