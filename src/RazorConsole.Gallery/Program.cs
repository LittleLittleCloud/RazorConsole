using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Gallery.Controllers;
using RazorConsole.Gallery.Services;
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
		services.AddSingleton<GreetingService>();
		services.AddSingleton<HomeController>();
	})
	.Build();

await using var scope = host.Services.CreateAsyncScope();

var controller = scope.ServiceProvider.GetRequiredService<HomeController>();
var intent = await controller.ExecuteAsync().ConfigureAwait(false);

if (intent.Type == NavigationIntentType.Navigate && intent.Target is { Length: > 0 })
{
	AnsiConsole.MarkupLine($"[yellow]Navigation to '{intent.Target}' requested, but no router is configured.[/]");
}
