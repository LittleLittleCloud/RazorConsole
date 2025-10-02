using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using RazorConsole.Gallery.Models;

await AppHost.RunAsync<Counter>(null, builder =>
{
    builder.Configure(options =>
    {
        options.AutoClearConsole = true;
    });

    builder.Services.AddLogging();
});
