using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Input;
using Xunit;

namespace RazorConsole.Tests;

public class MouseEventManagerTests
{
    [Fact]
    public void Constructor_ThrowsOnNullDispatcher()
    {
        var focusManager = new FocusManager(null);

        Assert.Throws<ArgumentNullException>(() =>
            new MouseEventManager(null!, focusManager));
    }

    [Fact]
    public void Constructor_ThrowsOnNullFocusManager()
    {
        var dispatcher = new TestMouseEventDispatcher();

        Assert.Throws<ArgumentNullException>(() =>
            new MouseEventManager(dispatcher, null!));
    }

    [Fact]
    public void IsEnabled_DefaultsToFalse()
    {
        var dispatcher = new TestMouseEventDispatcher();
        var focusManager = new FocusManager(null);
        var manager = new MouseEventManager(dispatcher, focusManager);

        Assert.False(manager.IsEnabled);
    }

    [Fact]
    public void IsEnabled_CanBeSetToTrue()
    {
        var dispatcher = new TestMouseEventDispatcher();
        var focusManager = new FocusManager(null);
        var manager = new MouseEventManager(dispatcher, focusManager);

        manager.IsEnabled = true;

        Assert.True(manager.IsEnabled);
    }

    [Fact]
    public async Task RunAsync_ReturnsImmediatelyWhenNotEnabled()
    {
        var dispatcher = new TestMouseEventDispatcher();
        var focusManager = new FocusManager(null);
        var manager = new MouseEventManager(dispatcher, focusManager);

        using var cts = new CancellationTokenSource(100);

        // Should return immediately without timing out
        await manager.RunAsync(cts.Token);
    }

    [Fact]
    public async Task RunAsync_RespectsImmediateCancellation()
    {
        var dispatcher = new TestMouseEventDispatcher();
        var focusManager = new FocusManager(null);
        var manager = new MouseEventManager(dispatcher, focusManager);

        manager.IsEnabled = true;

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Should complete without throwing despite being enabled
        await manager.RunAsync(cts.Token);
    }

    [Fact]
    public async Task HandleMouseEventAsync_CreatesCorrectEventArgs()
    {
        var dispatcher = new TestMouseEventDispatcher();
        var focusManager = new FocusManager(null);
        var manager = new MouseEventManager(dispatcher, focusManager, NullLogger<MouseEventManager>.Instance);

        // HandleMouseEventAsync currently just logs and returns
        // Once hit testing is implemented, this test would verify dispatch
        await manager.HandleMouseEventAsync(
            "click",
            x: 10,
            y: 20,
            button: 0,
            buttons: 1,
            altKey: false,
            ctrlKey: true,
            shiftKey: false,
            CancellationToken.None);

        // Currently no assertions as the method is a placeholder
        // Future: Assert that dispatcher received the event with correct properties
    }

    private sealed class TestMouseEventDispatcher : IMouseEventDispatcher
    {
        private readonly List<DispatchedEvent> _events = new();

        public IReadOnlyList<DispatchedEvent> Events => _events;

        public Task DispatchAsync(ulong handlerId, EventArgs eventArgs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _events.Add(new DispatchedEvent(handlerId, eventArgs));
            return Task.CompletedTask;
        }
    }

    private readonly record struct DispatchedEvent(ulong HandlerId, EventArgs Args);
}
