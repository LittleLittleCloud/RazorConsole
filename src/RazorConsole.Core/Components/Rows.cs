// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using RazorConsole.Core.Abstarctions.Components;

namespace RazorConsole.Components;

/// <summary>
/// Arranges content in vertical rows using Spectre.Console's <see cref="global::Spectre.Console.Rows"/> renderable.
/// </summary>
/// <remarks>
/// <para>
/// Creates stacked layouts where elements are arranged vertically. For horizontal layouts, use <see cref="Columns"/>.
/// </para>
/// <para>
/// See the <see href="https://spectreconsole.net/widgets/rows">Spectre.Console Rows documentation</see> for more information.
/// </para>
/// </remarks>
public sealed class Rows : ComponentBase, ISyntheticComponent
{
    /// <summary>
    /// Content to arrange in rows.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether rows should expand to fill available vertical space. Default is <c>false</c>.
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

