// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;

using RazorConsole.Core.Abstarctions.Components;
using Spectre.Console;

namespace RazorConsole.Components;

/// <summary>
/// Renders a pixel-based canvas for drawing graphics in the console using Spectre.Console's <see cref="global::Spectre.Console.Canvas"/> renderable.
/// </summary>
/// <remarks>
/// <para>
/// This component wraps Spectre.Console's Canvas to enable pixel-based drawing of graphics, charts, and pixel art.
/// Pixels are rendered using console characters with specified colors at (x, y) coordinates.
/// </para>
/// <para>
/// See the <see href="https://spectreconsole.net/widgets/canvas">Spectre.Console Canvas documentation</see> for more information.
/// </para>
/// </remarks>
public sealed class SpectreCanvas : ComponentBase, ISyntheticComponent, IDisposable
{
    /// <summary>
    /// The array of pixels to render. Each pixel is defined by (x, y) coordinates and color.
    /// </summary>
    /// <remarks>
    /// Total pixel count must not exceed <c>CanvasWidth * CanvasHeight</c>.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public (int x, int y, Color color)[] Pixels { get; set; } = [];

    /// <summary>
    /// Width of the canvas in pixels.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public int CanvasWidth { get; set; }

    /// <summary>
    /// Height of the canvas in pixels.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public int CanvasHeight { get; set; }

    /// <summary>
    /// Maximum width of the rendered canvas in console characters. If <c>null</c>, no constraint is applied.
    /// </summary>
    [Parameter]
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Width of each pixel when rendered. Default is 2 characters, creating more square-looking pixels.
    /// </summary>
    [Parameter]
    public int PixelWidth { get; set; } = 2;

    /// <summary>
    /// Whether the canvas should automatically scale to fit the console. Default is <c>false</c>.
    /// </summary>
    [Parameter]
    public bool Scale { get; set; }

    private (int x, int y, Color color)[]? _lastPixelsReference;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ValidatePixelsLength();
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ValidatePixelsLength();

        if (!ReferenceEquals(_lastPixelsReference, Pixels))
        {
            _lastPixelsReference = Pixels;
        }
    }

    private void ValidatePixelsLength()
    {
        if (Pixels.Length > CanvasWidth * CanvasHeight)
        {
            throw new ArgumentException(
                $"Canvas pixels count ({Pixels.Length}) must be <= canvas area ({CanvasWidth * CanvasHeight}).");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // No cleanup needed - Pixels are passed directly via Attrs
    }
}

