// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console;

namespace RazorConsole.Core.Rendering.Syntax;

/// <summary>
/// Options that control syntax highlighting output.
/// </summary>
public sealed record SyntaxOptions
{
    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static SyntaxOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets the number of spaces used to expand tab characters.
    /// </summary>
    public int TabWidth { get; init; } = 4;

    /// <summary>
    /// Gets or sets the style used for line numbers when they are rendered.
    /// </summary>
    public Style LineNumberStyle { get; init; } = new(Color.Grey62, Color.Default, Decoration.Dim);

    /// <summary>
    /// Gets or sets the placeholder text written when no code is provided.
    /// </summary>
    public string PlaceholderMarkup { get; init; } = "[dim](no code)[/]";
}
