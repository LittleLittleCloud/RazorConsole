// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using RazorConsole.Core.Abstarctions.Components;
using Spectre.Console;

namespace RazorConsole.Components;

/// <summary>
/// Adds padding (whitespace) around content using Spectre.Console's <see cref="global::Spectre.Console.Padder"/> renderable.
/// </summary>
/// <remarks>
/// <para>
/// Adds spacing around elements without creating borders. Padding is specified in characters as (left, top, right, bottom).
/// </para>
/// <para>
/// See the <see href="https://spectreconsole.net/widgets/padder">Spectre.Console Padder documentation</see> for more information.
/// </para>
/// </remarks>
public sealed class Padder : ComponentBase, ISyntheticComponent
{
    private static readonly Padding NoPadding = new(0, 0, 0, 0);

    /// <summary>
    /// Content to which padding will be applied.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Padding to apply as (left, top, right, bottom) in characters. Default is (0, 0, 0, 0). Example: <c>new Padding(2, 1, 2, 1)</c> adds 2 characters horizontally and 1 line vertically.
    /// </summary>
    [Parameter]
    public Padding Padding { get; set; } = NoPadding;

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is not null)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}

