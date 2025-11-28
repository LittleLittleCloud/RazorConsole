// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using RazorConsole.Core.Abstarctions.Components;
using Spectre.Console;

namespace RazorConsole.Components;

/// <summary>
/// Renders text in large ASCII art letters using Spectre.Console's <see cref="global::Spectre.Console.FigletText"/> renderable.
/// </summary>
/// <remarks>
/// <para>
/// Creates eye-catching headers and titles using FIGlet ASCII art format.
/// </para>
/// <para>
/// See the <see href="https://spectreconsole.net/widgets/figlet">Spectre.Console FigletText documentation</see> for more information.
/// </para>
/// </remarks>
public sealed class Figlet : ComponentBase, ISyntheticComponent
{
    /// <summary>
    /// Text content to render as FIGlet ASCII art.
    /// </summary>
    [Parameter]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Horizontal alignment of the FIGlet text. Default is <see cref="Justify.Center"/>.
    /// </summary>
    [Parameter]
    public Justify Justify { get; set; } = Justify.Center;

    /// <summary>
    /// Color of the FIGlet text. Default is <see cref="Color.Default"/> (console's default color).
    /// </summary>
    [Parameter]
    public Color Color { get; set; } = Color.Default;

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Synthetic component - rendering is handled by FigletElementTranslator
    }
}

