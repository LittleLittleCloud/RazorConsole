# Razor Console Demo

This sample shows how to render a Razor component (`.razor`) inside a .NET console app and display the output using [Spectre.Console](https://spectreconsole.net/). The component is re-rendered once a second so you can see live updates such as the current time.

## Prerequisites
- .NET 8 SDK or newer

## Run the demo
```pwsh
cd c:\Users\xiaoyuz\source\repos\RazorConsole
dotnet run
```
Press <kbd>Ctrl</kbd>+<kbd>C</kbd> to stop the loop.

## How it works
- `Components/HelloComponent.razor` constructs Spectre.Console markup from a `GreetingModel`.
- `Rendering/RazorComponentRenderer` uses the Blazor `HtmlRenderer` to materialize a component into a markup string.
- `Program.cs` wires a basic DI container, renders the component once per second, clears the terminal, and writes the markup inside a Spectre panel.

Feel free to tweak the component, add more data to `GreetingModel`, or swap in different Spectre widgets for richer layouts.
