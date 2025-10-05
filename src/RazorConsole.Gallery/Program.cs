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

// var table = new Table().Centered();

// await AnsiConsole.Live(table)
//     .AutoClear(false)
//     .Overflow(VerticalOverflow.Visible)
//     .StartAsync(async ctx =>
//     {
//         table.AddColumn("Messages");
//         while (true)
//         {
//             table.AddRow("Hello");
//             ctx.Refresh();
//             await Task.Delay(100);
//         }
//     });
