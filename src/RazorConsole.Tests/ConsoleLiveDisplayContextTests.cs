using System.Collections.Generic;
using RazorConsole.Core.Controllers;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests;

public class ConsoleLiveDisplayContextTests
{
    [Fact]
    public void UpdateView_DoesNotUpdate_WhenHtmlIsUnchanged()
    {
        var canvas = new TestCanvas();
        var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, "<p/> ");
        var initial = ConsoleViewResult.Create("<p/> ", new FakeRenderable());

        var updated = context.UpdateView(initial);

        Assert.False(updated);
        Assert.False(canvas.WasUpdated);
    }

    [Fact]
    public void UpdateView_Updates_WhenHtmlChanges()
    {
        var canvas = new TestCanvas();
        var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, "<p/> ");
        var view = ConsoleViewResult.Create("<div></div>", new FakeRenderable());

        var updated = context.UpdateView(view);

        Assert.True(updated);
        Assert.True(canvas.WasUpdated);
    }

    [Fact]
    public void UpdateRenderable_AllowsUpdatingSameHtmlAfterReset()
    {
        var canvas = new TestCanvas();
        var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, "<p/> ");

        context.UpdateRenderable(new FakeRenderable());
        var view = ConsoleViewResult.Create("<p/> ", new FakeRenderable());

        var updated = context.UpdateView(view);

        Assert.True(updated);
        Assert.True(canvas.WasUpdated);
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

    private sealed class FakeRenderable : IRenderable
    {
        public Measurement Measure(RenderOptions options, int maxWidth)
            => new(0, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
            => [];
    }
}
