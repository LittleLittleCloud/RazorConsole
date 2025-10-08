# How to Enable Mouse Events in RazorConsole

This guide explains how mouse events work in RazorConsole and how to enable them in your applications.

## Quick Start

RazorConsole components support `onclick` events out of the box. Here's a simple example:

```razor
<TextButton Content="Click Me"
            OnClick="HandleClick"
            BackgroundColor="@Color.Grey"
            FocusedColor="@Color.Blue" />

@code {
    private void HandleClick()
    {
        // This is called when the user activates the button
    }
}
```

## Current Implementation: Keyboard Activation

Currently, `onclick` events are triggered by **pressing Enter** when an element has focus:

1. **Navigate** - Press `Tab` to move focus between elements
2. **Activate** - Press `Enter` to trigger the `onclick` event on the focused element

This approach:
- ✅ Works on all platforms
- ✅ Works in all terminal emulators
- ✅ No special configuration needed
- ✅ Fully compatible with Blazor event handlers

## Future: Native Mouse Input

Native mouse support (clicking with an actual mouse pointer) is being developed. When available, you'll be able to enable it like this:

```csharp
var app = AppHost.Create<MyComponent>(builder =>
{
    builder.Configure(options =>
    {
        // Enable native mouse input
        options.EnableMouseInput = true;
    });
});

await app.RunAsync();
```

### Platform Requirements

Native mouse input requires terminal support:

| Platform | Requirement |
|----------|-------------|
| **Windows** | Windows Terminal or ConHost with mouse input enabled |
| **Linux** | Terminal with xterm mouse tracking (most modern terminals) |
| **macOS** | Terminal.app, iTerm2, or other xterm-compatible terminal |

### What Will Be Supported

Once implemented, native mouse input will support:

- **Click events** - `onclick`, `onmousedown`, `onmouseup`
- **Double-click** - `ondblclick`
- **Mouse movement** - `onmousemove`, `onmouseenter`, `onmouseleave`
- **Button tracking** - Left, middle, and right mouse buttons
- **Modifiers** - Alt, Ctrl, Shift keys during mouse events

## Try It Out

The RazorConsole Gallery includes a Mouse Events example. Install and run it:

```bash
dotnet tool install --global RazorConsole.Gallery
razorconsole-gallery
```

Navigate to the "Mouse Events" section to see an interactive demonstration.

## Technical Details

For implementation details, see:
- [design-doc/mouse-events.md](../design-doc/mouse-events.md) - Complete mouse event specification
- [design-doc/keyboard-events.md](../design-doc/keyboard-events.md) - Keyboard event specification
- [src/RazorConsole.Core/Input/MouseEventManager.cs](../src/RazorConsole.Core/Input/MouseEventManager.cs) - Mouse event manager implementation

## FAQ

### Why doesn't clicking with my mouse work?

Native mouse input is not yet implemented. Currently, you must use `Tab` to focus and `Enter` to click.

### Can I detect mouse hover?

Not yet. Use `@onfocus` and `@onfocusout` events as an alternative for hover-like effects.

### Will native mouse support work in my terminal?

Most modern terminals support mouse events via xterm protocols. The implementation will include terminal capability detection and graceful fallback.

### Can I use both keyboard and mouse?

Yes! When native mouse support is added, both keyboard (Tab + Enter) and mouse clicks will work simultaneously.

## Contributing

Want to help implement native mouse support? Check out the [design document](../design-doc/mouse-events.md) and open an issue to discuss your approach.
