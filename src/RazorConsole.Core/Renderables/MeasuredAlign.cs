// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Renderables;

/// <summary>
/// Represents a renderable used to align content.
/// </summary>
public sealed class MeasuredAlign : Renderable
{
    private readonly IRenderable _renderable;

    protected override Measurement Measure(RenderOptions options, int maxWidth)
    {
        var width = Math.Min(Width ?? maxWidth, maxWidth);
        var height = Height ?? options.Height;
        var measurement = _renderable.Measure(options with { Height = height }, width);
        return new Measurement(measurement.Min, width);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment.
    /// </summary>
    public HorizontalAlignment Horizontal { get; set; } = HorizontalAlignment.Left;

    /// <summary>
    /// Gets or sets the vertical alignment.
    /// </summary>
    public VerticalAlignment? Vertical { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Align"/> class.
    /// </summary>
    /// <param name="renderable">The renderable to align.</param>
    /// <param name="horizontal">The horizontal alignment.</param>
    /// <param name="vertical">The vertical alignment, or <c>null</c> if none.</param>
    public MeasuredAlign(IRenderable renderable, HorizontalAlignment horizontal, VerticalAlignment? vertical = null)
    {
        _renderable = renderable ?? throw new ArgumentNullException(nameof(renderable));

        Horizontal = horizontal;
        Vertical = vertical;
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var rendered = _renderable.Render(options with { Height = null }, maxWidth);
        var lines = Segment.SplitLines(rendered);

        var width = Math.Min(Width ?? maxWidth, maxWidth);
        var height = Height ?? options.Height;

        var blank = new SegmentLine(new[] { new Segment(new string(' ', width)) });

        // Align vertically
        if (Vertical != null && height != null)
        {
            switch (Vertical)
            {
                case VerticalAlignment.Top:
                {
                    var diff = height - lines.Count;
                    for (var i = 0; i < diff; i++)
                    {
                        lines.Add(blank);
                    }

                    break;
                }

                case VerticalAlignment.Middle:
                {
                    var top = (height - lines.Count) / 2;
                    var bottom = height - top - lines.Count;

                    for (var i = 0; i < top; i++)
                    {
                        lines.Insert(0, blank);
                    }

                    for (var i = 0; i < bottom; i++)
                    {
                        lines.Add(blank);
                    }

                    break;
                }

                case VerticalAlignment.Bottom:
                {
                    var diff = height - lines.Count;
                    for (var i = 0; i < diff; i++)
                    {
                        lines.Insert(0, blank);
                    }

                    break;
                }

                default:
                    throw new NotSupportedException("Unknown vertical alignment");
            }
        }

        // Align horizontally
        foreach (var line in lines)
        {
            MeasuredAlign.AlignHorizontally(line, Horizontal, width);
        }

        return new SegmentLineEnumerator(lines);
    }

    public static void AlignHorizontally<T>(T segments, HorizontalAlignment alignment, int maxWidth)
        where T : List<Segment>
    {
        var width = Segment.CellCount(segments);
        if (width >= maxWidth)
        {
            return;
        }

        switch (alignment)
        {
            case HorizontalAlignment.Left:
            {
                var diff = maxWidth - width;
                segments.Add(Segment.Padding(diff));
                break;
            }

            case HorizontalAlignment.Right:
            {
                var diff = maxWidth - width;
                segments.Insert(0, Segment.Padding(diff));
                break;
            }

            case HorizontalAlignment.Center:
            {
                // Left side.
                var diff = (maxWidth - width) / 2;
                segments.Insert(0, Segment.Padding(diff));

                // Right side
                segments.Add(Segment.Padding(diff));
                var remainder = (maxWidth - width) % 2;
                if (remainder != 0)
                {
                    segments.Add(Segment.Padding(remainder));
                }

                break;
            }

            default:
                throw new NotSupportedException("Unknown alignment");
        }
    }
}

