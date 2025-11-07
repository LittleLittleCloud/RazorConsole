using TodoList.Components;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;

var builder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<TodoList.Components.TodoList>();

var host = builder.Build();

await host.RunAsync();
