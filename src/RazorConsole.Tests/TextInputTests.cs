using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Components;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests;

public sealed class TextInputTests
{
    [Fact]
    public async Task TextInput_RendersFocusMetadataAndValue()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        using var renderer = new ConsoleRenderer(services, NullLoggerFactory.Instance);

        var snapshot = await renderer.MountComponentAsync<ValueHost>(ParameterView.Empty, CancellationToken.None);

        var root = Assert.IsType<RazorConsole.Core.Vdom.VNode>(snapshot.Root);
        Assert.Equal("div", root.TagName);
        Assert.Equal("true", root.Attributes["data-focusable"]);
        Assert.True(root.Attributes.ContainsKey("data-text-input"));
        Assert.Equal("Alice", root.Attributes["value"]);
        Assert.Equal("true", root.Attributes["data-has-value"]);
    }

    [Fact]
    public async Task TextInput_WithoutValue_ExposesPlaceholderMetadata()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        using var renderer = new ConsoleRenderer(services, NullLoggerFactory.Instance);

        var snapshot = await renderer.MountComponentAsync<PlaceholderHost>(ParameterView.Empty, CancellationToken.None);

        var root = Assert.IsType<RazorConsole.Core.Vdom.VNode>(snapshot.Root);
        Assert.Equal("", root.Attributes["value"]);
        Assert.Equal("false", root.Attributes["data-has-value"]);
        Assert.Equal("Type here", root.Attributes["data-placeholder"]);
    }

    [Fact]
    public async Task TextInput_WithExpand_SetsExpandAttribute()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        using var renderer = new ConsoleRenderer(services, NullLoggerFactory.Instance);

        var snapshot = await renderer.MountComponentAsync<ExpandedHost>(ParameterView.Empty, CancellationToken.None);

        var root = Assert.IsType<RazorConsole.Core.Vdom.VNode>(snapshot.Root);
        Assert.Equal("true", root.Attributes["data-expand"]);
    }

    private sealed class ValueHost : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<TextInput>(0);
            builder.AddAttribute(1, "Value", "Alice");
            builder.AddAttribute(2, "Label", "Name");
            builder.AddAttribute(3, "Placeholder", "Type here");
            builder.CloseComponent();
        }
    }

    private sealed class ExpandedHost : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<TextInput>(0);
            builder.AddAttribute(1, "Label", "Name");
            builder.AddAttribute(2, "Expand", true);
            builder.CloseComponent();
        }
    }

    private sealed class PlaceholderHost : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<TextInput>(0);
            builder.AddAttribute(1, "Label", "Name");
            builder.AddAttribute(2, "Placeholder", "Type here");
            builder.CloseComponent();
        }
    }
}