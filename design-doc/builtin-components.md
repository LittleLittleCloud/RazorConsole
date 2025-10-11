# Built-in RazorConsole Components

This reference summarizes the Razor components that ship with RazorConsole. Each component projects to a Spectre.Console renderable at runtime. When composing components, treat them as immutable outside of their rendering lifecycle and prefer configuring them through parameters.

> **Tip:** Components marked with `RenderFragment? ChildContent` accept arbitrary nested markup.

## Align
Wraps child content in an alignment container.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | — | Nested content to align. |
| `Horizontal` | `HorizontalAlignment` | `Left` | Horizontal alignment (`Left`, `Center`, `Right`). |
| `Vertical` | `VerticalAlignment` | `Top` | Vertical alignment (`Top`, `Middle`, `Bottom`). |
| `Width` | `int?` | `null` | Fixed width in characters when greater than zero. |
| `Height` | `int?` | `null` | Fixed height in rows when greater than zero. |

## Border
Creates a bordered panel around its children.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | — | Content rendered inside the border. |
| `BorderColor` | `Color?` | `null` | Optional border color. |
| `BoxBorder` | `BoxBorder` | `Rounded` | Border style (`Rounded`, `Square`, `Double`, etc.). |
| `Padding` | `Padding` | `new(0,0,0,0)` | Inner padding inside the border. |

## Columns
Flow child renderables in Spectre.Console columns.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | — | Column items. |
| `Expand` | `bool` | `false` | When `true`, forces columns to fill the available width. |

## Figlet
Renders large ASCII art text using Figlet fonts.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Content` | `string` | `string.Empty` | Text to render. |
| `Justify` | `Justify` | `Center` | Alignment inside the Figlet block. |
| `Color` | `Color` | `Color.Default` | Foreground color for the glyphs. |

## Grid
Builds a Spectre.Console grid with configurable columns.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | — | Grid rows and cells. |
| `Columns` | `int` | `2` | Number of columns in the grid. |
| `Expand` | `bool` | `false` | Stretch grid to available width. |
| `Width` | `int?` | `null` | Fixed width when greater than zero. |

## Markup
Outputs Spectre markup with styling.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Content` ⚠️ | `string` | — | Text to render; automatically escaped. Required. |
| `Foreground` | `Color` | `Style.Plain.Foreground` | Text color. |
| `Background` | `Color` | `Style.Plain.Background` | Background color. |
| `Decoration` | `Decoration` | `None` | Styling flags (`Bold`, `Italic`, etc.). |
| `link` | `string?` | `null` | Optional hyperlink target. |

## Newline
Emits a single line break via `<Markup>` with `Environment.NewLine`. No parameters.

## Padder
Adds padding around nested content.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | — | Inner content. |
| `Padding` | `Padding` | `new(0,0,0,0)` | Padding thickness (left, top, right, bottom). |

## Panel
Full-featured Spectre panel wrapper.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | — | Panel body content. |
| `Title` | `string?` | `null` | Optional panel header text. |
| `TitleColor` | `Color?` | `null` | Color for the header text. |
| `BorderColor` | `Color?` | `null` | Panel border color. |
| `Border` | `BoxBorder?` | `null` | Specific border style; defaults to Spectre's standard when `null`. |
| `Height` | `int?` | `null` | Fixed height when positive. |
| `Padding` | `Padding?` | `null` | Insets between border and content. |
| `Width` | `int?` | `null` | Fixed width when positive. |
| `Expand` | `bool` | `false` | Stretch panel to available width. |

## Rows
Stacks child renderables vertically.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | — | Row items. |
| `Expand` | `bool` | `false` | When `true`, rows fill the available height. |

## Select
Interactive dropdown for choosing a value. Integrates with the focus and keyboard systems.

