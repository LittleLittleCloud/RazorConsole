using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Core.Utilities;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Services;
using Spectre.Console;


await AppHost.RunAsync<App>(null, builder =>
{
    builder.Services.AddHttpClient<INuGetUpgradeChecker, NuGetUpgradeChecker>(client =>
    {
        client.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/", UriKind.Absolute);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("RazorConsoleGallery/1.0");
    });

    builder.Services.AddSingleton<ConsoleAppOptions>((services) =>
    {
        return new ConsoleAppOptions
        {
            AutoClearConsole = false
        };
    });
});
