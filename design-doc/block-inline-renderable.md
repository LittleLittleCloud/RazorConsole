# BlockInlineRenderable

The `BlockInlineRenderable` is a custom `IRenderable` implementation that handles the composition of block-level and inline-level elements, providing more precise control over layout rendering in RazorConsole.

## Concept

In HTML and typesetting, elements are categorized as:

- **Block elements**: Start on a new line (e.g., `<div>`, `<p>`, `<panel>`)
- **Inline elements**: Continue on the same line (e.g., `<span>`, `<strong>`, `<em>`)

The `BlockInlineRenderable` brings this concept to Spectre.Console rendering, allowing you to mix block and inline elements in a single renderable while maintaining proper flow.

## Usage

### Basic Example

```csharp
using RazorConsole.Core.Renderables;
using Spectre.Console;

// Create a mix of block and inline elements
var items = new List<BlockInlineRenderable.RenderableItem>
{
    // This starts on its own line
    BlockInlineRenderable.Block(new Markup("[bold]Title:[/]")),
    
    // These continue on the same line as the title
    BlockInlineRenderable.Inline(new Markup(" ")),
    BlockInlineRenderable.Inline(new Markup("[green]Success[/]")),
    
    // This starts on a new line
    BlockInlineRenderable.Block(new Markup("[italic]Description:[/]")),
    
    // These continue on the same line as the description
    BlockInlineRenderable.Inline(new Markup(" ")),
    BlockInlineRenderable.Inline(new Markup("The operation completed successfully.")),
};

var renderable = new BlockInlineRenderable(items);
AnsiConsole.Write(renderable);
```

Output:
```
Title: Success
Description: The operation completed successfully.
```

### Flow Behavior

The renderable groups items into lines based on their block/inline nature:

1. Block elements always start a new line (unless they're the first element)
2. Inline elements continue on the current line
3. Multiple consecutive inline elements flow together on the same line
4. A block element after inline elements causes a line break

### Example: Mixed Content

```csharp
var items = new List<BlockInlineRenderable.RenderableItem>
{
    BlockInlineRenderable.Inline(new Markup("Inline 1 ")),
    BlockInlineRenderable.Inline(new Markup("[bold]Inline 2[/] ")),
    BlockInlineRenderable.Inline(new Markup("Inline 3")),
    BlockInlineRenderable.Block(new Panel("Block 1")),
    BlockInlineRenderable.Block(new Panel("Block 2")),
    BlockInlineRenderable.Inline(new Markup("After blocks")),
};
```

This renders as:
```
Inline 1 Inline 2 Inline 3
┌─────────┐
│ Block 1 │
└─────────┘
┌─────────┐
│ Block 2 │
└─────────┘
After blocks
```

## API

### Static Factory Methods

#### `Block(IRenderable renderable)`
Creates a block-level item that starts on a new line.

```csharp
var blockItem = BlockInlineRenderable.Block(new Markup("[bold]Header[/]"));
```

#### `Inline(IRenderable renderable)`
Creates an inline-level item that continues on the current line.

```csharp
var inlineItem = BlockInlineRenderable.Inline(new Markup("text"));
```

### Constructor

```csharp
public BlockInlineRenderable(IReadOnlyList<RenderableItem> items)
```

Creates a new `BlockInlineRenderable` with the specified items.

### RenderableItem Class

```csharp
public sealed class RenderableItem
{
    public IRenderable Renderable { get; }
    public bool IsBlock { get; }
}
```

Represents a single item in the block/inline flow.

## Integration with VdomSpectreTranslator

The `BlockInlineRenderable` can be used within custom translators to provide better control over element layout:

```csharp
public sealed class CustomElementTranslator : IVdomElementTranslator
{
    public int Priority => 50;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        if (node.Kind != VNodeKind.Element || node.TagName != "custom")
        {
            renderable = null;
            return false;
        }

        var items = new List<BlockInlineRenderable.RenderableItem>();
        
        foreach (var child in node.Children)
        {
            if (context.TryTranslate(child, out var childRenderable))
            {
                // Determine if this child should be block or inline
                var isBlock = IsBlockElement(child);
                if (isBlock)
                {
                    items.Add(BlockInlineRenderable.Block(childRenderable));
                }
                else
                {
                    items.Add(BlockInlineRenderable.Inline(childRenderable));
                }
            }
        }

        renderable = new BlockInlineRenderable(items);
        return true;
    }

    private bool IsBlockElement(VNode node)
    {
        // Logic to determine if element is block-level
        return node.TagName switch
        {
            "div" => true,
            "p" => true,
            "panel" => true,
            _ => false
        };
    }
}
```

## Implementation Details

- The renderable groups items into logical lines during rendering
- Each line is rendered with its items, followed by a line break (except for the last line)
- Measurement aggregates the dimensions of all lines
- Line breaks between blocks are automatic; no manual line break elements needed

## See Also

- `Rows` - Spectre.Console's built-in vertical stacking renderable
- `Columns` - Spectre.Console's horizontal flow renderable
- `HtmlInlineTextElementTranslator` - Handles inline HTML text formatting
