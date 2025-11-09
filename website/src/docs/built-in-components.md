# Built-in Components

RazorConsole ships with a library of Spectre.Console-powered components covering layout, input, display, and utility scenarios. You can combine them just like regular Razor components to build rich terminal experiences.

## Layout

| Component | Highlights |
|-----------|------------|
| `Align` | Centers or aligns child content horizontally/vertically with optional fixed width/height. |
| `Columns` | Flows children left-to-right, optionally expanding to fill the console width. |
| `Rows` | Stacks children vertically, great for wizard-style layouts. |
| `Grid` | Builds a multi-column layout with configurable column count and width. |
| `Padder` | Adds Spectre padding around nested content to create spacing. |

```razor
<Columns Expand>
    <Markup Content="Step 1" />
    <Markup Content="Step 2" />
    <Markup Content="Step 3" />
</Columns>
```

## Input

| Component | Highlights |
|-----------|------------|
| `TextButton` | Focusable button with customizable colors and click handlers via `EventCallback`. |
| `TextInput` | Collects user input with placeholder, change handler, and optional password masking. |
| `Select` | Keyboard-driven dropdown with highlighted selection state and callbacks. |

```razor
<TextInput Value="@name" ValueChanged="@((v) => name = v)" Placeholder="Enter your name" />
<TextButton Content="Submit" OnClick="HandleSubmit" />
```

## Display

| Component | Highlights |
|-----------|------------|
| `Markup` | Renders Spectre markup with color, background, and text decoration support. |
| `Panel` | Creates framed sections with optional title and border styling. |
| `Border` | Wraps child content in a configurable border and padding. |
| `Figlet` | Produces large ASCII headers using Figlet fonts. |
| `SyntaxHighlighter` | Displays colorized source code with optional line numbers. |
| `Markdown` | Renders Markdown text directly in the console. |
| `Table` | Converts semantic `<table>` markup into Spectre tables. |

```razor
<Panel Title="Server Status" BorderColor="Color.Green" Expand>
    <Columns>
        <Markup Content="[green]✓[/] Online" />
        <Markup Content="[yellow]Latency: 42ms[/]" />
    </Columns>
</Panel>
```

## Utilities

| Component | Highlights |
|-----------|------------|
| `Spinner` | Animated progress indicator with optional message. |
| `Newline` | Emits an empty line to separate sections. |

```razor
<Spinner Message="Fetching data..." />
```

## Tips

- All components can be composed within one another—wrap inputs inside `Panel`, place buttons in `Columns`, etc.
- Spectre colors map to `Spectre.Console.Color`; use `Color.Red`, `Color.Blue`, or RGB constructors for precise styling.
- Prefer `EventCallback` parameters for handlers so components remain async-friendly.
- Combine with RazorConsole focus management features to deliver intuitive keyboard navigation.
