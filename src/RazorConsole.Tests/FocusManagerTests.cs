using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
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

        using var context = ConsoleLiveDisplayContext.CreateForTesting(
            new TestCanvas(),
            initial,
            new VdomDiffService());
        using var session = manager.BeginSession(context, initial, CancellationToken.None);
        await session.InitializationTask;

        PushInitialSnapshot(manager, initial);

        Assert.True(manager.HasFocusables);
        Assert.Equal("first", manager.CurrentFocusKey);
    }

    [Fact]
    public async Task FocusPreviousAsync_WrapsAround()
    {
        var manager = new FocusManager();
        var keys = new[] { "first", "second", "third" };
        var initial = CreateView(keys, focusedKey: null);

        using var context = ConsoleLiveDisplayContext.CreateForTesting(
            new TestCanvas(),
            initial,
            new VdomDiffService());
        using var session = manager.BeginSession(context, initial, CancellationToken.None);
        await session.InitializationTask;

        PushInitialSnapshot(manager, initial);

        await manager.FocusNextAsync(session.Token);
        await manager.FocusNextAsync(session.Token);
        Assert.Equal("third", manager.CurrentFocusKey);

        var changed = await manager.FocusPreviousAsync(session.Token);

        Assert.True(changed);
        Assert.Equal("second", manager.CurrentFocusKey);
    }

    [Fact]
    public async Task BeginSession_SelectsElementWithInteractiveEventsWhenNoFocusableAttribute()
    {
        var manager = new FocusManager();

        var interactive = VNode.CreateElement("button");
        interactive.SetAttribute("data-focus-key", "interactive");
        interactive.SetEvent("onclick", 1UL);
        interactive.AddChild(VNode.CreateText("Click"));

        var root = VNode.CreateElement("div");
        root.AddChild(interactive);

        var view = ConsoleViewResult.Create(
            "interactive",
            root,
            new FakeRenderable("interactive"),
            Array.Empty<IAnimatedConsoleRenderable>());

        using var context = ConsoleLiveDisplayContext.CreateForTesting(
            new TestCanvas(),
            view,
            new VdomDiffService());

        using var session = manager.BeginSession(context, view, CancellationToken.None);
        await session.InitializationTask;

        PushInitialSnapshot(manager, view);

        Assert.True(manager.HasFocusables);
        Assert.Equal("interactive", manager.CurrentFocusKey);
    }

    [Fact]
    public async Task FocusChange_DispatchesFocusEvents()
    {
        var dispatcher = new TestFocusEventDispatcher();
        var manager = new FocusManager(dispatcher);

        var first = VNode.CreateElement("input");
        first.SetAttribute("data-focus-key", "first");
        first.SetEvent("onfocus", 1UL);
        first.SetEvent("onfocusin", 2UL);
        first.SetEvent("onfocusout", 3UL);

        var second = VNode.CreateElement("input");
        second.SetAttribute("data-focus-key", "second");
        second.SetEvent("onfocus", 4UL);
        second.SetEvent("onfocusin", 5UL);
        second.SetEvent("onfocusout", 6UL);

        var root = VNode.CreateElement("div");
        root.AddChild(first);
        root.AddChild(second);

        var view = ConsoleViewResult.Create(
            "focus",
            root,
            new FakeRenderable("focus"),
            Array.Empty<IAnimatedConsoleRenderable>());

        using var context = ConsoleLiveDisplayContext.CreateForTesting(
            new TestCanvas(),
            view,
            new VdomDiffService());

        using var session = manager.BeginSession(context, view, CancellationToken.None);
        await session.InitializationTask;

        PushInitialSnapshot(manager, view);

        Assert.Collection(
            dispatcher.Events,
            e => Assert.Equal(("focusin", 2UL), e),
            e => Assert.Equal(("focus", 1UL), e));

        dispatcher.Events.Clear();

        await manager.FocusNextAsync(session.Token);

        Assert.Collection(
            dispatcher.Events,
            e => Assert.Equal(("focusout", 3UL), e),
            e => Assert.Equal(("focusin", 5UL), e),
            e => Assert.Equal(("focus", 4UL), e));
    }

    [Fact]
    public async Task FocusNextAsync_AdvancesFocus()
    {
        var manager = new FocusManager();
        var keys = new[] { "first", "second", "third" };
        var initial = CreateView(keys, focusedKey: null);

        using var context = ConsoleLiveDisplayContext.CreateForTesting(
            new TestCanvas(),
            initial,
            new VdomDiffService());
        using var session = manager.BeginSession(context, initial, CancellationToken.None);
        await session.InitializationTask;

        PushInitialSnapshot(manager, initial);

        Assert.True(manager.HasFocusables);
        Assert.Equal("first", manager.CurrentFocusKey);

        var changed = await manager.FocusNextAsync(session.Token);

        Assert.True(changed);
        Assert.Equal("second", manager.CurrentFocusKey);
    }

    private static void PushInitialSnapshot(FocusManager manager, ConsoleViewResult view)
    {
        if (view.VdomRoot is null)
        {
            return;
        }

        var snapshot = new ConsoleRenderer.RenderSnapshot(view.VdomRoot, view.Renderable, view.AnimatedRenderables);
        ((IObserver<ConsoleRenderer.RenderSnapshot>)manager).OnNext(snapshot);
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

    private sealed class TestFocusEventDispatcher : IFocusEventDispatcher
    {
        public List<(string EventType, ulong HandlerId)> Events { get; } = new();

        public Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken)
        {
            var type = eventArgs switch
            {
                FocusEventArgs focus => focus.Type ?? string.Empty,
                _ => eventArgs.GetType().Name,
            };

            Events.Add((type, handlerId));
            return Task.CompletedTask;
        }
    }
}
