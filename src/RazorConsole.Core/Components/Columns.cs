// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using RazorConsole.Core.Abstarctions.Components;

namespace RazorConsole.Components;

/// <summary>
/// Arranges content in horizontal columns using Spectre.Console's <see cref="global::Spectre.Console.Columns"/> renderable.
/// </summary>
/// <remarks>
/// <para>
/// Creates side-by-side layouts where each child is rendered as a column. For vertical layouts, use <see cref="Rows"/>.
/// </para>
/// <para>
/// See the <see href="https://spectreconsole.net/widgets/columns">Spectre.Console Columns documentation</see> for more information.
/// </para>
/// </remarks>
public sealed class Columns : ComponentBase, ISyntheticComponent
{
    /// <summary>
    /// Content to arrange in columns.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether columns should expand to fill available horizontal space. Default is <c>false</c>.
    /// </summary>
    [Parameter]
    public bool Expand { get; set; }

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is not null)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}

