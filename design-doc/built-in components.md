# RazorConsole built-in components roadmap

Planned additions to the RazorConsole component library.

## Proposed component families

### Layout primitives

| Component | Description | Key parameters | Rendering strategy |
|-----------|-------------|----------------|--------------------|
| `StackPanel` | Vertical layout that sequences child fragments with optional separators. | `Gap`, `Separator`, `Alignment`. | Map to Spectre `Rows` with `Justify` on nested panels; support simple Markdown separators.
| `Grid` | 2–4 column layout with automatic width balancing. | `Columns`, `Spacing`, `ShowHeaders`. | Translate into Spectre `Grid` with column builders; fallback to stacked layout on narrow terminals.
| `PanelGroup` | Collection of titled panels that render side-by-side when there is room. | `Orientation`, `MinWidth`, `BorderColor`. | Use Spectre `Columns` with nested `Panel` objects.

### Interaction helpers

| Component | Description | Key parameters | Rendering strategy |
|-----------|-------------|----------------|--------------------|
| `Prompt<T>` | Display prompt text and use `ConsoleNavigationManager` to capture input. | `Label`, `Default`, `Validator`, `Converter`. | Encapsulate calling flow to Spectre `Prompt`; surface validation errors inline.
| `ChoiceMenu<T>` | Render navigable choice list with shortcuts. | `Items`, `OnSelect`, `HotKeys`, `PageSize`. | Compose Spectre `SelectionPrompt`; highlight selected entry using markup.
| `Form` | Coordinated set of prompts with validation summary. | `Model`, `Fields`, `OnComplete`. | Drive component model with child `FormField` definitions that execute sequential prompts.

### Utility primitives

| Component | Description | Key parameters | Rendering strategy |
|-----------|-------------|----------------|--------------------|
| `Text` | Render raw Spectre markup or plain text with optional style preset. | `Value`, `Style`, `IsMarkup`. | If `IsMarkup`, emit markup verbatim; otherwise encode and wrap in `<span data-text>` for converter.
| `Newline` | Force a line break within parent layout. | `Count`. | Emit `<div data-newline>` with count attribute; converter writes `Environment.NewLine` repeated `Count` times.
| `Spacer` | Insert vertical padding matching terminal rows. | `Lines`, `FillCharacter`. | Translate to markup that renders blank lines or repeated characters to simulate spacing.
