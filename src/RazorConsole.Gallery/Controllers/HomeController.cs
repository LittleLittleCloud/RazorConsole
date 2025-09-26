using System;
using System.Threading;
using System.Threading.Tasks;
using RazorConsole.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Gallery.Models;
using RazorConsole.Gallery.Services;
using Spectre.Console;

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

            if (!string.Equals(view.Html, lastHtml, StringComparison.Ordinal))
            {
                AnsiConsole.Clear();
                view.WriteTo(AnsiConsole.Console);
                lastHtml = view.Html;
            }

            var input = CaptureName();
            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            _greetingService.UpdateName(input);
        }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[green]Goodbye![/]");
        return NavigationIntent.Exit;
    }

    private static string? CaptureName()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[grey53]Enter a name (leave blank to keep current / exit): [/]");
        return Console.ReadLine();
    }
}
