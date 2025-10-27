### Focus Management

Console UIs still need strong focus cues. RazorConsole automatically manages focus order, making keyboard navigation predictable.

#### Setting focus order

```razor
<TextInput Value="@name"
           ValueChanged="@((v) => name = v)"
           FocusOrder="1" />

<TextInput Value="@email"
           ValueChanged="@((v) => email = v)"
           FocusOrder="2" />

<TextButton Content="Submit"
            OnClick="HandleSubmit"
            FocusOrder="3" />
```

#### Responding to focus changes

```razor
<div @onfocus="OnFocus" @onfocusout="OnFocusOut">
    <Markup Content="@message" />
</div>

@code {
    private string message = "Not focused";

    private void OnFocus(FocusEventArgs e)
    {
        message = "Focused!";
        StateHasChanged();
    }

    private void OnFocusOut(FocusEventArgs e)
    {
        message = "Focus lost";
        StateHasChanged();
    }
}
```

Users can move through focusable elements with **Tab** (forward) and **Shift + Tab** (backward).
