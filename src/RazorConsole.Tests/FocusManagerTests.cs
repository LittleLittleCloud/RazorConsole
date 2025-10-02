using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Focus;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Tests;

public sealed class FocusManagerTests
{
    [Fact]
    public async Task BeginSession_SelectsFirstFocusable()
    {
        var manager = new FocusManager();
        var keys = new[] { "first", "second" };
        var initial = CreateView(keys, focusedKey: null);

        using var context = ConsoleLiveDisplayContext.CreateForTesting<FakeComponent>(
            new TestCanvas(),
            initial,
            new VdomDiffService(),
            (parameters, _) => Task.FromResult(CreateView(keys, manager.CurrentFocusKey)));
        using var session = manager.BeginSession(context, initial, CancellationToken.None);
        await session.InitializationTask;

        Assert.True(manager.HasFocusables);
        Assert.Equal("first", manager.CurrentFocusKey);
    }

    [Fact]
    public async Task FocusNextAsync_AdvancesFocus()
    {
        var manager = new FocusManager();
        var keys = new[] { "first", "second", "third" };
        var initial = CreateView(keys, focusedKey: null);
        var renderCount = 0;

        Task<ConsoleViewResult> RenderAsync(object? _, CancellationToken __)
        {
            Interlocked.Increment(ref renderCount);
            return Task.FromResult(CreateView(keys, manager.CurrentFocusKey));
        }

        using var context = ConsoleLiveDisplayContext.CreateForTesting<FakeComponent>(
            new TestCanvas(),
            initial,
            new VdomDiffService(),
            RenderAsync);
        using var session = manager.BeginSession(context, initial, CancellationToken.None);
        await session.InitializationTask;

        renderCount = 0;
        var changed = await manager.FocusNextAsync(session.Token);

        Assert.True(changed);
        Assert.Equal("second", manager.CurrentFocusKey);
        Assert.True(renderCount >= 1);
    }

    [Fact]
    public async Task FocusPreviousAsync_WrapsAround()
    {
        var manager = new FocusManager();
        var keys = new[] { "first", "second", "third" };
        var initial = CreateView(keys, focusedKey: null);

        using var context = ConsoleLiveDisplayContext.CreateForTesting<FakeComponent>(
            new TestCanvas(),
            initial,
            new VdomDiffService(),
            (parameters, _) => Task.FromResult(CreateView(keys, manager.CurrentFocusKey)));
        using var session = manager.BeginSession(context, initial, CancellationToken.None);
        await session.InitializationTask;

        await manager.FocusNextAsync(session.Token);
        await manager.FocusNextAsync(session.Token);
        Assert.Equal("third", manager.CurrentFocusKey);

        var changed = await manager.FocusPreviousAsync(session.Token);

        Assert.True(changed);
        Assert.Equal("second", manager.CurrentFocusKey);
    }

    private static ConsoleViewResult CreateView(IReadOnlyList<string> keys, string? focusedKey)
    {
        var children = new List<VNode>(keys.Count);
        foreach (var key in keys)
        {
            var element = VNode.CreateElement("span");
            element.SetAttribute("data-focusable", "true");
            element.SetAttribute("data-focus-key", key);
            element.SetAttribute("data-text", "true");

            if (string.Equals(key, focusedKey, StringComparison.Ordinal))
            {
                element.SetAttribute("data-style", "yellow");
            }

            element.AddChild(VNode.CreateText(key));
            children.Add(element);
        }

        var root = VNode.CreateElement("div");
        root.SetAttribute("data-rows", "true");
        foreach (var child in children)
        {
            root.AddChild(child);
        }

        var html = focusedKey ?? "none";
        var renderable = new FakeRenderable(html);
        return ConsoleViewResult.Create(html, root, renderable, Array.Empty<IAnimatedConsoleRenderable>());
    }

    private sealed class TestCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
    {
        public void UpdateTarget(IRenderable? renderable)
        {
        }

        public bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable) => false;

        public bool TryUpdateText(IReadOnlyList<int> path, string? text) => false;

        public bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes) => false;

        public void Refresh()
        {
        }
    }

    private sealed class FakeRenderable : IRenderable
    {
        private readonly string _content;

        public FakeRenderable(string content)
        {
            _content = content;
        }

        public Measurement Measure(RenderOptions options, int maxWidth)
            => new(0, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
            => new[] { new Segment(_content) };
    }

    private sealed class FakeComponent : IComponent
    {
        public void Attach(RenderHandle renderHandle)
        {
        }

        public Task SetParametersAsync(ParameterView parameters)
            => Task.CompletedTask;
    }
}
