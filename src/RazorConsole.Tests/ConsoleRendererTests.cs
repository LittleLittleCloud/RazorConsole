using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Vdom;

namespace RazorConsole.Tests;

public sealed class ConsoleRendererTests
{
    [Fact]
    public async Task ElementCapturesComponentChildren_AsRenderableNodes()
    {
        using var renderer = new ConsoleRenderer(new ServiceCollection().BuildServiceProvider(), NullLoggerFactory.Instance);

        var snapshot = await renderer.MountComponentAsync<ContainerComponent>(ParameterView.Empty, CancellationToken.None);

        var root = Assert.IsType<VNode>(snapshot.Root);
        Assert.Equal(VNodeKind.Element, root.Kind);
        Assert.Equal("div", root.TagName);
        Assert.DoesNotContain(Enumerate(root).Skip(1), node => node.Kind == VNodeKind.Component);

        var span = Assert.Single(root.Children);
        Assert.Equal(VNodeKind.Element, span.Kind);
        Assert.Equal("span", span.TagName);
        var text = Assert.Single(span.Children);
        Assert.Equal(VNodeKind.Text, text.Kind);
        Assert.Equal("child", text.Text);
    }

    private static IEnumerable<VNode> Enumerate(VNode node)
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
}
