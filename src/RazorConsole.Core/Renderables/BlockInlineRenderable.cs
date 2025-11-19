// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console.Rendering;

namespace RazorConsole.Core.Renderables;

/// <summary>
/// A renderable that can compose block and inline elements together.
/// Block elements start on a new line, while inline elements continue on the same line.
/// </summary>
public sealed class BlockInlineRenderable : IRenderable
{
    private readonly IReadOnlyList<RenderableItem> _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockInlineRenderable"/> class.
    /// </summary>
    /// <param name="items">The collection of items to render.</param>
    public BlockInlineRenderable(IReadOnlyList<RenderableItem> items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>
    /// Creates a block-level renderable item.
    /// </summary>
    /// <param name="renderable">The renderable to wrap.</param>
    /// <returns>A block-level renderable item.</returns>
    public static RenderableItem Block(IRenderable renderable)
    {
        return new RenderableItem(renderable, isBlock: true);
    }

    /// <summary>
    /// Creates an inline-level renderable item.
    /// </summary>
    /// <param name="renderable">The renderable to wrap.</param>
    /// <returns>An inline-level renderable item.</returns>
    public static RenderableItem Inline(IRenderable renderable)
    {
        return new RenderableItem(renderable, isBlock: false);
    }

    /// <inheritdoc/>
    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        if (_items.Count == 0)
        {
            return new Measurement(0, 0);
        }

        // Group items into lines based on block/inline behavior
        var lines = GroupIntoLines(_items);

        var minWidth = 0;
        var maxWidthNeeded = 0;

        foreach (var line in lines)
        {
            var lineMin = 0;
            var lineMax = 0;

            foreach (var item in line)
            {
                var measurement = item.Renderable.Measure(options, maxWidth);
                lineMin += measurement.Min;
                lineMax += measurement.Max;
            }

            minWidth = Math.Max(minWidth, lineMin);
            maxWidthNeeded = Math.Max(maxWidthNeeded, lineMax);
        }

        return new Measurement(minWidth, maxWidthNeeded);
    }

    /// <inheritdoc/>
    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (_items.Count == 0)
        {
            yield break;
        }

        // Group items into lines based on block/inline behavior
        var lines = GroupIntoLines(_items);

        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];

            // Render all inline items on this line
            foreach (var item in line)
            {
                foreach (var segment in item.Renderable.Render(options, maxWidth))
                {
                    yield return segment;
                }
            }

            // Add a line break after each line except the last
            if (lineIndex < lines.Count - 1)
            {
                yield return Segment.LineBreak;
            }
        }
    }

    private static List<List<RenderableItem>> GroupIntoLines(IReadOnlyList<RenderableItem> items)
    {
        var lines = new List<List<RenderableItem>>();
        var currentLine = new List<RenderableItem>();

        foreach (var item in items)
        {
            if (item.IsBlock && currentLine.Count > 0)
            {
                // Start a new line for block elements (unless it's the first item)
                lines.Add(currentLine);
                currentLine = new List<RenderableItem> { item };
            }
            else
            {
                currentLine.Add(item);
            }
        }

        // Add the last line if it has any items
        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    /// <summary>
    /// Represents an item in the block/inline flow.
    /// </summary>
    public sealed class RenderableItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderableItem"/> class.
        /// </summary>
        /// <param name="renderable">The renderable content.</param>
        /// <param name="isBlock">Whether this is a block-level element.</param>
        public RenderableItem(IRenderable renderable, bool isBlock)
        {
            Renderable = renderable ?? throw new ArgumentNullException(nameof(renderable));
            IsBlock = isBlock;
        }

        /// <summary>
        /// Gets the renderable content.
        /// </summary>
        public IRenderable Renderable { get; }

        /// <summary>
        /// Gets a value indicating whether this is a block-level element.
        /// Block elements start on a new line, inline elements continue on the same line.
        /// </summary>
        public bool IsBlock { get; }
    }
}
