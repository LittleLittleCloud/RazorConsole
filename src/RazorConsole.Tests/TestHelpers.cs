using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;

namespace RazorConsole.Tests;

internal static class TestHelpers
{
    public static ConsoleRenderer CreateTestRenderer(IServiceProvider? serviceProvider = null)
    {
        var services = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
        var translator = CreateTestTranslator();
        return new ConsoleRenderer(services, NullLoggerFactory.Instance, translator);
    }

    public static VdomSpectreTranslator CreateTestTranslator()
    {
        var services = new ServiceCollection();
        services.AddDefaultVdomTranslators();
        var serviceProvider = services.BuildServiceProvider();
        var translators = serviceProvider.GetServices<IVdomElementTranslator>()
            .OrderBy(t => t.Priority)
            .ToList();
        return new VdomSpectreTranslator(translators);
    }
}
