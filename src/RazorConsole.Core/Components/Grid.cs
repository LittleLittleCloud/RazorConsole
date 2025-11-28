// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using RazorConsole.Core.Abstarctions.Components;

namespace RazorConsole.Components;

/// <summary>
/// Arranges content in a multi-column grid using Spectre.Console's <see cref="global::Spectre.Console.Grid"/> renderable.
/// </summary>
/// <remarks>
/// <para>
/// Automatically arranges children into the specified number of columns, wrapping to new rows as needed (similar to CSS grid).
/// </para>
/// <para>
/// See the <see href="https://spectreconsole.net/widgets/grid">Spectre.Console Grid documentation</see> for more information.
/// </para>
/// </remarks>
public sealed class Grid : ComponentBase, ISyntheticComponent
{
    /// <summary>
    /// Content to arrange in the grid.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Number of columns in the grid. Default is 2. Must be positive.
    /// </summary>
    [Parameter]
    public int Columns { get; set; } = 2;

    /// <summary>
    /// Whether the grid should expand to fill available space. Default is <c>false</c>.
    /// </summary>
    [Parameter]
    public bool Expand { get; set; }

    /// <summary>
    /// Width of the grid in characters. If <c>null</c>, automatically determined by content.
    /// </summary>
    [Parameter]
    public int? Width { get; set; }

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is not null)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}

