using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests;

public class ConsoleLiveDisplayContextTests
{
    [Fact]
    public void UpdateView_DoesNotUpdate_WhenHtmlIsUnchanged()
    {
    var canvas = new TestCanvas();
    using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, "<p/> ", Array.Empty<IAnimatedConsoleRenderable>());
    var initial = ConsoleViewResult.Create("<p/> ", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());

        var updated = context.UpdateView(initial);

        Assert.False(updated);
        Assert.False(canvas.WasUpdated);
    }

    [Fact]
    public void UpdateView_Updates_WhenHtmlChanges()
    {
    var canvas = new TestCanvas();
    using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, "<p/> ", Array.Empty<IAnimatedConsoleRenderable>());
    var view = ConsoleViewResult.Create("<div></div>", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());

        var updated = context.UpdateView(view);

        Assert.True(updated);
        Assert.True(canvas.WasUpdated);
    }

    [Fact]
    public void UpdateRenderable_AllowsUpdatingSameHtmlAfterReset()
    {
    var canvas = new TestCanvas();
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, "<p/> ", Array.Empty<IAnimatedConsoleRenderable>());

    context.UpdateRenderable(new FakeRenderable());
        var view = ConsoleViewResult.Create("<p/> ", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());

        var updated = context.UpdateView(view);

        Assert.True(updated);
        Assert.True(canvas.WasUpdated);
    }

    [Fact]
    public void AnimatedRenderables_TriggerRefresh()
    {
        var canvas = new RefreshTrackingCanvas();
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, "<p/> ", new[] { new FakeAnimatedRenderable(TimeSpan.FromMilliseconds(10)) });

        SpinWait.SpinUntil(() => canvas.RefreshCount > 2, TimeSpan.FromMilliseconds(200));

        Assert.True(canvas.RefreshCount > 0);
    }

    [Fact]
    public async Task UpdateModelAsync_ReRendersUsingCallback()
    {
        var canvas = new TestCanvas();
        var renderCalls = 0;

        using var context = ConsoleLiveDisplayContext.CreateForTesting(
            canvas,
            "<p/>",
            Array.Empty<IAnimatedConsoleRenderable>(),
            (parameters, _) =>
            {
                renderCalls++;
                var value = parameters is DummyModel dummy ? dummy.Value : 0;
                return Task.FromResult(ConsoleViewResult.Create($"<div>{value}</div>", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>()));
            },
            new DummyModel { Value = 0 });

        var updated = await context.UpdateModelAsync(new DummyModel { Value = 42 });

        Assert.True(updated);
        Assert.True(canvas.WasUpdated);
        Assert.Equal(1, renderCalls);
    }

    private sealed class TestCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
    {
        public bool WasUpdated { get; private set; }

        public void Refresh()
        {
        }

        public void UpdateTarget(IRenderable? renderable)
        {
            WasUpdated = true;
        }
    }

    private sealed class RefreshTrackingCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
    {
        private int _refreshCount;

        public int RefreshCount => _refreshCount;

        public void Refresh()
        {
            Interlocked.Increment(ref _refreshCount);
        }

        public void UpdateTarget(IRenderable? renderable)
        {
        }
    }

    private sealed class FakeRenderable : IRenderable
    {
        public Measurement Measure(RenderOptions options, int maxWidth)
            => new(0, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
            => [];
    }

    private sealed class FakeAnimatedRenderable : IAnimatedConsoleRenderable
    {
        public FakeAnimatedRenderable(TimeSpan interval)
        {
            RefreshInterval = interval;
        }

        public TimeSpan RefreshInterval { get; }
    }

    private sealed class DummyModel
    {
        public int Value { get; set; }
    }
}
