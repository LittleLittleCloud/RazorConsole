<div align="center">

# RazorConsole

[![NuGet](https://img.shields.io/nuget/v/RazorConsole.Core.svg)](https://www.nuget.org/packages/RazorConsole.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
</div>

> Build interactive console applications with .NET Razor and Spectre.Console

## Install

```bash
dotnet add package RazorConsole.Core
```

## Usage
```csharp
// Counter.razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using RazorConsole.Components

<Columns>
    <p>Current count</p>
    <Markup Content="@currentCount.ToString()" Foreground="@Spectre.Console.Color.Green" />
</Columns>
<TextButton Content="Click me"
            OnClick="IncrementCount"
            BackgroundColor="@Spectre.Console.Color.Grey"
            FocusedColor="@Spectre.Console.Color.Blue" />


@code {
    private int currentCount = 0;
    private void IncrementCount()
    {
        currentCount++;
    }
}

// Program.cs
await AppHost.RunAsync<Counter>();
```

![Image](https://github.com/user-attachments/assets/24d8cc11-6428-4886-93c1-873e45b47c74)

## Built-in components

RazorConsole ships with a catalog of ready-to-use components that wrap Spectre.Console constructs:

- `Align` – position child content horizontally and vertically within a fixed box.
- `Border` – draw Spectre borders with customizable style, color, and padding.
- `Columns` – arrange items side-by-side, optionally stretching to fill the console width.
- `Figlet` – render big ASCII art text using FIGlet fonts.
- `Grid` – build multi-row, multi-column layouts with precise cell control.
- `Markup` – emit styled text with Spectre markup tags.
- `Newline` – insert intentional spacing between renderables.
- `Padder` – add outer padding around child content without altering the child itself.
- `Panel` – frame content inside a titled container with border and padding options.
- `Rows` – stack child content vertically with optional expansion behavior.
- `Select` – present a focusable option list with keyboard navigation.
- `Spinner` – show animated progress indicators using Spectre spinner presets.
- `SyntaxHighlighter` – colorize code snippets using ColorCode themes.
- `TextButton` – display clickable text with focus and pressed-state styling.
- `TextInput` – capture user input with optional masking and change handlers.

See [`design-doc/builtin-components.md`](design-doc/builtin-components.md) for the full reference, including parameters and customization tips.

## Component Gallery

Explore the built-in components interactively with the RazorConsole Component Gallery. Install the tool globally and launch it from any terminal:

```bash
dotnet tool install --global RazorConsole.Gallery --version 0.0.2-alpha.181b79
```

After installation, run `razorconsole-gallery` to open the showcase and browse component examples rendered in the console. The gallery includes quick links back to this README for more details.

![Component Gallery](./assets/gallery.png)

## HotReload

RazorConsole supports hotreload via metadata update handler so you can apply UI changes on the fly.


## Community & support

- File issues using the GitHub **Issues** tab.

## License

This project is distributed under the MIT License. See [`LICENSE`](LICENSE) for details.
