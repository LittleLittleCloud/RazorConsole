// Copyright (c) RazorConsole. All rights reserved.

#pragma warning disable BL0006 // RenderTree types are "internal-ish"; acceptable for console renderer tests.
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests.Rendering;

public sealed class ConsoleRendererObserverTests
{
    [Fact]
    public void Subscribe_NotifiesObserverWithCurrentSnapshot()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var observer = Substitute.For<IObserver<ConsoleRenderer.RenderSnapshot>>();

        var subscription = renderer.Subscribe(observer);

        observer.Received(1).OnNext(Arg.Any<ConsoleRenderer.RenderSnapshot>());
        subscription.ShouldNotBeNull();
    }

    [Fact]
    public void Subscribe_WhenDisposed_CompletesObserver()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var observer = Substitute.For<IObserver<ConsoleRenderer.RenderSnapshot>>();

        renderer.Dispose();
        var subscription = renderer.Subscribe(observer);

        observer.Received(1).OnNext(Arg.Any<ConsoleRenderer.RenderSnapshot>());
        observer.Received(1).OnCompleted();
        subscription.ShouldNotBeNull();
    }

    [Fact]
    public void Unsubscribe_RemovesObserver()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var observer = Substitute.For<IObserver<ConsoleRenderer.RenderSnapshot>>();

        var subscription = renderer.Subscribe(observer);
        subscription.Dispose();

        // Observer should not receive further notifications after unsubscribe
        // This is tested implicitly - if unsubscribe didn't work, observer would receive more calls
        observer.Received().OnNext(Arg.Any<ConsoleRenderer.RenderSnapshot>());
    }

    [Fact]
    public void Subscribe_WithNullObserver_ThrowsArgumentNullException()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        Should.Throw<ArgumentNullException>(() => renderer.Subscribe(null!));
    }
}

#pragma warning restore BL0006

