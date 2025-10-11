using Spectre.Console;

namespace RazorConsole.Core.Rendering;

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
    /// Creates a new instance with default settings.
    /// </summary>
    public static ConsoleLiveDisplayOptions Default => new();

    internal ConsoleLiveDisplayOptions Clone()
        => new()
        {
            AutoClear = AutoClear,
        };
}
