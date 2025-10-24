using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Core.Utilities;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Services;
using Spectre.Console;

var builder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<App>();

builder.ConfigureServices(services =>
{
    services.AddHttpClient<INuGetUpgradeChecker, NuGetUpgradeChecker>(client =>
    {
        client.BaseAddress = new Uri("https://api.nuget.org/v3-flatcontainer/", UriKind.Absolute);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("RazorConsoleGallery/1.0");
    });
});

var host = builder.Build();

await host.RunAsync();
