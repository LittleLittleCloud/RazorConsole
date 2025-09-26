using System;

namespace RazorConsole.Core.Input;

/// <summary>
/// Represents input captured from the console during a controller execution cycle.
/// </summary>
public sealed record ConsoleInputContext
{
    /// <summary>
    /// Gets or sets free-form text entered by the user.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Gets or sets a command keyword that the shell interpreted (optional).
    /// </summary>
    public string? Command { get; init; }

    /// <summary>
    /// Gets or sets arbitrary metadata associated with most recent input event.
    /// </summary>
    public object? Tag { get; init; }

    /// <summary>
    /// Factory helper for text input.
    /// </summary>
    public static ConsoleInputContext FromText(string? text)
        => new() { Text = text };

    /// <summary>
    /// Returns a reusable empty context.
    /// </summary>
    public static ConsoleInputContext Empty { get; } = new();

    /// <summary>
    /// Normalizes the text field (trim + null for whitespace) and returns a new instance.
    /// </summary>
    public ConsoleInputContext Normalize()
    {
        var normalized = string.IsNullOrWhiteSpace(Text) ? null : Text.Trim();
        if (normalized == Text)
        {
            return this;
        }

        return this with { Text = normalized };
    }
}
