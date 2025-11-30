// Copyright (c) RazorConsole. All rights reserved.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using RazorConsole.Core.Abstarctions.Components;
using Spectre.Console;

namespace RazorConsole.Components;

/// <summary>
/// Renders a bar chart using Spectre.Console's <see cref="global::Spectre.Console.BarChart"/> renderable.
/// </summary>
/// <remarks>
/// <para>
/// Displays data as horizontal bars with optional labels and values. Each bar can have a custom color.
/// </para>
/// <para>
/// See the <see href="https://spectreconsole.net/widgets/barchart">Spectre.Console BarChart documentation</see> for more information.
/// </para>
/// </remarks>
public sealed class BarChart : ComponentBase, ISyntheticComponent
{
    /// <summary>
    /// Bar chart data items. Each item must provide <c>Label</c> and <c>Value</c>, with optional <c>Color</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// new BarChartItem("Jan", 12, Color.Red)
    /// </code>
    /// </example>
    [Parameter]
    [EditorRequired]
    public required List<IBarChartItem> BarChartItems { get; set; }

    /// <summary>
    /// Width of the bar chart in characters. If <c>null</c>, uses available console width.
    /// </summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>
    /// Label (title) displayed above the bar chart.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Foreground (text) color of the chart label. Default is <see cref="Style.Plain"/> foreground.
    /// </summary>
    [Parameter]
    public Color LabelForeground { get; set; } = Style.Plain.Foreground;

    /// <summary>
    /// Background color of the chart label. Default is <see cref="Style.Plain"/> background (transparent).
    /// </summary>
    [Parameter]
    public Color LabelBackground { get; set; } = Style.Plain.Background;

    /// <summary>
    /// Text decoration for the chart label (bold, italic, underline, etc.). Default is <see cref="Decoration.None"/>.
    /// </summary>
    [Parameter]
    public Decoration LabelDecoration { get; set; } = Decoration.None;

    /// <summary>
    /// Alignment of the chart label. Options: <see cref="Justify.Left"/>, <see cref="Justify.Center"/>, <see cref="Justify.Right"/>.
    /// </summary>
    [Parameter]
    public Justify? LabelAlignment { get; set; }

    /// <summary>
    /// Fixed maximum value for scaling bars. When set, bars scale relative to this value instead of the largest data point. Example: <c>MaxValue = 100</c> creates a progress chart (0–100%).
    /// </summary>
    [Parameter]
    public double? MaxValue { get; set; }

    /// <summary>
    /// Whether numeric values should be displayed next to each bar. Default is <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ShowValues { get; set; }

    /// <summary>
    /// CultureInfo to use when rendering values. Default is <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    [Parameter]
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Synthetic component - rendering is handled by BarChartTranslator
    }
}

