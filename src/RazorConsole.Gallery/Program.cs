using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Gallery.Components;
using Spectre.Console;

await AppHost.RunAsync<App>(null, builder =>
{
    builder.Configure(options =>
    {
        options.AutoClearConsole = false;
        options.ConsoleLiveDisplayOptions.Overflow = Spectre.Console.VerticalOverflow.Visible;
    });
});
