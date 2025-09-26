## RazorConsole MVC Adaptation

The goal is to layer the existing Razor-based console renderer behind a light-weight Model-View-Controller facade so we can reason about screen states, user input, and navigation flows using the familiar MVC mental model.

### High-Level Mapping

- **Views** stay as `.razor` files inside `Components/`. They focus on markup + simple UI logic and continue to render via `RazorComponentRenderer` / Spectre Console.
- **Models** are POCOs under `Models/`, representing the state required to render a screen (immutable snapshots where possible).
- **Controllers** become new classes under `Controllers/`. Each controller coordinates a model instance, handles input commands, invokes domain services, and chooses which Razor component to render.
- **Services** (domain logic, storage, HTTP calls, etc.) live under `Services/` and remain UI-agnostic. Controllers depend on these via DI.

```
┌────────┐    bind model     ┌────────────┐    render    ┌──────────────┐
│ Model  │ ───────────────▶ │ Controller │ ───────────▶ │ Razor View   │
└────────┘                  │ (Console)  │              │ (.razor)      │
		 ▲                      └────────────┘              └────▲──────────┘
		 │                           │                             │
		 │   update model via        ▼      convert to Spectre     │
		 │   domain services   ┌───────────────┐      markup        │
		 └────────────────────▶│ Core Services │◀───────────────────┘
														└───────────────┘
```

### Controller Contract

Create a common base (e.g., `ConsoleController`) to standardize controller responsibilities:

1. **Model lifecycle**: expose a `Task<ModelSnapshot>` that surfaces the view state built from services.
2. **Render**: return `Task<string>` or a wrapper (e.g., `ConsoleViewResult`) that wraps the rendered markup produced by `RazorComponentRenderer`.
3. **Input handling**: accept a `ConsoleInputContext` (key strokes, form submissions, command strings) and mutate the domain state before re-rendering.
4. **Navigation**: emit `NavigationIntent` values (`Stay`, `RouteTo("/users")`, `Exit`) that a higher-level dispatcher can interpret.

Controllers should be thin orchestrators, delegating to services for any stateful or business behavior. They should never directly touch Spectre APIs; rendering stays encapsulated inside `SpectrePanelFactory` and the Razor views.

### Library vs. Application Responsibilities

| Area | Provided by `RazorConsole.Core` (library) | Provided by consuming app (e.g., `RazorConsole.Gallery`) |
| --- | --- | --- |
| **Infrastructure** | `RazorComponentRenderer`, `SpectrePanelFactory`, `ConsoleNavigationManager`, component activator, base DI registration helpers, base `ConsoleController`, `ConsoleViewResult`, `ConsoleInputContext`, `NavigationIntent`, dispatcher host skeleton. | Application bootstrapping (`Program.cs`), Spectre console loop, controller resolution strategy, wiring of domain services and controllers into DI. |
| **Views** | Shared building blocks (`Border.razor`, layout components, form helpers) and reusable UI primitives. | Concrete views per scenario (e.g., `HomeView.razor`, feature-specific components). |
| **Models** | Optional abstractions or interfaces for common patterns (e.g., `IConsoleModel`, paging filter records). | Concrete models describing app state (`HomeModel`, `OrderSummaryModel`). |
| **Controllers** | Base classes, contracts, and generic helpers for input handling and rendering orchestration. | Feature controllers implementing behavior (`HomeController`, `OrdersController`), navigation decisions, validation, domain coordination. |
| **Services** | Cross-cutting helpers that are UI-facing (e.g., formatting, console theming) and interfaces for host-level interactions. | Domain services (data access, APIs, business logic), plus overrides for any optional services exposed by the library. |
| **Testing Utilities** | Optional test harnesses or fakes to help simulate rendering/input. | Feature tests asserting model creation, navigation intents, and component markup for the specific app. |

> Guiding principle: the library owns the framework scaffolding and reusable UI primitives, while the application supplies domain-specific models, controllers, and views that express actual business flows.

### Project Structure Updates

```
src/RazorConsole.Core/
	Components/              # shared UI primitives (e.g., Border.razor)
	Controllers/             # base controller abstractions and helpers
	Models/                  # shared model contracts (optional)
	Rendering/               # rendering pipeline (already present)
	Services/                # shared infrastructure services (optional)

src/<AppProject>/
	Controllers/             # app-specific controllers (e.g., HomeController)
	Models/                  # app-specific models (e.g., HomeModel)
	Services/                # domain services (e.g., GreetingService)
	Components/ or Views/    # app-specific Razor components (e.g., HomeView.razor)
	Program.cs               # bootstrapping + DI wiring
```

> Keep reusable Razor components in the core library; place feature-specific components in the consuming project. Choose `Components/` or `Views/` naming per Razor conventions.

### Data Flow Sequence

1. Entry point resolves a controller via DI and calls `ExecuteAsync`.
2. Controller gathers data through services, builds a model, and passes it as `RenderParameters` to the Razor component.
3. `RazorComponentRenderer` produces HTML, which `SpectrePanelFactory` turns into console markup.
4. Console shell displays the markup and collects user intent (commands, form inputs).
5. Controller receives user input, updates domain state, and loops back to step 2 until a navigation intent ends the flow.

### Implementation Guidelines

- **Models**: favor immutable records; include validation or computed props as needed. Keep them serialization-ready if you plan to persist wizard states.
- **Controllers**: inject `RazorComponentRenderer`, `ConsoleNavigationManager`, and required services. Expose async methods to support background data fetches.
- **Views**: keep logic minimal—trigger events via bound callbacks that call back into the controller (through event handlers passed as parameters).
- **DI Registration**: extend the existing service registrations with `services.AddTransient<HomeController>();` etc. The root program orchestrator can resolve controllers by name or route key.
- **Testing**: unit-test controllers by mocking services and asserting on emitted models + navigation intents; component tests render `.razor` files with Bunit to validate markup from given models.

### Edge Considerations

- **Long-running operations**: controllers should surface loading states in their models so views can render progress indicators.
- **Navigation unsupported**: `ConsoleNavigationManager` is a no-op; controllers should emit intents the host can interpret instead of calling `NavigationManager` directly.
- **Input validation**: validate within controllers or domain services and add error messages to the model; Razor views read the messages and display them.
- **State persistence**: if flows require multi-step wizards, store the current model snapshot in the controller between renders or move to scoped services.

### Next Steps

- Scaffold a sample `HomeController` + `HomeView.razor` + `HomeModel` trio to prove the pattern.
- Introduce a simple dispatcher (e.g., `ConsoleMvcHost`) responsible for selecting controllers based on routes/commands.
- Expand tests in `RazorConsole.Tests` to cover controller logic and markup rendering.
