using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Models;

await AppHost.RunAsync<HelloComponent>(null, builder =>
{
    builder.Configure(options =>
    {
        options.AutoClearConsole = false;
    });

    builder.Services.AddLogging();
});
