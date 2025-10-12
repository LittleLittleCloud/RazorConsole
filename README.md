<div align="center">

# 🚀 RazorConsole

[![NuGet Version](https://img.shields.io/nuget/vpre/RazorConsole.Core.svg?style=flat-square&logo=nuget&include_prereleases=true)](https://www.nuget.org/packages/RazorConsole.Core)
[![Component Gallery](https://img.shields.io/nuget/vpre/RazorConsole.Gallery.svg?style=flat-square&logo=nuget&label=gallery&color=purple&include_prereleases=true)](https://www.nuget.org/packages/RazorConsole.Gallery)
[![NuGet Downloads](https://img.shields.io/nuget/dt/RazorConsole.Core.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/RazorConsole.Core)

[![GitHub Release](https://img.shields.io/github/v/release/LittleLittleCloud/RazorConsole?style=flat-square&logo=github)](https://github.com/LittleLittleCloud/RazorConsole/releases)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4?style=flat-square)](https://dotnet.microsoft.com/)

**Build rich, interactive console applications using familiar Razor syntax and the power of Spectre.Console**

</div>

## 🎯 What is RazorConsole?

RazorConsole bridges the gap between modern web UI development and console applications. It lets you create sophisticated terminal interfaces using Razor components, complete with interactive elements, rich styling, and familiar development patterns. Whether you're building CLI tools, admin dashboards, or developer utilities, RazorConsole makes console app development as intuitive as building a web application.

## 📦 Install

```bash
dotnet add package RazorConsole.Core
```

## 🚀 Usage

### Project Setup

RazorConsole requires the Microsoft.NET.Sdk.Razor SDK to compile Razor components. Update your project file (`.csproj`) to use the Razor SDK:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
    <!-- other settings -->
</Project>
```

### Basic Example

Here's a simple counter component to get you started:

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


## ✨ Key Features

### 🧩 **Component-Based Architecture**
Build your console UI using familiar Razor components with full support for data binding, event handling, and component lifecycle methods.

### 🎮 **Interactive Components**
Create engaging user experiences with interactive elements like buttons, text inputs, selectors, and keyboard navigation - all with focus management handled automatically.

### 🎯 **Built-in Component Library**
Get started quickly with 15+ pre-built components covering layout, input, display, and navigation needs:
- **Layout**: `Grid`, `Columns`, `Rows`, `Align`, `Padder`
- **Input**: `TextInput`, `TextButton`, `Select`
- **Display**: `Markup`, `Panel`, `Border`, `Figlet`, `SyntaxHighlighter`, `Table`
- **Utilities**: `Spinner`, `Newline`

For a full list of components and usage details, see the [Built-in Components](#built-in-components) section below.

### ⚡ **Hot Reload Support**
Experience rapid development with built-in hot reload support. See your UI changes instantly without restarting your application.

### 🎪 **Interactive Component Gallery**
Explore all components hands-on with the included interactive gallery tool. Install globally and run `razorconsole-gallery` to see live examples of every component in action.

For more details, see the [Component Gallery](#component-gallery) section below.


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
dotnet tool install --global RazorConsole.Gallery --version 0.0.3-alpha.4657e6
```

After installation, run `razorconsole-gallery` to open the showcase and browse component examples rendered in the console. The gallery includes quick links back to this README for more details.

![Component Gallery](./assets/gallery.png)

## HotReload

RazorConsole supports hotreload via metadata update handler so you can apply UI changes on the fly.

## Development

### Git LFS

This repository uses [Git LFS](https://git-lfs.github.io/) for tracking large media files. If you're contributing or cloning the repository, make sure you have Git LFS installed:

```bash
# Install Git LFS (if not already installed)
git lfs install

# Clone the repository (LFS files will be downloaded automatically)
git clone https://github.com/LittleLittleCloud/RazorConsole.git
```

The following file types are automatically tracked by Git LFS:
- Images: `*.gif`, `*.png`, `*.jpg`, `*.jpeg`
- Videos: `*.mp4`, `*.mov`, `*.avi`
- Archives: `*.zip`, `*.tar.gz`
- Documents: `*.pdf`

## Community & support

- File issues using the GitHub **Issues** tab.

## License

This project is distributed under the MIT License. See [`LICENSE`](LICENSE) for details.
