using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Extensions;
using RazorConsole.Gallery.Services;

var rootCommand = new RootCommand();
rootCommand.AddDebugCliCommand();

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

rootCommand.SetAction(async (parseResult, ct) =>
{
    await host.RunAsync(ct);
});

var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();
