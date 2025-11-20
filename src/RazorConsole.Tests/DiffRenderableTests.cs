// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Renderables;
using Spectre.Console.Rendering;
using static RazorConsole.Core.Utilities.AnsiSequences;

namespace RazorConsole.Tests;

public sealed class DiffRenderableTests
{
    [Fact]
    public void RenderLineDiff_SkipsUnchangedSegmentsAndWritesDiff()
    {
        var previousLine = new SegmentLine
        {
            new("prefix "),
            new("value"),
        };

        var nextLine = new SegmentLine
        {
            new("prefix "),
            new("changed"),
        };

        var result = DiffRenderable.RenderLineDiff(nextLine, previousLine).ToList();

        var controlSegments = result
            .Select(segment => segment.Text)
            .Where(text => text.Contains(ESC, StringComparison.Ordinal))
            .ToList();

        var prefixWidth = Segment.CellCount(new List<Segment>
        {
            new("prefix "),
        });

        Assert.Contains(controlSegments, text => text.Contains(CUF(prefixWidth), StringComparison.Ordinal));
        Assert.DoesNotContain(controlSegments, text => text.Contains(EL(2), StringComparison.Ordinal));
        Assert.Contains(result, segment => segment.Text == "changed");
    }

    [Fact]
    public void RenderLineDiff_ClearsTailWhenLineShrinks()
    {
        var previousLine = new SegmentLine
        {
            new("prefix "),
            new("value"),
        };

        var nextLine = new SegmentLine
        {
            new("prefix "),
        };

        var result = DiffRenderable.RenderLineDiff(nextLine, previousLine).ToList();

        var controlSegments = result
            .Select(segment => segment.Text)
            .Where(text => text.Contains(ESC, StringComparison.Ordinal))
            .ToList();

        Assert.Contains(controlSegments, text => text.Contains(EL(0), StringComparison.Ordinal));
    }
}
