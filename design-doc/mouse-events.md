# Mouse event specification

## Overview
RazorConsole provides mouse event support for interactive console applications. This document explains how mouse events work, their current limitations, and how to enable and use them in your components.

## Current State

### Keyboard-triggered Click Events
RazorConsole currently supports `onclick` events that are triggered by pressing the **Enter** key when an element has focus. This provides keyboard-based interactivity compatible with Blazor's event model:

```razor
<TextButton Content="Click me" 
            OnClick="HandleClick"
            BackgroundColor="@Color.Grey" 
            FocusedColor="@Color.Blue" />

@code {
    private void HandleClick()
    {
        // This is called when the user presses Enter while the button has focus
    }
}
```

When Enter is pressed on a focused element:
1. An `onclick` event is dispatched with `MouseEventArgs`
2. The event payload includes: `Type = "click"`, `Button = 0`, `Detail = 1`
3. This maintains compatibility with Blazor event handlers

## Actual Mouse Input Support

### Platform Considerations
Native mouse input in console applications requires platform-specific APIs:

#### Windows
- Uses Windows Console API (`ReadConsoleInput`)
- Requires enabling `ENABLE_MOUSE_INPUT` mode via `SetConsoleMode`
- Provides click, double-click, and mouse move events
- Returns screen buffer coordinates

#### Linux/macOS
- Uses ANSI escape sequences (xterm mouse tracking)
- Requires sending escape codes to enable mouse mode: `\x1b[?1000h` (basic), `\x1b[?1003h` (all events)
- Mouse events arrive as escape sequences in stdin: `\x1b[M` followed by button and coordinate data
- Different tracking modes: button events, drag events, all motion events

### Implementation Architecture

To fully support mouse events, RazorConsole would need:

1. **MouseEventManager** - Similar to `KeyboardEventManager`, orchestrates mouse event lifecycle
2. **Platform-specific input readers** - Windows Console API or ANSI escape sequence parsers
3. **Coordinate mapping** - Translate console coordinates to VNode hierarchy
4. **Layout information** - Track where elements are rendered on screen
5. **Event dispatch** - Route events to handlers on the appropriate VNode

### Mouse Event Types

When fully implemented, the following events would be supported:

- `onmousedown` - Mouse button pressed
- `onmouseup` - Mouse button released  
- `onclick` - Complete click (down + up on same element)
- `ondblclick` - Double-click
- `onmousemove` - Mouse pointer moved (may generate many events)
- `onmouseenter` - Mouse enters element bounds
- `onmouseleave` - Mouse leaves element bounds

### Event Payload

Mouse events would include `MouseEventArgs` with:
- `Type` - Event type (click, mousedown, etc.)
- `Button` - Which button: 0 (left), 1 (middle), 2 (right)
- `Buttons` - Bitmask of currently pressed buttons
- `ClientX`, `ClientY` - Coordinates relative to the viewport
- `ScreenX`, `ScreenY` - Coordinates relative to the screen
- `Detail` - Click count (1 for single, 2 for double)
- Modifier flags: `AltKey`, `CtrlKey`, `ShiftKey`, `MetaKey`

## Enabling Mouse Events (Future)

When mouse support is implemented, you would enable it like this:

```csharp
var app = AppHost.Create<MyComponent>(builder =>
{
    builder.Configure(options =>
    {
        options.EnableMouseInput = true;
        options.MouseTrackingMode = MouseTrackingMode.ButtonEvents; // or AllMotion
    });
});

await app.RunAsync();
```

## Current Limitations

1. **No actual mouse input** - Only Enter key triggers onclick
2. **No coordinate tracking** - Console rendering doesn't track element positions
3. **Platform differences** - Would require separate implementations for Windows vs Unix
4. **Terminal compatibility** - Not all terminals support mouse escape sequences
5. **Performance** - Mouse move events can overwhelm the event loop

## Workarounds

Until native mouse support is available, you can:

1. **Use keyboard navigation** - Tab through elements and press Enter
2. **Focus handlers** - Use `@onfocus` and `@onfocusout` for hover-like effects
3. **Keyboard shortcuts** - Implement key handlers for common actions

## Future Work

To implement full mouse support:

1. Add platform detection and input mode initialization
2. Create `MouseEventManager` with async input reading
3. Extend `ConsoleLiveDisplayContext` to track element positions
4. Implement coordinate-to-VNode hit testing
5. Add terminal capability detection
6. Provide fallback behavior when mouse input is unavailable
7. Add comprehensive tests for mouse event dispatch
8. Document browser compatibility and terminal requirements

## Related Documentation

- [keyboard-events.md](./keyboard-events.md) - Keyboard event handling
- [vdom design.md](./vdom design.md) - Virtual DOM and event dispatch architecture
