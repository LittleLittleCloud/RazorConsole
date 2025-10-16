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
        services.AddVdomTranslator<CustomTestTranslator>(priority: 5); // High priority

        var serviceProvider = services.BuildServiceProvider();
        var translators = serviceProvider.GetServices<VdomSpectreTranslator.IVdomElementTranslator>();

        // Act
        var translatorList = new List<VdomSpectreTranslator.IVdomElementTranslator>(translators);

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
        services.AddVdomTranslator<CustomTestTranslator>(priority: 1); // Very high priority

        var serviceProvider = services.BuildServiceProvider();
        var translators = serviceProvider.GetServices<VdomSpectreTranslator.IVdomElementTranslator>()
            .OrderBy(t => GetPriority(t))
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
        services.AddVdomTranslator(new CustomTestTranslator(), priority: 1);

        var serviceProvider = services.BuildServiceProvider();
        var translators = serviceProvider.GetServices<VdomSpectreTranslator.IVdomElementTranslator>()
            .OrderBy(t => GetPriority(t))
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

    private static int GetPriority(object translator)
    {
        var priorityProperty = translator.GetType().GetProperty("Priority", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (priorityProperty is not null && priorityProperty.PropertyType == typeof(int))
        {
            return (int)priorityProperty.GetValue(translator)!;
        }
        return int.MaxValue;
    }

    // Test custom translator
    private sealed class CustomTestTranslator : VdomSpectreTranslator.IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, VdomSpectreTranslator.TranslationContext context, out IRenderable? renderable)
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
