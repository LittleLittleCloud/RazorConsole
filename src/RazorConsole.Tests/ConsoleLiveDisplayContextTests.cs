using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests;

public class ConsoleLiveDisplayContextTests
{
    [Fact]
    public void UpdateView_DoesNotUpdate_WhenHtmlIsUnchanged()
    {
        var canvas = new TestCanvas();
        var initial = ConsoleViewResult.Create("<p/> ", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initial);

        var updated = context.UpdateView(initial);

        Assert.False(updated);
        Assert.False(canvas.WasUpdated);
    }

    [Fact]
    public void UpdateView_Updates_WhenHtmlChanges()
    {
        var canvas = new TestCanvas();
        var initial = ConsoleViewResult.Create("<p/> ", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initial);
        var view = ConsoleViewResult.Create("<div></div>", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());

        var updated = context.UpdateView(view);

        Assert.True(updated);
        Assert.True(canvas.WasUpdated);
    }

    [Fact]
    public void UpdateRenderable_AllowsUpdatingSameHtmlAfterReset()
    {
        var canvas = new TestCanvas();
        var initial = ConsoleViewResult.Create("<p/> ", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initial);

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
        var initial = ConsoleViewResult.Create("<p/> ", new FakeRenderable(), new[] { new FakeAnimatedRenderable(TimeSpan.FromMilliseconds(10)) });
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initial);

        SpinWait.SpinUntil(() => canvas.RefreshCount > 2, TimeSpan.FromMilliseconds(200));

