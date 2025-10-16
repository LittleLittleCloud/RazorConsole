using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests.Vdom;

public sealed class CustomTranslatorTests
{
    [Fact]
    public void CanRegisterCustomTranslatorWithPriority()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDefaultVdomTranslators();
        services.AddVdomTranslator<CustomTestTranslator>();

        var serviceProvider = services.BuildServiceProvider();
        var translators = serviceProvider.GetServices<IVdomElementTranslator>();

        // Act
        var translatorList = new List<IVdomElementTranslator>(translators);

        // Assert - Custom translator should be in the list
        // We should have at least one more translator than the default count (20)
        Assert.True(translatorList.Count > 20);
    }

    [Fact]
    public void CustomTranslatorWithHighPriority_ProcessedFirst()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDefaultVdomTranslators();
        services.AddVdomTranslator<CustomTestTranslator>();

        var serviceProvider = services.BuildServiceProvider();
        var translators = serviceProvider.GetServices<IVdomElementTranslator>()
            .OrderBy(t => t.Priority)
            .ToList();

        var translator = new VdomSpectreTranslator(translators);

        var node = VNode.CreateElement("custom-element");
        node.SetAttribute("data-custom", "true");

        // Act
        var success = translator.TryTranslate(node, out var renderable, out _);

        // Assert
        Assert.True(success);
        Assert.IsType<Text>(renderable);
    }

    [Fact]
    public void CanRegisterCustomTranslatorInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDefaultVdomTranslators();
        services.AddVdomTranslator(new CustomTestTranslator());

        var serviceProvider = services.BuildServiceProvider();
        var translators = serviceProvider.GetServices<IVdomElementTranslator>()
            .OrderBy(t => t.Priority)
            .ToList();

        var translator = new VdomSpectreTranslator(translators);

        var node = VNode.CreateElement("custom-element");
        node.SetAttribute("data-custom", "true");

        // Act
        var success = translator.TryTranslate(node, out var renderable, out _);

        // Assert
        Assert.True(success);
        Assert.IsType<Text>(renderable);
    }

    // Test custom translator
    private sealed class CustomTestTranslator : IVdomElementTranslator
    {
        public int Priority => 1; // High priority

        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!string.Equals(node.TagName, "custom-element", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-custom", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            renderable = new Text("Custom Translation!");
            return true;
        }
    }
}
