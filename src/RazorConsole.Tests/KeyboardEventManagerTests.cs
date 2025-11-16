using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Input;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests;

public class KeyboardEventManagerTests
{
    [Fact]
    public async Task HandleKeyAsync_Enter_DispatchesClickAndChange()
    {
        await using var harness = await KeyboardHarness.CreateAsync(
            new FocusElementSpec(
                key: "first",
                value: "seed",
                events: new Dictionary<string, ulong>
                {
                    ["onclick"] = 1,
                    ["oninput"] = 2,
                    ["onchange"] = 3,
                }));

        var enter = new ConsoleKeyInfo('\r', ConsoleKey.Enter, shift: false, alt: false, control: false);
        await harness.Manager.HandleKeyAsync(enter, CancellationToken.None);

        Assert.Collection(
            harness.Dispatcher.Events,
            dispatched =>
            {
                Assert.Equal(1UL, dispatched.HandlerId);
                Assert.IsType<MouseEventArgs>(dispatched.Args);
            },
            dispatched =>
            {
                Assert.Equal(3UL, dispatched.HandlerId);
                var change = Assert.IsType<ChangeEventArgs>(dispatched.Args);
                Assert.Equal("seed", change.Value);
            });
    }

    [Fact]
    public async Task HandleKeyAsync_InputCharacter_RaisesOnInput()
    {
        await using var harness = await KeyboardHarness.CreateAsync(
            new FocusElementSpec(
                key: "input",
                value: string.Empty,
                events: new Dictionary<string, ulong>
                {
                    ["oninput"] = 5,
                    ["onchange"] = 6,
                }));

        var character = new ConsoleKeyInfo('x', ConsoleKey.X, shift: false, alt: false, control: false);
        await harness.Manager.HandleKeyAsync(character, CancellationToken.None);

        var @event = Assert.Single(harness.Dispatcher.Events);
        Assert.Equal(5UL, @event.HandlerId);
        var changeArgs = Assert.IsType<ChangeEventArgs>(@event.Args);
        Assert.Equal("x", changeArgs.Value);
    }

    [Fact]
    public async Task HandleKeyAsync_Tab_MovesFocusToNext()
    {
        await using var harness = await KeyboardHarness.CreateAsync(
            new FocusElementSpec("first"),
            new FocusElementSpec("second"));

        Assert.Equal("first", harness.FocusManager.CurrentFocusKey);

        var tab = new ConsoleKeyInfo('\t', ConsoleKey.Tab, shift: false, alt: false, control: false);
        await harness.Manager.HandleKeyAsync(tab, CancellationToken.None);

        Assert.Equal("second", harness.FocusManager.CurrentFocusKey);
        Assert.Empty(harness.Dispatcher.Events);
    }

    [Fact]
    public async Task HandleKeyAsync_PrintableKey_DispatchesKeyboardLifecycleEvents()
    {
        await using var harness = await KeyboardHarness.CreateAsync(
            new FocusElementSpec(
                key: "input",
                value: string.Empty,
                events: new Dictionary<string, ulong>
                {
                    ["onkeydown"] = 10,
                    ["onkeypress"] = 11,
                    ["onkeyup"] = 12,
                    ["oninput"] = 13,
                }));

        var character = new ConsoleKeyInfo('a', ConsoleKey.A, shift: false, alt: false, control: false);
        await harness.Manager.HandleKeyAsync(character, CancellationToken.None);

        Assert.Collection(
            harness.Dispatcher.Events,
            dispatched =>
            {
                Assert.Equal(10UL, dispatched.HandlerId);
                var args = Assert.IsType<KeyboardEventArgs>(dispatched.Args);
                Assert.Equal("keydown", args.Type);
                Assert.Equal("a", args.Key);
                Assert.Equal("KeyA", args.Code);
            },
            dispatched =>
            {
                Assert.Equal(11UL, dispatched.HandlerId);
                var args = Assert.IsType<KeyboardEventArgs>(dispatched.Args);
                Assert.Equal("keypress", args.Type);
                Assert.Equal("a", args.Key);
                Assert.Equal("KeyA", args.Code);
            },
            dispatched =>
            {
                Assert.Equal(13UL, dispatched.HandlerId);
                var change = Assert.IsType<ChangeEventArgs>(dispatched.Args);
                Assert.Equal("a", change.Value);
            },
            dispatched =>
            {
                Assert.Equal(12UL, dispatched.HandlerId);
                var args = Assert.IsType<KeyboardEventArgs>(dispatched.Args);
                Assert.Equal("keyup", args.Type);
                Assert.Equal("a", args.Key);
                Assert.Equal("KeyA", args.Code);
            });
    }

    [Fact]
    public async Task HandleKeyAsync_Tab_DispatchesKeyEventsWithFallback()
    {
        await using var harness = await KeyboardHarness.CreateAsync(
            new FocusElementSpec(
                key: "first",
                events: new Dictionary<string, ulong>
                {
                    ["onkeydown"] = 21,
                    ["onkeyup"] = 22,
                }),
            new FocusElementSpec("second"));

        var tab = new ConsoleKeyInfo('\t', ConsoleKey.Tab, shift: false, alt: false, control: false);
        await harness.Manager.HandleKeyAsync(tab, CancellationToken.None);

        Assert.Equal("second", harness.FocusManager.CurrentFocusKey);

        Assert.Collection(
            harness.Dispatcher.Events,
            dispatched =>
            {
                Assert.Equal(21UL, dispatched.HandlerId);
                var args = Assert.IsType<KeyboardEventArgs>(dispatched.Args);
                Assert.Equal("keydown", args.Type);
                Assert.Equal("Tab", args.Key);
            },
            dispatched =>
            {
                Assert.Equal(22UL, dispatched.HandlerId);
                var args = Assert.IsType<KeyboardEventArgs>(dispatched.Args);
                Assert.Equal("keyup", args.Type);
                Assert.Equal("Tab", args.Key);
            });
    }

