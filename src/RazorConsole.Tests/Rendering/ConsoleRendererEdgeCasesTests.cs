// Copyright (c) RazorConsole. All rights reserved.

#pragma warning disable BL0006 // RenderTree types are "internal-ish"; acceptable for console renderer tests.
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests.Rendering;

public sealed class ConsoleRendererEdgeCasesTests
{

    [Fact]
    public async Task MountComponentAsync_WithNestedComponents_HandlesCorrectly()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var snapshot = await renderer.MountComponentAsync<NestedComponent>(ParameterView.Empty, CancellationToken.None);

        snapshot.Root.ShouldNotBeNull();
    }

    [Fact]
    public async Task MountComponentAsync_WithLargeComponentTree_HandlesCorrectly()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        var snapshot = await renderer.MountComponentAsync<LargeComponent>(ParameterView.Empty, CancellationToken.None);

        snapshot.Root.ShouldNotBeNull();
    }

    [Fact]
    public void Subscribe_WithNullObserver_ThrowsArgumentNullException()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        Should.Throw<ArgumentNullException>(() => renderer.Subscribe(null!));
    }

    [Fact]
    public void Subscribe_AfterDispose_ReturnsNoopDisposable()
    {
        var renderer = TestHelpers.CreateTestRenderer();
        var observer = new TestObserver();

        renderer.Dispose();

        var subscription = renderer.Subscribe(observer);
        subscription.ShouldNotBeNull();

        observer.Snapshots.Count.ShouldBeGreaterThanOrEqualTo(0);
        observer.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public void DispatchEventAsync_WithZeroHandlerId_ThrowsArgumentOutOfRangeException()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        Should.Throw<ArgumentOutOfRangeException>(() =>
            renderer.DispatchEventAsync(0, new EventArgs()));
    }

    [Fact]
    public void DispatchEventAsync_WithNullEventArgs_ThrowsArgumentNullException()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        Should.Throw<ArgumentNullException>(() =>
            renderer.DispatchEventAsync(1, null!));
    }

    private sealed class EmptyComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.CloseElement();
        }
    }

    private sealed class NestedComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.OpenComponent<SimpleNestedComponent>(1);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }

    private sealed class SimpleNestedComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddContent(1, "Nested");
            builder.CloseElement();
        }
    }

    private sealed class LargeComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            for (int i = 0; i < 100; i++)
            {
                builder.OpenElement(i * 2 + 1, "div");
                builder.AddContent(i * 2 + 2, $"Item {i}");
                builder.CloseElement();
            }
            builder.CloseElement();
        }
    }

    private sealed class TestObserver : IObserver<ConsoleRenderer.RenderSnapshot>
    {
        public List<ConsoleRenderer.RenderSnapshot> Snapshots { get; } = new();
        public List<Exception> Errors { get; } = new();
        public bool IsCompleted { get; private set; }

        public void OnNext(ConsoleRenderer.RenderSnapshot value)
        {
            Snapshots.Add(value);
        }

        public void OnError(Exception error)
        {
            Errors.Add(error);
        }

        public void OnCompleted()
        {
            IsCompleted = true;
        }
    }
}

#pragma warning restore BL0006

