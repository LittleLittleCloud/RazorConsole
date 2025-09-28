# RazorConsole built-in components roadmap

Planned additions to the RazorConsole component library.

## Proposed component families

### Layout primitives

**Rows**
Vertical layout that sequences child fragments with optional gaps.
Key parameters:
- `Expand`: Whether the row layout stretches to the full console width.

**Columns**
Horizontal layout that balances child fragments across terminal width.
Key parameters:
- `Expand`: Whether the column layout stretches to the full console width.
- `Padding`: Inner padding applied around the combined column content.

**Grid**
Two-to-four column layout with headers and automatic width balancing.
Key parameters:
- `Columns`: Total number of columns to render.
- `ShowHeaders`: Toggle visibility of header row rendering.
- `Expand`: Whether the grid stretches to fill available width.
- `Width`: Fixed width assigned to the overall grid.

**Panel**
Titled container with optional border and accent styling.
Key parameters:
- `Title`: Optional text shown in the panel header.
- `Border`: Box border style applied around the panel.
- `BorderColor`: Style applied to the panel border.
- `Padding`: Inner padding around the child content.
- `Height`: Fixed height reserved for the panel.
- `Width`: Fixed width the panel should occupy.
- `Expand`: Whether the panel grows to fill available width.

**Padder**
Wrap child content with configurable padding on each side.
Key parameters:
- `Padding`: Left, top, right, bottom padding values.
- `Expand`: Whether to stretch to the parent’s width.

**Align**
Position child content within the available console space.
Key parameters:
- `Horizontal`: Horizontal alignment inside the container.
- `Vertical`: Vertical alignment inside the container.
- `Width`: Fixed width the content should occupy.
- `Height`: Fixed height reserved for the content.

### Interaction helpers

**Prompt<T>**
Display prompt text and use `ConsoleNavigationManager` to capture input.
Key parameters:
- `Label`: Prompt text shown to the user.
- `Default`: Value returned when the user submits without input.
- `Validator`: Delegate that validates the entered value.
- `Converter`: Converts raw input into the target type `T`.

**ChoiceMenu<T>**
Render navigable choice list with shortcuts.
Key parameters:
- `Items`: Collection of selectable options.
- `OnSelect`: Callback invoked when an item is chosen.
- `HotKeys`: Optional mappings for quick selection.
- `PageSize`: Number of options displayed per page.

**Form**
Coordinated set of prompts with validation summary.
Key parameters:
- `Model`: Backing data object bound to the form.
- `Fields`: Configuration for each prompt displayed.
- `OnComplete`: Handler executed when the form submits successfully.

**Button**
Clickable action surface with variant styling and focus support.
Key parameters:
- `Label`: Text rendered inside the button when no child content is provided.
- `Variant`: Visual preset (Neutral, Primary, Success, Warning, Danger).
- `IsActive`: Highlights the button to reflect hover/focus/pressed states.
- `IsDefault`: Marks the button as the default action and surfaces the Enter shortcut.
- `Disabled`: Applies dimmed styling and removes focusability.
- `FocusKey`: Optional key used with the focus manager for navigation ordering.

### Utility primitives

**Text**
Render raw Spectre markup or plain text with optional style preset.
Key parameters:
- `Value`: Text or markup to render.
- `Style`: Optional Spectre style applied to the text.
- `IsMarkup`: Set true to treat `Value` as Spectre markup.

**Newline**
Force a line break within parent layout.
Key parameters:
- `Count`: Number of newline characters to emit.

**Spacer**
Insert vertical padding matching terminal rows.
Key parameters:
- `Lines`: Height of the spacer in rows.
- `FillCharacter`: Character used to render each spacer line.

**Spinner**
Animated indicator for background work with optional status text.
Key parameters:
- `Message`: Text displayed next to the spinner.
- `Style`: Spectre style applied to the spinner output.
- `SpinnerType`: Spinner animation preset to use.
- `AutoDismiss`: Whether to remove the spinner when work completes.
