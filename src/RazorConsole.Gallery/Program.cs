using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RazorConsole.Components;
using RazorConsole.Core.Models;
using RazorConsole.Core.Rendering;
using Spectre.Console;

using var host = Host.CreateDefaultBuilder(args)
	.ConfigureServices(services =>
	{
		services.AddLogging();
		services.AddSingleton<IComponentActivator, ServiceProviderComponentActivator>();
		services.AddSingleton<NavigationManager, ConsoleNavigationManager>();
		services.AddSingleton(sp =>
		{
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return new HtmlRenderer(sp, loggerFactory);
		});
		services.AddSingleton<RazorComponentRenderer>();
	})
	.Build();

await using var scope = host.Services.CreateAsyncScope();

var renderer = scope.ServiceProvider.GetRequiredService<RazorComponentRenderer>();

var greeting = new GreetingModel
{
	Date = DateOnly.FromDateTime(DateTime.Now),
};

greeting.Tips.AddRange(new[]
{
	"Experiment with different Spectre widgets.",
	"Update the Razor component with your own data.",
	"Try piping the output through other CLI tools.",
});

string? lastHtml = null;
greeting.Timestamp = DateTime.Now;

while (true)
{
	var html = await renderer.RenderAsync<HelloComponent>(new { Model = greeting });
	Panel panel;
	if (SpectrePanelFactory.TryCreatePanel(html, out var borderPanel) && borderPanel is not null)
	{
		panel = borderPanel;
	}
	else
	{
		var markup = HtmlToSpectreMarkupConverter.Convert(html);
		panel = new Panel(new Markup(markup))
			.Expand()
			.SquareBorder()
			.BorderColor(Color.Grey93);
	}

	if (!string.Equals(html, lastHtml, StringComparison.Ordinal))
	{
		AnsiConsole.Clear();
		AnsiConsole.Write(panel);

		lastHtml = html;
	}

	AnsiConsole.WriteLine();
	Console.Write("Enter a name (leave blank to keep current): ");
	var input = Console.ReadLine();

	if (!string.IsNullOrWhiteSpace(input))
	{
		greeting.Name = input.Trim();
		lastHtml = null;
		greeting.Timestamp = DateTime.Now;
		continue;
	}

	break;
}

AnsiConsole.Clear();
AnsiConsole.MarkupLine("[green]Goodbye![/]");
