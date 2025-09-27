# RazorConsole built-in components roadmap

Planned additions to the RazorConsole component library.

## Proposed component families

### Layout primitives

| Component | Description | Key parameters | Rendering strategy |
|-----------|-------------|----------------|--------------------|
| `Rows` | Vertical layout that sequences child fragments with optional gaps. | `Gap`, `Alignment`, `Separator`. | Wrap Spectre `Rows` primitive and provide helper for consistent spacing and simple markdown separators.
| `Columns` | Horizontal layout that balances child fragments across terminal width. | `Spacing`, `Alignment`, `CollapseOnNarrow`. | Compose Spectre `Columns` with responsive fallback to stacked rows when the console is narrow.
| `Grid` | Two-to-four column layout with headers and automatic width balancing. | `Columns`, `Spacing`, `ShowHeaders`. | Translate into Spectre `Grid` with column builders; gracefully degrades to `Rows` when constraints are tight.
| `Panel` | Titled container with optional border and accent styling. | `Title`, `BorderColor`, `Padding`, `Orientation`. | Render Spectre `Panel` with nested layout content; support stacking within `Columns` or `Grid`.

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
| `Spinner` | Animated indicator for background work with optional status text. | `Message`, `Style`, `SpinnerType`, `AutoDismiss`. | Wrap Spectre `Spinner` with lifecycle hooks; render inline message and stop once `AutoDismiss` triggers or navigation advances.
