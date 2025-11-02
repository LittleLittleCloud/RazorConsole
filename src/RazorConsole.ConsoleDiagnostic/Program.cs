using Microsoft.Extensions.Hosting;
using RazorConsole.ConsoleDiagnostic;
using RazorConsole.Core;

var builder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<ConsoleInfo>();

var host = builder.Build();

await host.RunAsync();