**Core parameters**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Options` | `string[]` | `Array.Empty<string>()` | Available options. |
| `Value` | `string?` | `null` | Current selection. |
| `ValueChanged` | `EventCallback<string?>` | — | Raised on selection change. |
| `OnSelected` | `EventCallback<string?>` | — | Fired after a user confirms an option. |
| `OnClear` | `EventCallback` | — | Fired when selection is cleared. |
| `Placeholder` | `string` | `"Select an option"` | Placeholder when no selection is set. |
| `Expand` | `bool` | `false` | Stretch the control horizontally. |
| `FocusOrder` | `int?` | `null` | Order key for focus navigation. |
| `BorderStyle` | `BoxBorder` | `Rounded` | Border style for the input panel. |

**Appearance parameters**

| Parameter | Role | Default |
|-----------|------|---------|
| `PlaceholderColor` / `Decoration` | Styling when no selection exists. | `Grey70`, `Italic | Dim` |
| `ValueColor` / `Decoration` | Styling for selected value. | `White`, `None` |
| `EmptyLabel` / `EmptyForeground` / `EmptyDecoration` | Displayed when `Options` is empty. | `"No options available"`, `Grey70`, `Italic` |
| `OptionForeground` / `OptionDecoration` | Styling for list options. | `White`, `None` |
| `SelectedOptionForeground` / `SelectedOptionDecoration` | Highlight for the focused option. | `Chartreuse1`, `Bold` |

Keyboard navigation supports arrows, <kbd>Space</kbd> to toggle, <kbd>Enter</kbd> to commit, <kbd>Escape</kbd> to cancel, and type-ahead letters for quick jumps.

## Spinner
Shows a Spectre spinner with optional message.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SpinnerType` | `Spinner` | `Spinner.Known.Dots` | Spinner instance to render. |
| `SpinnerName` | `string?` | `null` | Explicit spinner name override. |
| `Message` | `string?` | `null` | Optional message displayed alongside the spinner. |
| `Style` | `string?` | `null` | Spectre style markup for the spinner and message. |
| `AutoDismiss` | `bool` | `false` | Remove spinner automatically when completed. |

## SyntaxHighlighter
Renders highlighted code blocks with `SyntaxHighlightingService`.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Code` | `string` | `string.Empty` | Source text to highlight. |
| `Language` | `string?` | `null` | Language identifier (e.g., `"csharp"`). |
| `Theme` | `string?` | `null` | Theme key registered with the service. |
| `ShowLineNumbers` | `bool` | `false` | Display line numbers when `true`. |
| `Options` | `SyntaxOptions?` | `null` | Optional preconfigured syntax options. |
| `TabWidth` | `int?` | `null` | Overrides `Options.TabWidth` when > 0. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object?>?` | `null` | Pass-through attributes for advanced scenarios. |

### Default language keys
The built-in `ColorCodeLanguageRegistry` ships with the following language keys (case-insensitive). Aliases map to the same syntax highlighter:

- `text`, `plaintext`, `plain`
- `csharp`, `cs`
- `razor`
- `html`
- `json`
- `xml`
- `sql`
- `js`, `javascript`
- `ts`, `typescript`
- `css`
- `powershell`, `ps`
- `python`
- `md`, `markdown`

Register additional languages via `ISyntaxLanguageRegistry.Register` during application startup.

## TextButton
Clickable text button that changes background while focused.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Content` | `string` | `string.Empty` | Button label. |
| `BackgroundColor` | `Color` | `Color.Default` | Background when idle. |
| `FocusedColor` | `Color` | `Color.Yellow` | Background when focused. |
| `OnClick` | `EventCallback` | — | Fired when the button is clicked. |

## TextInput
Text entry control with placeholder, masking, and focus management.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `string?` | `null` | Two-way bound text value. |
| `ValueChanged` | `EventCallback<string?>` | — | Raised on input changes. |
| `ValueExpression` | `Expression<Func<string?>>?` | `null` | Used by validation frameworks. |
| `OnInput` | `EventCallback<string?>` | — | Fires for every keystroke. |
| `OnSubmit` | `EventCallback<string?>` | — | Fires when the control dispatches a change event. |
| `OnFocus` / `OnBlur` | `EventCallback<FocusEventArgs>` | — | Focus lifecycle hooks. |
| `Label` | `string?` | `null` | Optional label rendered before the input. |
| `LabelColor` / `LabelDecoration` | `Color`, `Decoration` | `White`, `None` | Label styling. |
| `Placeholder` | `string?` | `null` | Placeholder text when value is empty. |
| `ValueColor` | `Color` | `White` | Foreground of typed text. |
| `PlaceholderColor` | `Color` | `Grey` | Placeholder foreground. |
| `PlaceholderDecoration` | `Decoration` | `Dim | Italic` | Placeholder styling. |
| `BorderColor` | `Color` | `Grey37` | Idle border color. |
| `FocusedBorderColor` | `Color` | `Yellow` | Border color while focused. |
| `DisabledBorderColor` | `Color` | `Grey19` | Border color when disabled. |
| `BorderStyle` | `BoxBorder` | `Rounded` | Spectre border style. |
| `BorderPadding` | `Padding` | `new(0,0,0,0)` | Padding around the panel. |
| `ContentPadding` | `Padding` | `new(1,0,1,0)` | Padding around the text content. |
| `Expand` | `bool` | `false` | Stretch to available width. |
| `Disabled` | `bool` | `false` | Disables input and focus.
| `MaskInput` | `bool` | `false` | Replace characters with bullets while displayed. |
| `FocusOrder` | `int?` | `null` | Order key for focus traversal. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object?>?` | `null` | Arbitrary attributes forwarded to the root element. |

Keyboard handlers adjust border colors and manage focus state automatically.
