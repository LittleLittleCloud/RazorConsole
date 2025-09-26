using System;
using System.Threading;
using System.Threading.Tasks;
using RazorConsole.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Gallery.Models;
using RazorConsole.Gallery.Services;

namespace RazorConsole.Gallery.Controllers;

public sealed class HomeController : ConsoleController
{
    private readonly GreetingService _greetingService;

    public HomeController(RazorComponentRenderer renderer, GreetingService greetingService)
        : base(renderer)
    {
        _greetingService = greetingService ?? throw new ArgumentNullException(nameof(greetingService));
    }

    public override async Task<NavigationIntent> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        string? lastHtml = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            var model = _greetingService.GetSnapshot();
            var view = await RenderViewAsync<HelloComponent>(new { Model = model }).ConfigureAwait(false);

            WriteViewIfChanged(view, ref lastHtml);

            WriteLine();
            var inputContext = ReadLineInput("[grey53]Enter a name (leave blank to keep current / exit): [/]");
            var input = inputContext.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            _greetingService.UpdateName(input);
        }

        ClearOutput();
        WriteMarkupLine("[green]Goodbye![/]");
        return NavigationIntent.Exit;
    }
}
