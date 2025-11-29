// Copyright (c) RazorConsole. All rights reserved.

#pragma warning disable BL0006 // RenderTree types are "internal-ish"; acceptable for console renderer tests.
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests.Rendering;

public sealed class ConsoleRendererObserverTests
{
    [Fact]
    public void Subscribe_NotifiesObserverWithCurrentSnapshot()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var observer = new TestObserver();

        var subscription = renderer.Subscribe(observer);

        observer.Snapshots.Count.ShouldBe(1);
        subscription.ShouldNotBeNull();
    }

    [Fact]
    public void Subscribe_WhenDisposed_CompletesObserver()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var observer = new TestObserver();

        renderer.Dispose();
        var subscription = renderer.Subscribe(observer);

        observer.Snapshots.Count.ShouldBe(1);
        observer.IsCompleted.ShouldBeTrue();
        subscription.ShouldNotBeNull();
    }

    [Fact]
    public async Task Unsubscribe_RemovesObserver()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var observer = new TestObserver();

        var subscription = renderer.Subscribe(observer);
        var initialSnapshotCount = observer.Snapshots.Count;
        subscription.Dispose();

        // Trigger a render cycle to verify observer is detached
        await renderer.MountComponentAsync<SimpleTestComponent>(ParameterView.Empty, CancellationToken.None);

        // Observer should not receive further notifications after unsubscribe
        observer.Snapshots.Count.ShouldBe(initialSnapshotCount);
    }

    [Fact]
    public void Subscribe_WithNullObserver_ThrowsArgumentNullException()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        Should.Throw<ArgumentNullException>(() => renderer.Subscribe(null!));
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

    private sealed class SimpleTestComponent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Test");
            builder.CloseElement();
        }
    }
}

#pragma warning restore BL0006
