# ConsoleApp design

## Goals
- Provide a single entry point for rendering a Razor component (`TComponent`) to the console using Spectre.Console primitives.
- Hide the boilerplate currently required to wire up `HtmlRenderer`, component activation, and RazorConsole-specific services.
- Offer light customization hooks (service registration, console output behavior, render parameters).

## Non-goals
- Replacing the controller pattern used for complex interaction loops (those should continue to derive from `ConsoleController`).
- Implementing routing, navigation stacks, or CLI argument parsing.
- Full dependency injection host abstraction (e.g., no opinionated logging or configuration pipelines beyond what is needed to render components).

## Proposed API

```csharp
public static class ConsoleApp
{
    public static ConsoleApp<TComponent> Create<TComponent>(Action<ConsoleAppBuilder>? configure = null)
        where TComponent : IComponent;

    public static Task RunAsync<TComponent>(object? parameters = null, Action<ConsoleAppBuilder>? configure = null, CancellationToken cancellationToken = default)
        where TComponent : IComponent;
}

public sealed class ConsoleApp<TComponent> where TComponent : IComponent
{
    Task<ConsoleViewResult> RenderAsync(object? parameters = null, CancellationToken cancellationToken = default);
    Task RunAsync(object? parameters = null, CancellationToken cancellationToken = default);
}

public sealed class ConsoleAppBuilder
{
    IServiceCollection Services { get; }
    ConsoleAppOptions Options { get; }

    ConsoleAppBuilder ConfigureServices(Action<IServiceCollection> configure);
    ConsoleAppBuilder Configure(Action<ConsoleAppOptions> configure);
}

public sealed class ConsoleAppOptions
{
    bool AutoClearConsole { get; set; } = true;
    Func<ConsoleViewResult, CancellationToken, Task>? AfterRenderAsync { get; set; }
        = (view, _) => { AnsiConsole.Write(view.Renderable); return Task.CompletedTask; };
}
```

## Behavior Overview
1. `ConsoleApp.Create<TComponent>` initialises a `ConsoleAppBuilder` with default services and options.
2. Default services register:
   - `IComponentActivator` → `ServiceProviderComponentActivator`
   - `NavigationManager` → `ConsoleNavigationManager`
   - `HtmlRenderer`
   - `RazorComponentRenderer`
   - `ConsoleNavigationManager` dependencies
3. Builder allows callers to extend `IServiceCollection` before the provider is created.
4. `ConsoleApp<TComponent>` lazily builds a `ServiceProvider` (scoped for the lifetime of the app) and exposes:
   - `RenderAsync`: renders Razor component, converts HTML via `SpectreRenderableFactory`, returns `ConsoleViewResult`. Throws if conversion fails.
   - `RunAsync`: clears console if `AutoClearConsole` is `true`, calls `RenderAsync`, then invokes `AfterRenderAsync`. When `AfterRenderAsync` is `null`, output is written with `AnsiConsole.Write`.
5. Both `RenderAsync` and `RunAsync` support optional anonymous object or dictionary parameters that pass through to `RazorComponentRenderer`.
6. Resources are disposed when the `ConsoleApp<TComponent>` instance is disposed. `RunAsync` overloads will use `await using` to ensure scope disposal.

## Extensibility
- Callers can register additional services (e.g., `ILoggerFactory`, application state, custom converters) through `configure`.
- Options delegate can change console behaviors (suppress auto-clear, replace `AfterRenderAsync`).
- MEF discovery for `IRenderableConverter`s continues to work because assemblies are already loaded; documentation will note that assemblies containing converters must be loaded before calling `RunAsync`.
- Future work: support chaining multiple renders or injecting a controller to produce `ConsoleViewResult`.

## Error Handling
- Throw `InvalidOperationException` if the Razor render returns empty HTML or fails conversion.
- Bubble up exceptions from `RazorComponentRenderer` or the options callback (no swallowing).
- Ensure cancellation tokens propagate to the renderer dispatcher invocation.

## Usage Example

```csharp
await ConsoleApp.RunAsync<HomeView>(new { UserName = "Console" });

// with customization
await ConsoleApp.RunAsync<HomeView>(configure: builder =>
{
    builder.ConfigureServices(services =>
    {
        services.AddSingleton<GreetingService>();
    });

    builder.Configure(options =>
    {
        options.AutoClearConsole = false;
        options.AfterRenderAsync = async (view, _) =>
        {
            AnsiConsole.Write(view.Renderable);
            await Task.Delay(2000);
        };
    });
});
```

## Open Questions
- Should we integrate `ConsoleController` execution into this API (e.g., a `RunControllerAsync<TController>` helper) or keep it separate?
No, keep separate, in the future we might deprecate `ConsoleController` in favor of a more flexible pattern.
- Do we need explicit hooks to load additional assemblies for MEF before rendering, or is documenting assembly loading sufficient?
Not for now. Document assembly loading is sufficient.

- Should `ConsoleApp<TComponent>` expose `IServiceProvider` for advanced scenarios, or keep it private to avoid leak-prone patterns?
Yes.
