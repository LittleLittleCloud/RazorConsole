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

## Community & support

- File issues using the GitHub **Issues** tab.
- Use GitHub Discussions (if enabled) for open-ended questions.
- Follow the guidance in `.github/copilot-instructions.md` for project-specific GitHub Copilot prompts and context.

## License

This project is distributed under the MIT License. See [`LICENSE`](LICENSE) for details.
