using Spectre.Console;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Provides configuration settings for live display rendering.
/// </summary>
public sealed class ConsoleLiveDisplayOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the live display should automatically clear the console when it completes.
    /// </summary>
    public bool AutoClear { get; set; }

    /// <summary>
    /// Gets or sets the vertical overflow behaviour for the live display.
    /// </summary>
    public VerticalOverflow Overflow { get; set; } = VerticalOverflow.Ellipsis;

    /// <summary>
    /// Gets or sets the vertical overflow cropping strategy.
    /// </summary>
    public VerticalOverflowCropping Cropping { get; set; } = VerticalOverflowCropping.Top;

    /// <summary>
    /// Creates a new instance with default settings.
    /// </summary>
    public static ConsoleLiveDisplayOptions Default => new();

    internal ConsoleLiveDisplayOptions Clone()
        => new()
        {
            AutoClear = AutoClear,
            Overflow = Overflow,
            Cropping = Cropping,
        };
}
