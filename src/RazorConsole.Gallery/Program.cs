using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Gallery.Models;

var model = new GreetingModel
{
    Timestamp = DateTime.Now,
};

await AppHost.RunAsync<HelloComponent>(new { Model = model });
