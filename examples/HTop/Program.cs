using HTop.Components;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;

var builder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<HTop.Components.HTop>();

var host = builder.Build();

await host.RunAsync();
