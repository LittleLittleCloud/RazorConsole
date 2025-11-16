### Quick Start Guide

Get up and running with RazorConsole in four straightforward steps.

#### Step 1 · Installation

Install the core package from NuGet:

```shell
dotnet add package RazorConsole.Core
```

#### Step 2 · Project Setup

Enable the Razor SDK so your Razor components compile correctly. Update your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
    </PropertyGroup>
</Project>
```

#### Step 3 · Build Your First Component

Create a `Counter.razor` component:

```razor
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using RazorConsole.Components

<Columns>
    <p>Current count</p>
    <Markup Content="@currentCount.ToString()" 
            Foreground="@Spectre.Console.Color.Green" />
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
```

Wire it up in `Program.cs`:

```csharp
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<Counter>();

IHost host = hostBuilder.Build();
await host.RunAsync();
```

#### Step 4 · Run the App

Build and run your console UI:

```shell
dotnet run
```

![img](https://private-user-images.githubusercontent.com/16876986/497579617-24d8cc11-6428-4886-93c1-873e45b47c74.gif?jwt=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3NjMzMTgwMzUsIm5iZiI6MTc2MzMxNzczNSwicGF0aCI6Ii8xNjg3Njk4Ni80OTc1Nzk2MTctMjRkOGNjMTEtNjQyOC00ODg2LTkzYzEtODczZTQ1YjQ3Yzc0LmdpZj9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNTExMTYlMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjUxMTE2VDE4Mjg1NVomWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTBkMjczNmM2NzIxZWU2ZGMyMjEzMjA2NGM4MzViYTQwYTJkMmVjYjlmODdkNWRhYThkYjY5ZjMyYmYwOWEwYmMmWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0In0.aRSMvUTb6-7gwSexBibN9jMTRZzWfS74Gy9nft_CsvA)

You should see an interactive counter in the terminal. Use **Tab** to traverse focusable elements and **Enter** or **Space** to trigger actions.
