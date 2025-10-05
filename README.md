<div align="center">

# RazorConsole

[![NuGet](https://img.shields.io/nuget/v/RazorConsole.Core.svg)](https://www.nuget.org/packages/RazorConsole.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
</div>

> Build interactive console applications with .NET Razor components.

## Install

```bash
dotnet add package RazorConsole
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

![Image](https://github.com/user-attachments/assets/888dd6c7-1221-4b2e-8232-a03417fc70a1)

## Component Gallery

Explore the built-in components interactively with the RazorConsole Component Gallery. Install the tool globally and launch it from any terminal:

```bash
dotnet tool install --global RazorConsole.Gallery --version 0.0.1
```

After installation, run `razorconsole-gallery` to open the showcase and browse component examples rendered in the console. The gallery includes quick links back to this README for more details.

![Component Gallery](./assets/gallery.png)

## Community & support

- File issues using the GitHub **Issues** tab.

## License

This project is distributed under the MIT License. See [`LICENSE`](LICENSE) for details.