        Assert.True(canvas.RefreshCount > 0);
    }

    [Fact]
    public async Task UpdateModelAsync_ReRendersUsingCallback()
    {
        var canvas = new TestCanvas();
        var renderCalls = 0;

        var initial = ConsoleViewResult.Create("<p/>", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        var renderer = new FakeRenderer(() => renderCalls++);

        using var context = ConsoleLiveDisplayContext.CreateForTesting<FakeComponent>(
            canvas,
            initial,
            new VdomDiffService(),
            renderer,
            initialParameters: new DummyModel { Value = 0 });

        var updated = await context.UpdateModelAsync(new DummyModel { Value = 42 });

        Assert.True(updated);
        Assert.True(canvas.WasUpdated);
        Assert.Equal(1, renderCalls);
    }

    [Fact]
    public void UpdateView_AppliesReplaceMutation()
    {
        var canvas = new RecordingCanvas();
        var initialVNode = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-rows", "true" },
            },
            new List<VNode>
            {
                new VElementNode(
                    "span",
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        { "data-text", "true" },
                    },
                    new List<VNode> { new VTextNode("Hello") }),
            });

        var updatedVNode = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-rows", "true" },
            },
            new List<VNode>
            {
                new VElementNode(
                    "div",
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        { "data-spacer", "true" },
                        { "data-lines", "1" },
                    },
                    Array.Empty<VNode>()),
            });

        var initial = ConsoleViewResult.Create("initial", initialVNode, new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initial);

        var updated = ConsoleViewResult.Create("updated", updatedVNode, new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());

        var changed = context.UpdateView(updated);

        Assert.True(changed);
        Assert.False(canvas.WasUpdatedViaTarget);
        var replace = Assert.Single(canvas.ReplaceCalls);
        Assert.Equal(new[] { 0 }, replace.Path);
        Assert.NotNull(replace.Renderable);
    }

    [Fact]
    public void UpdateView_AppliesTextMutation()
    {
        var canvas = new RecordingCanvas();
        var initialVNode = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-rows", "true" },
            },
            new List<VNode> { new VTextNode("Hello") });

        var updatedVNode = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-rows", "true" },
            },
            new List<VNode> { new VTextNode("World") });

        var initial = ConsoleViewResult.Create("initial", initialVNode, new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initial);

        var updated = ConsoleViewResult.Create("updated", updatedVNode, new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());

        var changed = context.UpdateView(updated);

        Assert.True(changed);
        Assert.False(canvas.WasUpdatedViaTarget);
        var textUpdate = Assert.Single(canvas.TextCalls);
        Assert.Equal(new[] { 0 }, textUpdate.Path);
        Assert.Equal("World", textUpdate.Text);
    }

    [Fact]
    public void UpdateView_AppliesAttributeMutation()
    {
        var canvas = new RecordingCanvas();
        var initialVNode = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-rows", "true" },
            },
            new List<VNode>
            {
                new VElementNode(
                    "span",
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        { "data-text", "true" },
                        { "data-style", "red" },
                    },
                    new List<VNode> { new VTextNode("Styled") }),
            });

        var updatedVNode = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-rows", "true" },
            },
            new List<VNode>
            {
                new VElementNode(
                    "span",
                    new Dictionary<string, string?>(StringComparer.Ordinal)
                    {
                        { "data-text", "true" },
                        { "data-style", "blue" },
                    },
                    new List<VNode> { new VTextNode("Styled") }),
            });

        var initial = ConsoleViewResult.Create("initial", initialVNode, new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
        using var context = ConsoleLiveDisplayContext.CreateForTesting(canvas, initial);

        var updated = ConsoleViewResult.Create("updated", updatedVNode, new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());

        var changed = context.UpdateView(updated);

        Assert.True(changed);
        Assert.False(canvas.WasUpdatedViaTarget);
        var attributeUpdate = Assert.Single(canvas.AttributeCalls);
        Assert.Equal(new[] { 0 }, attributeUpdate.Path);
        Assert.Equal("blue", attributeUpdate.Attributes["data-style"]);
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

        public bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable) => false;

        public bool TryUpdateText(IReadOnlyList<int> path, string? text) => false;

        public bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes) => false;
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

        public bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable) => false;

        public bool TryUpdateText(IReadOnlyList<int> path, string? text) => false;

        public bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes) => false;
    }

    private sealed class RecordingCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
    {
        public List<(IReadOnlyList<int> Path, IRenderable Renderable)> ReplaceCalls { get; } = new();
        public List<(IReadOnlyList<int> Path, string Text)> TextCalls { get; } = new();
        public List<(IReadOnlyList<int> Path, IReadOnlyDictionary<string, string?> Attributes)> AttributeCalls { get; } = new();

        public bool WasUpdatedViaTarget { get; private set; }

        public void Refresh()
        {
        }

        public void UpdateTarget(IRenderable? renderable)
        {
            WasUpdatedViaTarget = true;
        }

        public bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable)
        {
            ReplaceCalls.Add((path.ToArray(), renderable));
            return true;
        }

        public bool TryUpdateText(IReadOnlyList<int> path, string? text)
        {
            TextCalls.Add((path.ToArray(), text ?? string.Empty));
            return true;
        }

        public bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes)
        {
            AttributeCalls.Add((path.ToArray(), attributes));
            return true;
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

    private sealed class FakeRenderer : IRazorComponentRenderer
    {
        private readonly Action _onRender;

        public FakeRenderer(Action onRender)
        {
            _onRender = onRender;
        }

        public Task<ConsoleViewResult> RenderAsync<TComponent>(object? parameters = null, CancellationToken cancellationToken = default)
            where TComponent : IComponent
        {
            _onRender();
            var value = parameters is DummyModel dummy ? dummy.Value : 0;
            var view = ConsoleViewResult.Create($"<div>{value}</div>", new FakeRenderable(), Array.Empty<IAnimatedConsoleRenderable>());
            return Task.FromResult(view);
        }
    }

    private sealed class FakeComponent : IComponent
    {
        public void Attach(RenderHandle renderHandle)
        {
        }

        public Task SetParametersAsync(ParameterView parameters)
            => Task.CompletedTask;
    }

    private sealed class DummyModel
    {
        public int Value { get; set; }
    }
}
