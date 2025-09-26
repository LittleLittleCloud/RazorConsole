dotnet run
# RazorConsole

RazorConsole demonstrates how to render Razor components inside a console application and translate the resulting HTML into rich [Spectre.Console](https://spectreconsole.net/) output. The repository is organised as a small solution with reusable libraries, a gallery app, and automated tests.

## Project layout

```
src/
	RazorConsole.Core/        # Rendering pipeline, Spectre helpers, domain models
	RazorConsole.Components/  # Razor component library consumable by any host
	RazorConsole.Gallery/     # Console showcase that renders the components
	RazorConsole.Tests/       # xUnit tests for the core helpers
design-doc/                 # Architecture notes and future plans
.github/workflows/          # GitHub Actions CI definition
```

Shared MSBuild configuration lives in `Directory.Build.props`, `Directory.Build.targets`, and `Directory.Packages.props`. The SDK version is pinned via `global.json` so the solution behaves consistently across environments.

## Prerequisites

- .NET 8 SDK (the exact version is specified in `global.json`)

## Build and run

```pwsh
dotnet build RazorConsole.sln
dotnet run --project src/RazorConsole.Gallery/RazorConsole.Gallery.csproj
```

The gallery app renders `HelloComponent` inside a Spectre panel and lets you interactively update the greeting name. Press <kbd>Ctrl</kbd>+<kbd>C</kbd> or submit an empty name to exit.

## Tests

```pwsh
dotnet test RazorConsole.sln
```

## Continuous integration

The workflow under `.github/workflows/ci.yml` builds and tests the solution on every push and pull request targeting `main`.
