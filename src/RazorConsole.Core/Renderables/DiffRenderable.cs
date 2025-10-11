using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorConsole.Core.Utilities;
using Spectre.Console;
using Spectre.Console.Rendering;
using static RazorConsole.Core.Utilities.AnsiSequences;
namespace RazorConsole.Core.Renderables;

internal class DiffRenderable : Renderable
{
    private readonly object _lock = new object();
    private readonly IAnsiConsole _console;
    private IRenderable _renderable;
    private SegmentShape _shape = new(0, 0);
    private List<SegmentLine> _previousLines = new();
    public bool DidOverflow { get; private set; }

    /// <summary>
    /// Initializes a new instance of the DiffRenderable class to display the differences between two renderable objects
    /// using the specified console.
    /// </summary>
    public DiffRenderable(IAnsiConsole console, IRenderable renderable)
    {
        _renderable = renderable;
        _console = console;
    }

    public void UpdateRenderable(IRenderable renderable)
    {
        lock (_lock)
        {
            _renderable = renderable;
        }
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        lock (_lock)
        {
            yield return Segment.Control(RM(DECTCEM));
            DidOverflow = false;
            var segments = _renderable.Render(options, maxWidth);
            var segmentLines = Segment.SplitLines(segments);
            var shape = SegmentShape.Calculate(options, segmentLines);

            // Check for overflow
            if (shape.Height > options.ConsoleSize.Height || shape.Width > options.ConsoleSize.Width)
            {
                DidOverflow = true;
            }
            var previousLines = _previousLines ?? EmptyLines;
            var totalLines = segmentLines.Count;
            var renderFromLine = 0;
            for (renderFromLine = 0; renderFromLine < totalLines; renderFromLine++)
            {
                var line = segmentLines[renderFromLine];
                var previousLine = renderFromLine < previousLines.Count
                    ? previousLines[renderFromLine]
                    : EmptyLine;
                if (!LinesAreEqual(line, previousLine))
                {
                    break;
                }
            }

            // Move cursor to the first different line in the viewport
            int linesToMoveUp = _shape.Height - renderFromLine;
            bool needFullClear = linesToMoveUp > Console.CursorTop;
            if (needFullClear)
            {
                // The previous content is larger than the current console height, we need to clear everything
                yield return Segment.Control(ED(2) + ED(3) + CUP(1, 1));
                renderFromLine = 0;
                linesToMoveUp = 0;
                previousLines = EmptyLines;
                renderFromLine = 0;
            }
            else
            {
                for (var i = 0; i < linesToMoveUp; i++)
                {
                    var previousLineIndex = previousLines.Count - i;
                    if (previousLineIndex >= totalLines)
                    {
                        // The previous line is beyond the current total lines, move up and clear
                        yield return Segment.Control(EL(2) + CUU(1));
                    }
                    else
                    {
                        // just move up
                        yield return Segment.Control(CUU(1));
                    }
                }
            }

            // Render from the first different line
            for (var i = renderFromLine; i < totalLines; i++)
            {
                var line = segmentLines[i];
                var previousLine = i < previousLines.Count
                    ? previousLines[i]
                    : EmptyLine;

                if (!LinesAreEqual(line, previousLine))
                {
                    foreach (var segment in RenderLineDiff(line, previousLine))
                    {
                        yield return segment;
                    }
                }

                yield return Segment.Control(NEL());
            }

            // Update the previous lines for next comparison
            _previousLines = CloneLines(segmentLines);
            _shape = shape;
            yield return Segment.Control(SM(DECTCEM));
            yield break;
        }
    }

    private static bool LinesAreEqual(SegmentLine line1, SegmentLine line2)
    {
        if (line1.Count != line2.Count)
        {
            return false;
        }

        for (var i = 0; i < line1.Count; i++)
        {
            var segment1 = line1[i];
            var segment2 = line2[i];

            if (!SegmentsAreEqual(segment1, segment2))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SegmentsAreEqual(Segment segment1, Segment segment2)
    {
        return string.Equals(segment1.Text, segment2.Text, StringComparison.Ordinal)
            && Equals(segment1.Style, segment2.Style);
    }

    internal static IEnumerable<Segment> RenderLineDiff(SegmentLine line, SegmentLine previousLine)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }

        if (previousLine is null)
        {
            throw new ArgumentNullException(nameof(previousLine));
        }

        var minSegmentCount = Math.Min(line.Count, previousLine.Count);
        var firstDifferentSegmentIndex = 0;
        for (; firstDifferentSegmentIndex < minSegmentCount; firstDifferentSegmentIndex++)
        {
            if (!SegmentsAreEqual(line[firstDifferentSegmentIndex], previousLine[firstDifferentSegmentIndex]))
            {
                break;
            }
        }

        var prefixWidth = firstDifferentSegmentIndex > 0
            ? Segment.CellCount(line.GetRange(0, firstDifferentSegmentIndex))
            : 0;

        if (prefixWidth > 0)
        {
            yield return Segment.Control(CUF(prefixWidth));
        }

        for (var segmentIndex = firstDifferentSegmentIndex; segmentIndex < line.Count; segmentIndex++)
        {
            yield return line[segmentIndex];
        }

        var currentLineWidth = Segment.CellCount(line);
        var previousLineWidth = Segment.CellCount(previousLine);
        if (currentLineWidth < previousLineWidth)
        {
            yield return Segment.Control(EL(0));
        }
    }

    private static List<SegmentLine> CloneLines(List<SegmentLine> source)
    {
        var result = new List<SegmentLine>(source.Count);
        foreach (var line in source)
        {
            result.Add(new SegmentLine(line));
        }

        return result;
    }

    private static readonly List<SegmentLine> EmptyLines = new(0);
    private static readonly SegmentLine EmptyLine = new();
}
