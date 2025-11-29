// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Renderables;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Tests.Renderables;

public class BlockInlineRenderableTests
{
    private static RenderOptions CreateTestRenderOptions()
    {
        // Create a simple console to get valid render options
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(System.IO.TextWriter.Null)
        });
        return new RenderOptions(console.Profile.Capabilities, new Size(80, 25));
    }

    [Fact]
    public void Constructor_WithNullItems_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new BlockInlineRenderable(null!));
    }

    [Fact]
    public void Constructor_WithEmptyList_DoesNotThrow()
    {
        var renderable = new BlockInlineRenderable(new List<BlockInlineRenderable.RenderableItem>());
        renderable.ShouldNotBeNull();
    }

    [Fact]
    public void Block_CreatesBlockItem()
    {
        var markup = new Markup("Hello");
        var item = BlockInlineRenderable.Block(markup);

        item.ShouldNotBeNull();
        item.IsBlock.ShouldBeTrue();
        item.Renderable.ShouldBeSameAs(markup);
    }

    [Fact]
    public void Inline_CreatesInlineItem()
    {
        var markup = new Markup("Hello");
        var item = BlockInlineRenderable.Inline(markup);

        item.ShouldNotBeNull();
        item.IsBlock.ShouldBeFalse();
        item.Renderable.ShouldBeSameAs(markup);
    }

    [Fact]
    public void Render_WithEmptyList_ReturnsNoSegments()
    {
        var renderable = new BlockInlineRenderable(new List<BlockInlineRenderable.RenderableItem>());
        var options = CreateTestRenderOptions();

        var segments = renderable.Render(options, 80);

        segments.ShouldBeEmpty();
    }

    [Fact]
    public void Render_WithSingleInlineElement_RendersWithoutLineBreak()
    {
        var items = new List<BlockInlineRenderable.RenderableItem>
        {
            BlockInlineRenderable.Inline(new Markup("Hello"))
        };

        var renderable = new BlockInlineRenderable(items);
        var options = CreateTestRenderOptions();

        var segments = new List<Segment>(renderable.Render(options, 80));

        // Should have the text segment(s) but no line break at the end
        segments.ShouldNotBeEmpty();
        segments.ShouldNotContain(s => s.IsLineBreak);
    }

    [Fact]
    public void Render_WithMultipleInlineElements_RendersOnSameLine()
    {
        var items = new List<BlockInlineRenderable.RenderableItem>
        {
            BlockInlineRenderable.Inline(new Markup("Hello")),
            BlockInlineRenderable.Inline(new Markup(" ")),
            BlockInlineRenderable.Inline(new Markup("World"))
        };

        var renderable = new BlockInlineRenderable(items);
        var options = CreateTestRenderOptions();

        var segments = new List<Segment>(renderable.Render(options, 80));

        // Should render all items without line breaks between them
        segments.ShouldNotBeEmpty();
        segments.ShouldNotContain(s => s.IsLineBreak);
    }

    [Fact]
    public void Render_WithBlockElementFirst_StartsNewBlock()
    {
        var items = new List<BlockInlineRenderable.RenderableItem>
        {
            BlockInlineRenderable.Block(new Markup("Block 1")),
            BlockInlineRenderable.Inline(new Markup("Inline after block"))
        };

        var renderable = new BlockInlineRenderable(items);
        var options = CreateTestRenderOptions();

        var segments = new List<Segment>(renderable.Render(options, 80));

        // Should have content from both items, no line break at the end
        segments.ShouldNotBeEmpty();
    }

    [Fact]
    public void Render_WithBlockElementAfterInline_CreatesNewLine()
    {
        var items = new List<BlockInlineRenderable.RenderableItem>
        {
            BlockInlineRenderable.Inline(new Markup("Inline")),
            BlockInlineRenderable.Block(new Markup("Block"))
        };

        var renderable = new BlockInlineRenderable(items);
        var options = CreateTestRenderOptions();

        var segments = new List<Segment>(renderable.Render(options, 80));

        // Should have a line break between inline and block
        segments.ShouldNotBeEmpty();
        segments.ShouldContain(s => s.IsLineBreak);
    }

    [Fact]
    public void Render_WithMultipleBlockElements_EachOnNewLine()
    {
        var items = new List<BlockInlineRenderable.RenderableItem>
        {
            BlockInlineRenderable.Block(new Markup("Block 1")),
            BlockInlineRenderable.Block(new Markup("Block 2")),
            BlockInlineRenderable.Block(new Markup("Block 3"))
        };

        var renderable = new BlockInlineRenderable(items);
        var options = CreateTestRenderOptions();

        var segments = new List<Segment>(renderable.Render(options, 80));

        // Should have line breaks between blocks
        var lineBreaks = segments.Count(s => s.IsLineBreak);
        lineBreaks.ShouldBe(2); // 2 line breaks for 3 blocks
    }

    [Fact]
    public void Render_MixedBlockAndInline_GroupsCorrectly()
    {
        var items = new List<BlockInlineRenderable.RenderableItem>
        {
            BlockInlineRenderable.Block(new Markup("Block 1")),
            BlockInlineRenderable.Inline(new Markup("Inline 1")),
            BlockInlineRenderable.Inline(new Markup("Inline 2")),
            BlockInlineRenderable.Block(new Markup("Block 2"))
        };

        var renderable = new BlockInlineRenderable(items);
        var options = CreateTestRenderOptions();

        var segments = new List<Segment>(renderable.Render(options, 80));

        // Should have 1 line break (between the two block elements)
        var lineBreaks = segments.Count(s => s.IsLineBreak);
        lineBreaks.ShouldBe(1);
    }

    [Fact]
    public void Measure_WithEmptyList_ReturnsZero()
    {
        var renderable = new BlockInlineRenderable(new List<BlockInlineRenderable.RenderableItem>());
        var options = CreateTestRenderOptions();

        var measurement = renderable.Measure(options, 80);

        measurement.Min.ShouldBe(0);
        measurement.Max.ShouldBe(0);
    }

    [Fact]
    public void Measure_WithSingleElement_ReturnsElementMeasurement()
    {
        var items = new List<BlockInlineRenderable.RenderableItem>
        {
            BlockInlineRenderable.Inline(new Markup("Hello"))
        };

        var renderable = new BlockInlineRenderable(items);
        var options = CreateTestRenderOptions();

        var measurement = renderable.Measure(options, 80);

        // Measurement should reflect the content
        measurement.Min.ShouldBeGreaterThan(0);
        measurement.Max.ShouldBeGreaterThan(0);
    }
}
