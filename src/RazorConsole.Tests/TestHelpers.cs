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
        var translators = serviceProvider.GetServices<VdomSpectreTranslator.IVdomElementTranslator>()
            .OrderBy(t => GetPriority(t))
            .ToList();
        return new VdomSpectreTranslator(translators);
    }

    private static int GetPriority(object translator)
    {
        var priorityProperty = translator.GetType().GetProperty("Priority", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (priorityProperty is not null && priorityProperty.PropertyType == typeof(int))
        {
            return (int)priorityProperty.GetValue(translator)!;
        }
        return int.MaxValue;
    }
}
