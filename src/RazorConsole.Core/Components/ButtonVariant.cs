namespace RazorConsole.Components;

/// <summary>
/// Describes built-in color/style presets for <see cref="Button"/> components.
/// </summary>
public enum ButtonVariant
{
    /// <summary>
    /// Neutral button styling with muted gray tones.
    /// </summary>
    Neutral = 0,

    /// <summary>
    /// Primary action styling using an accent color.
    /// </summary>
    Primary,

    /// <summary>
    /// Success styling for confirming operations.
    /// </summary>
    Success,

    /// <summary>
    /// Warning styling for cautious actions.
    /// </summary>
    Warning,

    /// <summary>
    /// Danger styling for destructive or irreversible actions.
    /// </summary>
    Danger,
}
