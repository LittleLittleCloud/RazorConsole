// Copyright (c) RazorConsole. All rights reserved.

#pragma warning disable BL0006 // RenderTree types are "internal-ish"; acceptable for console renderer tests.
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RazorConsole.Tests.Rendering;

public sealed class ConsoleRendererTests
{
    [Fact]
    public async Task ElementCapturesComponentChildren_AsRenderableNodes()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var snapshot = await renderer.MountComponentAsync<ContainerComponent>(ParameterView.Empty, CancellationToken.None);

        var root = snapshot.Root.ShouldBeOfType<Core.Vdom.VNode>();
        root.Kind.ShouldBe(Core.Vdom.VNodeKind.Element);
        root.TagName.ShouldBe("div");
        Enumerate(root).Skip(1).ShouldNotContain(node => node.Kind == Core.Vdom.VNodeKind.Component);

        root.Children.ShouldHaveSingleItem();
        var span = root.Children.Single();
        span.Kind.ShouldBe(Core.Vdom.VNodeKind.Element);
        span.TagName.ShouldBe("span");
        span.Children.ShouldHaveSingleItem();
        var text = span.Children.Single();
        text.Kind.ShouldBe(Core.Vdom.VNodeKind.Text);
        text.Text.ShouldBe("child");
    }

    [Fact]
    public async Task MountComponentAsync_WithValidComponent_ReturnsSnapshot()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var snapshot = await renderer.MountComponentAsync<SimpleComponent>(ParameterView.Empty, CancellationToken.None);

        snapshot.Root.ShouldNotBeNull();
        snapshot.Renderable.ShouldNotBeNull();
    }

    [Fact]
    public async Task MountComponentAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await renderer.MountComponentAsync<SimpleComponent>(ParameterView.Empty, cts.Token));
    }

    [Fact]
    public async Task MountComponentAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var renderer = TestHelpers.CreateTestRenderer();
        renderer.Dispose();

        await Should.ThrowAsync<ObjectDisposedException>(async () =>
            await renderer.MountComponentAsync<SimpleComponent>(ParameterView.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task MountComponentAsync_WithParameters_PassesParametersToComponent()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { "Text", "Test" }
        });

        var snapshot = await renderer.MountComponentAsync<ParameterComponent>(parameters, CancellationToken.None);

        var root = snapshot.Root.ShouldBeOfType<Core.Vdom.VNode>();
        root.Kind.ShouldBe(Core.Vdom.VNodeKind.Element);
        root.TagName.ShouldBe("div");
        root.Children.ShouldHaveSingleItem();

        var text = root.Children.Single();
        text.Kind.ShouldBe(Core.Vdom.VNodeKind.Text);
        text.Text.ShouldBe("Test");
    }

    private static IEnumerable<Core.Vdom.VNode> Enumerate(Core.Vdom.VNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in Enumerate(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed class ContainerComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.OpenComponent<ChildComponent>(1);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }

    private sealed class ChildComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddContent(1, "child");
            builder.CloseElement();
        }
    }

    private sealed class SimpleComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Simple");
            builder.CloseElement();
        }
    }

    private sealed class ParameterComponent : ComponentBase
    {
        [Parameter]
        public string? Text { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, Text ?? "Empty");
            builder.CloseElement();
        }
    }
}
#pragma warning restore BL0006