    [Fact]
    public async Task FocusChange_ClearsBufferedInput()
    {
        await using var harness = await KeyboardHarness.CreateAsync(
            new FocusElementSpec(
                key: "first",
                value: string.Empty,
                events: new Dictionary<string, ulong>
                {
                    ["oninput"] = 101,
                }),
            new FocusElementSpec(
                key: "second",
                value: string.Empty,
                events: new Dictionary<string, ulong>
                {
                    ["oninput"] = 201,
                }));

        var firstChar = new ConsoleKeyInfo('x', ConsoleKey.X, shift: false, alt: false, control: false);
        await harness.Manager.HandleKeyAsync(firstChar, CancellationToken.None);

        harness.Dispatcher.Reset();

        await harness.FocusManager.FocusNextAsync(harness.FocusToken);
        await harness.FocusManager.FocusPreviousAsync(harness.FocusToken);

        var nextChar = new ConsoleKeyInfo('b', ConsoleKey.B, shift: false, alt: false, control: false);
        await harness.Manager.HandleKeyAsync(nextChar, CancellationToken.None);

        var @event = Assert.Single(harness.Dispatcher.Events);
        Assert.Equal(101UL, @event.HandlerId);
        var change = Assert.IsType<ChangeEventArgs>(@event.Args);
        Assert.Equal("b", change.Value);
    }

    private sealed class KeyboardHarness : IAsyncDisposable
    {
        private readonly ConsoleRenderer _renderer;
        private readonly ConsoleLiveDisplayContext _context;
        private readonly FocusManager.FocusSession _session;

        private KeyboardHarness(
            FocusManager focusManager,
            KeyboardEventManager manager,
            TestKeyboardEventDispatcher dispatcher,
            ConsoleRenderer renderer,
            ConsoleLiveDisplayContext context,
            FocusManager.FocusSession session)
        {
            FocusManager = focusManager;
            Manager = manager;
            Dispatcher = dispatcher;
            _renderer = renderer;
            _context = context;
            _session = session;
        }

        public FocusManager FocusManager { get; }

        public KeyboardEventManager Manager { get; }

        public TestKeyboardEventDispatcher Dispatcher { get; }

        public CancellationToken FocusToken => _session.Token;

        public static async Task<KeyboardHarness> CreateAsync(params FocusElementSpec[] elements)
        {
            if (elements is null || elements.Length == 0)
            {
                throw new ArgumentException("At least one focus element is required.", nameof(elements));
            }

            var focusManager = new FocusManager();
            var dispatcher = new TestKeyboardEventDispatcher();
            var manager = new KeyboardEventManager(focusManager, dispatcher, NullLogger<KeyboardEventManager>.Instance);

            var services = new ServiceCollection().BuildServiceProvider();
            var renderer = TestHelpers.CreateTestRenderer(services);
            var canvas = new NoopCanvas();
            var view = BuildView(elements);
            var context = new ConsoleLiveDisplayContext(canvas, renderer, view);

            var session = focusManager.BeginSession(context, view, CancellationToken.None);
            if (view.VdomRoot is not null)
            {
                var snapshot = new ConsoleRenderer.RenderSnapshot(view.VdomRoot, view.Renderable, view.AnimatedRenderables);
                ((IObserver<ConsoleRenderer.RenderSnapshot>)focusManager).OnNext(snapshot);
            }
            await session.InitializationTask.ConfigureAwait(false);

            return new KeyboardHarness(focusManager, manager, dispatcher, renderer, context, session);
        }

        public ValueTask DisposeAsync()
        {
            _session.Dispose();
            _context.Dispose();
            GC.KeepAlive(_renderer);
            return ValueTask.CompletedTask;
        }

        private static ConsoleViewResult BuildView(IEnumerable<FocusElementSpec> elements)
        {
            var root = VNode.CreateElement("div");
            foreach (var element in elements)
            {
                var node = VNode.CreateElement(element.TagName);
                node.SetAttribute("data-focusable", "true");
                node.SetKey(element.Key);
                if (!string.IsNullOrEmpty(element.Value))
                {
                    node.SetAttribute("value", element.Value);
                }

                foreach (var evt in element.Events)
                {
                    node.SetEvent(evt.Key, evt.Value);
                }

                root.AddChild(node);
            }

            return ConsoleViewResult.Create(
                html: "<div></div>",
                vdomRoot: root,
                renderable: new FakeRenderable(),
                animatedRenderables: Array.Empty<IAnimatedConsoleRenderable>());
        }
    }

    private sealed class FocusElementSpec
    {
        public FocusElementSpec(string key, string? value = null, IReadOnlyDictionary<string, ulong>? events = null, string tagName = "div")
        {
            Key = key;
            Value = value;
            Events = events ?? new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
            TagName = tagName;
        }

        public string Key { get; }

        public string? Value { get; }

        public IReadOnlyDictionary<string, ulong> Events { get; }

        public string TagName { get; }
    }

    private sealed class TestKeyboardEventDispatcher : IKeyboardEventDispatcher
    {
        private readonly List<DispatchedEvent> _events = new();

        public IReadOnlyList<DispatchedEvent> Events => _events;

        public void Reset()
            => _events.Clear();

        public Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _events.Add(new DispatchedEvent(handlerId, eventArgs));
            return Task.CompletedTask;
        }
    }

    private readonly record struct DispatchedEvent(ulong HandlerId, EventArgs Args);

    private sealed class NoopCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
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
        public Measurement Measure(RenderOptions options, int maxWidth)
            => new(0, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
            => Array.Empty<Segment>();
    }
}
