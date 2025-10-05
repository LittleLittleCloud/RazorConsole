using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Components;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Vdom;

namespace RazorConsole.Tests;

public sealed class SelectTests
{
    [Fact]
    public async Task Select_WithoutValue_ExposesPlaceholderMetadata()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        using var renderer = new ConsoleRenderer(services, NullLoggerFactory.Instance);

        var snapshot = await renderer.MountComponentAsync<PlaceholderHost>(ParameterView.Empty, CancellationToken.None);

        var root = Assert.IsType<VNode>(snapshot.Root);
        Assert.Equal("true", root.Attributes["data-select"]);
        Assert.Equal("false", root.Attributes["data-has-selection"]);
        Assert.Equal("false", root.Attributes["data-select-open"]);
        Assert.Equal("Choose", root.Attributes["data-placeholder"]);

        var display = FindNode(root, static node =>
            node.Attributes.TryGetValue("data-select-toggle", out var toggle) && toggle == "true");

        Assert.NotNull(display);

        var content = FindNode(display!, static node =>
            node.Attributes.TryGetValue("data-text", out var textFlag) && textFlag == "true");

        Assert.NotNull(content);
        Assert.Equal("Choose", content!.Attributes["data-content"]);
    }

    [Fact]
    public async Task Select_WithValue_RendersSelectionMetadata()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        using var renderer = new ConsoleRenderer(services, NullLoggerFactory.Instance);

        var snapshot = await renderer.MountComponentAsync<ValueHost>(ParameterView.Empty, CancellationToken.None);

        var root = Assert.IsType<VNode>(snapshot.Root);
        Assert.Equal("true", root.Attributes["data-has-selection"]);

        var clear = FindNode(root, static node =>
            node.Attributes.TryGetValue("data-select-clear", out var clearFlag) && clearFlag == "true");

        Assert.NotNull(clear);

        var content = FindNode(root, static node =>
            node.Attributes.TryGetValue("data-text", out var textFlag) && textFlag == "true");

        Assert.NotNull(content);
        Assert.Equal("Azure", content!.Attributes["data-content"]);
    }

    private sealed class PlaceholderHost : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Select>(0);
            builder.AddAttribute(1, "Options", new[] { "Azure", "Crimson" });
            builder.AddAttribute(2, "Placeholder", "Choose");
            builder.CloseComponent();
        }
    }

    private sealed class ValueHost : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Select>(0);
            builder.AddAttribute(1, "Options", new[] { "Azure", "Crimson" });
            builder.AddAttribute(2, "Value", "Azure");
            builder.CloseComponent();
        }
    }

    private static VNode? FindNode(VNode node, Func<VNode, bool> predicate)
    {
        if (predicate(node))
        {
            return node;
        }

        foreach (var child in node.Children)
        {
            var match = FindNode(child, predicate);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}
