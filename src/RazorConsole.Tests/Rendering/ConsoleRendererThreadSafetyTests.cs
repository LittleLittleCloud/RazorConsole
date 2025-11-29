// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests.Rendering;

public sealed class ConsoleRendererThreadSafetyTests
{
    [Fact]
    public async Task Subscribe_FromMultipleThreads_IsThreadSafe()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var observers = new List<TestObserver>();
        var exceptions = new List<Exception>();

        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            try
            {
                var observer = new TestObserver();
                lock (observers)
                {
                    observers.Add(observer);
                }
                renderer.Subscribe(observer);
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        exceptions.ShouldBeEmpty();
        observers.Count.ShouldBe(10);
    }

    [Fact]
    public async Task Unsubscribe_FromMultipleThreads_IsThreadSafe()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var subscriptions = new List<IDisposable>();
        var exceptions = new List<Exception>();

        // Subscribe first
        for (int i = 0; i < 10; i++)
        {
            var observer = new TestObserver();
            subscriptions.Add(renderer.Subscribe(observer));
        }

        // Unsubscribe from multiple threads
        var tasks = subscriptions.Select(sub => Task.Run(() =>
        {
            try
            {
                sub.Dispose();
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        exceptions.ShouldBeEmpty();
    }

    [Fact]
    public async Task MountComponentAsync_SequentialCalls_WorkCorrectly()
    {
        using var renderer = TestHelpers.CreateTestRenderer();

        // MountComponentAsync is not designed for concurrent calls
        // Test sequential calls instead to verify it works correctly
        var snapshot1 = await renderer.MountComponentAsync<SimpleTestComponent>(
            ParameterView.Empty, CancellationToken.None);
        snapshot1.Root.ShouldNotBeNull();

        var snapshot2 = await renderer.MountComponentAsync<SimpleTestComponent>(
            ParameterView.Empty, CancellationToken.None);
        snapshot2.Root.ShouldNotBeNull();
    }

    [Fact]
    public async Task Subscribe_AndUnsubscribe_Concurrently_IsThreadSafe()
    {
        using var renderer = TestHelpers.CreateTestRenderer();
        var subscriptions = new List<IDisposable>();
        var exceptions = new List<Exception>();

        var subscribeTasks = Enumerable.Range(0, 5).Select(i => Task.Run(() =>
        {
            try
            {
                var observer = new TestObserver();
                var sub = renderer.Subscribe(observer);
                lock (subscriptions)
                {
                    subscriptions.Add(sub);
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(subscribeTasks);

        var unsubscribeTasks = subscriptions.Select(sub => Task.Run(() =>
        {
            try
            {
                sub.Dispose();
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(unsubscribeTasks);

        exceptions.ShouldBeEmpty();
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

    private sealed class TestObserver : IObserver<ConsoleRenderer.RenderSnapshot>
    {
        public List<ConsoleRenderer.RenderSnapshot> Snapshots { get; } = new();
        public List<Exception> Errors { get; } = new();
        public bool IsCompleted { get; private set; }

        public void OnNext(ConsoleRenderer.RenderSnapshot value)
        {
            lock (Snapshots)
            {
                Snapshots.Add(value);
            }
        }

        public void OnError(Exception error)
        {
            lock (Errors)
            {
                Errors.Add(error);
            }
        }

        public void OnCompleted()
        {
            IsCompleted = true;
        }
    }
}

