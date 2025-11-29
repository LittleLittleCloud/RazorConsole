// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Renderables;
using Spectre.Console.Rendering;
using static RazorConsole.Core.Utilities.AnsiSequences;

namespace RazorConsole.Tests.Renderables;

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

        controlSegments.ShouldContain(text => text.Contains(CUF(prefixWidth), StringComparison.Ordinal));
        controlSegments.ShouldNotContain(text => text.Contains(EL(2), StringComparison.Ordinal));
        result.ShouldContain(segment => segment.Text == "changed");
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

        controlSegments.ShouldContain(text => text.Contains(EL(0), StringComparison.Ordinal));
    }
}

