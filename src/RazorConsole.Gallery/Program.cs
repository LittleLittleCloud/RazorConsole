using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Gallery.Components;
await AppHost.RunAsync<App>(null, builder =>
{
    builder.Configure(options =>
    {
        options.AutoClearConsole = true;
    });

    builder.Services.AddLogging();
});
