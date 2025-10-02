using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RazorConsole.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Tests;

public sealed class ConsoleComponentBaseTests
{
    [Fact]
    public void TryGetCurrentView_ReturnsFalse_WhenNoContext()
    {
        var component = new TestConsoleComponent();

        var hasView = component.TryGetView(out var view);

        Assert.False(hasView);
        Assert.Null(view);
    }

    [Fact]
    public void TryGetCurrentView_ReturnsView_WhenContextAttached()
    {
        var component = new TestConsoleComponent();
        var accessor = new LiveDisplayContextAccessor();
        component.SetLiveDisplayAccessor(accessor);

        var canvas = new RecordingCanvas();
        var initialView = ConsoleViewResult.Create("<div data-rows=\"true\"></div>", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initialView);
        accessor.Attach(context);

        var hasView = component.TryGetView(out var view);

        Assert.True(hasView);
        Assert.Same(initialView, view);
    }

    [Fact]
    public async Task UpdateConsoleAsync_DelegatesToAccessor()
    {
        var component = new TestConsoleComponent();
        var accessor = new LiveDisplayContextAccessor();
        component.SetLiveDisplayAccessor(accessor);

        var canvas = new RecordingCanvas();
        var initialHtml = "<div data-rows=\"true\"><span data-text=\"true\">initial</span></div>";
        var nextHtml = "<div data-rows=\"true\"><span data-text=\"true\">updated</span></div>";
        var initialView = ConsoleViewResult.Create(initialHtml, new FakeRenderable("initial"), Array.Empty<IAnimatedConsoleRenderable>());
        var nextView = ConsoleViewResult.Create(nextHtml, new FakeRenderable("updated"), Array.Empty<IAnimatedConsoleRenderable>());
        var renderCalls = 0;
        object? observedParameters = null;

        var context = ConsoleLiveDisplayContext.CreateForTesting(
            canvas,
            initialView,
            new VdomDiffService(),
            (parameters, _) =>
            {
                Interlocked.Increment(ref renderCalls);
                observedParameters = parameters;
                return Task.FromResult(nextView);
            });

        accessor.Attach(context);

        var updated = await component.RequestUpdateAsync(new { Value = 42 });

        Assert.True(updated);
        Assert.Equal(1, renderCalls);
        Assert.NotNull(observedParameters);
        Assert.True(canvas.WasUpdated);
    }

    [Fact]
    public async Task RefreshConsoleAsync_ReusesPreviousParameters()
    {
        var component = new TestConsoleComponent();
        var accessor = new LiveDisplayContextAccessor();
        component.SetLiveDisplayAccessor(accessor);

        var canvas = new RecordingCanvas();
        var initialHtml = "<div data-rows=\"true\"><span data-text=\"true\">initial</span></div>";
        var nextHtml = "<div data-rows=\"true\"><span data-text=\"true\">updated</span></div>";
        var initialView = ConsoleViewResult.Create(initialHtml, new FakeRenderable("initial"), Array.Empty<IAnimatedConsoleRenderable>());
        var nextView = ConsoleViewResult.Create(nextHtml, new FakeRenderable("updated"), Array.Empty<IAnimatedConsoleRenderable>());
        var renderCalls = 0;
        object? observedParameters = null;
        var initialParameters = new { Value = 7 };

        var context = ConsoleLiveDisplayContext.CreateForTesting(
            canvas,
            initialView,
            new VdomDiffService(),
            (parameters, _) =>
            {
                Interlocked.Increment(ref renderCalls);
                observedParameters = parameters;
                return Task.FromResult(nextView);
            },
            initialParameters: initialParameters);

        accessor.Attach(context);

        var updated = await component.RefreshAsync();

        Assert.True(updated);
        Assert.Equal(1, renderCalls);
        Assert.Same(initialParameters, observedParameters);
        Assert.True(canvas.WasUpdated);
    }

    private sealed class TestConsoleComponent : ConsoleComponentBase
    {
        public void SetLiveDisplayAccessor(LiveDisplayContextAccessor accessor)
        {
            LiveDisplayContextAccessor = accessor;
        }

        public bool TryGetView(out ConsoleViewResult? view)
            => TryGetCurrentView(out view);

        public Task<bool> RequestUpdateAsync(object? parameters = null, CancellationToken cancellationToken = default)
            => UpdateConsoleAsync(parameters, cancellationToken);

        public Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
            => RefreshConsoleAsync(cancellationToken);
    }

    private sealed class RecordingCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
    {
        public bool WasUpdated { get; private set; }

        public void Refresh()
        {
        }

        public void UpdateTarget(IRenderable? renderable)
        {
            WasUpdated = true;
        }

        public bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable) => false;

        public bool TryUpdateText(IReadOnlyList<int> path, string? text) => false;

        public bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes) => false;
    }

    private sealed class FakeRenderable : IRenderable
    {
        private readonly string _content;

        public FakeRenderable(string content = "content")
        {
            _content = content;
        }

        public Measurement Measure(RenderOptions options, int maxWidth)
            => new(0, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
            => new[] { new Segment(_content) };
    }
}
