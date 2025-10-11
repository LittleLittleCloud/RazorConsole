# Syntax highlighter design

## Purpose and scope
- Render code samples in the console with language-aware styling so snippets in the gallery and diagnostic flows stay readable.
- Provide an extensible tokenization and theming pipeline that is decoupled from Spectre.Console specifics.
- Support incremental updates so the highlighter can participate in live preview and hot-reload scenarios without repainting the entire buffer.

## Goals
- Ship a reusable Razor component (`<SyntaxHighlighter>`) that highlights an arbitrary text fragment using a chosen language definition.
- Integrate [ColorCode](https://github.com/ColorCode/ColorCode-Universal) (via `ColorCode.Core`) for language parsing and token classification.
- Allow dynamic language selection at runtime and support a pluggable registry for custom language packs on top of ColorCode.
- Support at least C# and Razor syntax with efficient reuse of ColorCode tokenization and minimal allocations when converting to Spectre markup.
- Preserve whitespace, indentation, and line numbers when requested.
- Keep renderables immutable once produced to align with existing rendering loops.

## Non-goals
- Building a full compiler-grade parser or enforcing semantic correctness.
- Providing IDE-grade features such as code folding, symbol navigation, or inline diagnostics.
- Handling megabyte-size files or streaming gigabytes of log data; focus on snippets under a few hundred lines.

## Key scenarios
1. **Gallery demos**: showcase highlighted code samples alongside live component output, using shared themes with the rest of the gallery.
2. **Diagnostics and tutorials**: embed highlighted Razor or C# snippets inside panels, prompts, or walkthroughs rendered in Spectre.Console.
3. **Hot reload previews**: re-highlight only the modified regions when Razor source changes, minimizing flicker in the console output.

## Architectural overview
The highlighter is divided into three layers:

1. **Language definition & tokenization**
   - Lean on ColorCode's built-in `ILanguage` implementations for token classification (C#, Razor, etc.).
   - Maintain a thin registry (`ColorCodeLanguageRegistry`) that maps language keys to `ILanguage` instances and allows registering custom ColorCode languages.
   - Avoid custom tokenizers unless ColorCode lacks coverage; in that case, wrap additional `ILanguage` implementations inside the registry.

2. **Markup formatting**
   - Create `SpectreMarkupFormatter`, deriving from ColorCode's `CodeColorizerBase`, that converts ColorCode scopes into Spectre.Console markup spans.
   - Formatter looks up styles from `SyntaxTheme` and emits appropriately escaped markup segments, preserving whitespace and optional line numbers.
   - Themes remain DI-registered (`services.AddSyntaxThemes()`), returning cached `Style` instances keyed by ColorCode scope names.
   - Refer to the upstream implementation in the `ColorCode-Universal` repository (`ColorCode.Core/Common/CodeColorizerBase.cs`) to understand the overriding contract and required scope-handling callbacks.

3. **Rendering and components**
   - `SyntaxRenderable` implements `IRenderable`, consuming the markup produced by `SpectreMarkupFormatter` and writing styled segments via `IRenderContext`.
   - `<SyntaxHighlighter>` Razor component wraps the renderable and accepts parameters:
     - `Code` (`string`) – required source text.
     - `Language` (`string?`) – language key; defaults to global option.
     - `ShowLineNumbers` (`bool`) – toggles line gutter.
     - `Theme` (`string?`) – optional theme override.
   - Components are placed under `src/RazorConsole.Core/Components/Syntax/` with code-behind classes in `.razor.cs` files when logic exceeds markup.

### Data flow
```
Code string ➡ Tokenizer (ISyntaxTokenizer) ➡ IReadOnlyList<SyntaxToken>
        ➡ SyntaxRenderable + SyntaxTheme ➡ Spectre.Console Canvas ➡ Console output
```

### Hot reload integration
- Tokenizers are stateless; hot reload re-runs tokenization on updated code fragments.
- `SpectreMarkupFormatter` caches the latest markup string keyed by `(language, theme, code hash)` to avoid reprocessing when nothing changed.
- An invalidation signal from `HotReloadRenderPipeline` clears caches when the global theme changes.

## Extensibility
- Consumers register additional languages by providing ColorCode `ILanguage` implementations and calling `ColorCodeLanguageRegistry.Register("language-id", language)`.
- Themes can derive from `SyntaxTheme` or compose existing ones by overriding `GetStyle(scopeName)` to match ColorCode scopes.
- `SpectreMarkupFormatter` exposes an optional `IEnumerable<ISyntaxDecorator>` pipeline that can rewrite markup segments (e.g., inline annotations) before rendering.

## Edge cases and mitigation
- **Tabs vs. spaces**: normalize tabs according to `SyntaxOptions.TabWidth` before rendering to keep gutters aligned.
- **Wide glyphs and Unicode**: rely on Spectre.Console's `CellWidth` helpers and pre-compute display widths of tokens.
- **Long lines**: support optional soft-wrap by splitting tokens to respect the console width; default is horizontal scrolling via panels.
- **Empty or null input**: render a dimmed "(no code)" placeholder while keeping layout stable.
- **Unsupported languages**: fall back to a plain-text formatter with no ColorCode scopes while emitting a warning to diagnostics output.

## Testing strategy
- Add unit tests in `src/RazorConsole.Tests/Syntax/SyntaxHighlighterTests.cs` covering:
   - ColorCode language resolution for C#, Razor, and any registered custom language.
   - Formatter output: ensure `SpectreMarkupFormatter` escapes markup correctly and applies theme styles.
   - Preservation of whitespace and line numbers.
   - Rendering snapshot tests against Spectre.Console's virtual console using markup produced by the formatter.
- Integration tests in the gallery ensure `<SyntaxHighlighter>` renders correctly within panels and hot reload scenarios.

## Tooling and developer experience
- Provide sample usage in the gallery via `SyntaxHighlighterDemo.razor` under `RazorConsole.Gallery/Components`.
- Document configuration in `README.md` once feature ships: language registration, theming, and usage examples.
- Offer `dotnet new` template guidance for consuming apps to enable default theme registration.

## Future enhancements
- Add themes mirroring popular IDE palettes (Dark+, Solarized, OneHalf).
- Support incremental token updates based on diffing to lower allocations for large documents.
- Investigate pluggable grammars via Tree-sitter or Roslyn for richer classification without full integration.
