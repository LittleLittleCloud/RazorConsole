<div align="center">

# RazorConsole
</div>

> Build interactive console applications with .NET Razor components.

## Install

```bash
dotnet add package RazorConsole
```

## Usage
```csharp
// Counter.razor
<p>Current count: @currentCount</p>
<button data-focusable="true" @onclick="IncrementCount">Click me</button>

@code {
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
        
        StateHasChanged();
    }
}

// Program.cs
await AppHost.RunAsync<Counter>();
```
<img src="./assets/counter.gif" alt="Counter component demonstration" style="width: 100%; height: auto;" />
## Component Gallery

Explore the built-in components interactively with the RazorConsole Component Gallery. Install the tool globally and launch it from any terminal:

```bash
dotnet tool install --global RazorConsole.Gallery --version 0.0.1
```

After installation, run `razorconsole-gallery` to open the showcase and browse component examples rendered in the console. The gallery includes quick links back to this README for more details.

## Community & support

- File issues using the GitHub **Issues** tab.

## License

This project is distributed under the MIT License. See [`LICENSE`](LICENSE) for details.
