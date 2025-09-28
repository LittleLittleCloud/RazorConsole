## Virtual DOM bridge design

### Purpose
- Introduce an intermediate virtual DOM (vdom) layer between Razor-generated HTML and console `IRenderable` objects.
- Improve incremental updates by diffing successive HTML trees and emitting minimal console updates.
- Align Razor authoring semantics (HTML) with Spectre.Console rendering primitives while keeping Razor components unchanged.

### Rendering pipeline (target state)
1. **Razor component** renders to an HTML fragment (as today).
2. **HTML parser** converts the fragment into a normalized DOM tree.
3. **Vdom adapter** maps the DOM tree to lightweight vdom nodes (`VNode`).
4. **Diff + patch** compares the new vdom with the previous frame and produces a set of vdom mutations.
5. **Renderable translator** converts mutations into `IRenderable` updates for Spectre.Console.
6. **Console renderer** applies updates to the live display.

> Previous pipeline: `.razor -> HTML -> IRenderable -> console`
>
> New pipeline: `.razor -> HTML -> vdom -> IRenderable -> console`

### Guiding principles
- **Deterministic mapping**: every HTML element used in the console should map to a predictable vdom node type and rendering primitive.
- **Immutable snapshots**: treat vdom trees as persistent snapshots to simplify diffing.
- **Diff first**: render steps operate on diffs rather than entire trees to reduce Spectre.Console churn and blinking.
- **Separation of concerns**: keep Razor authoring, DOM parsing, vdom management, and Spectre rendering in distinct services.

### VNode model
- `VNode`
	- `type`: element, text, component placeholder, etc.
	- `tagName`: lowercase HTML tag (for element nodes).
	- `attributes`: normalized key/value pairs, preserving styles and Spectre metadata.
	- `children`: ordered list of child `VNode`s.
	- `key`: optional stable identifier for keyed diffing (sourced from `@key` Razor directive or `data-key` attributes).
- `VText`
	- Leaf node storing the final text to render.
- `VComponent`
	- For Razor child components that emit HTML lazily; holds component metadata and embedded vdom subtree when realized.

### Diffing algorithm
- Base diff on keyed tree comparison (similar to React reconciliation or the reference implementation in `RazorCliVdom`).
- Compare `type`, `tagName`, and `key` to determine identity.
- Produce a mutation list:
	- `ReplaceNode`, `InsertNode`, `RemoveNode`, `UpdateAttributes`, `UpdateText`.
- Bypass diffing for nodes marked `data-static="true"` or when keys change.
- Support partial re-render: when Razor component re-renders only a subtree, diff against the previous subtree snapshot.

### HTML to vdom adapter
- Parse HTML fragment using existing pipeline (ensure consistent casing & whitespace handling).
- Normalize void elements (`br`, `img`) and custom components.
- Map HTML attributes to Spectre concepts (e.g., `class="panel"` -> `PanelRenderable`).
- Preserve data attributes (`data-*`) for downstream translators.

### Vdom to Spectre translator
- Maintain a registry of handlers: `tagName -> IVirtualElementTranslator`.
- Each translator converts vdom nodes into `IRenderable` (e.g., `<panel>` -> `PanelRenderable`).
- Handle attribute diffs: translators receive `oldNode`, `newNode`, and the mutation type to avoid re-building expensive renderables.
- Provide fallback translator for unsupported tags that renders plain text or logs diagnostics.

### State management
- Store previous vdom tree per live display context (`ConsoleLiveDisplayContext`).
- Integrate with existing hot reload: on file change, clear cached vdom to force a fresh render.
- Expose diagnostics hook to inspect diff results during development (attachable to tests).

### Edge cases
- **Whitespace normalization**: ensure text nodes preserve intentional line breaks while trimming excess whitespace that Spectre would collapse.
- **Spectre limitations**: some HTML constructs (tables, flex layouts) may not map 1:1 to console components; document fallbacks.
- **Streaming updates**: handle large tables/lists via chunked diffs to avoid large console repaints.
- **Keys vs indices**: default to index-based reconciliation when no key is supplied, but warn in diagnostics when element order is unstable.
- **Error resilience**: if HTML parsing fails, emit a diagnostic and fall back to previous renderable to avoid blank screens.

### Integration plan
1. **Create vdom abstractions** (`VNode`, diffing utilities, translators).
2. **Wrap HTML renderer** to produce vdom snapshots.
3. **Inject vdom diff service** into `ConsoleApp` render pipeline.
4. **Update tests**: add unit tests for diff cases and translation mapping; update integration tests to assert minimal Spectre updates.
5. **Telemetry & logging**: capture diff stats (nodes touched, time) for tuning.

### Testing strategy
- Unit tests for diff algorithm covering insert/remove/reorder and attribute changes.
- Golden tests comparing HTML input to expected Spectre renderables.
- Performance regression test simulating frequent updates (e.g., spinner, counter) to ensure no flicker.

### Future considerations
- Cross-project sharing: align vdom utilities with `RazorCliVdom` (reference implementation) to avoid duplication.
- Async component rendering: support streaming partial vdoms when Razor components emit fragments over time.
- Serialization: allow exporting vdom snapshots for debugging or tooling integration.

### Assumptions & references
- Based on prior experiments in `RazorCliVdom` (local reference in `Program.cs`); replicate its diffing approach while adapting naming to this repository.
- HTML input is well-formed; malformed markup will be sanitized before diffing.
