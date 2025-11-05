# Counter Example

![alt text](../../assets/counter.png)

A simple counter application demonstrating the basics of RazorConsole. This example shows how to:

- Create interactive buttons with focus management
- Handle click events
- Update component state
- Display styled text using Markup
- Layout components using Columns and Rows
- Use Panels and Figlet for visual appeal

## Running the Example

From the repository root:

```bash
dotnet run --project examples/Counter
```

Or from this directory:

```bash
dotnet run
```

## Features Demonstrated

- **Interactive Buttons**: Three buttons (Increment, Decrement, Reset) with keyboard navigation
- **State Management**: Counter value that updates in response to button clicks
- **Styling**: Uses Spectre.Console colors and decorations for visual feedback
- **Layout**: Organized UI using built-in layout components (Rows, Columns, Panel)

## Key Components Used

- `Figlet` - ASCII art title
- `Panel` - Bordered container with title
- `Rows` - Vertical layout
- `Columns` - Horizontal layout
- `TextButton` - Interactive button with focus states
- `Markup` - Styled text with colors and decorations
- `Newline` - Spacing between elements

## Code Structure

- `Counter.razor` - The main component with counter logic
- `Program.cs` - Application entry point
- `Counter.csproj` - Project configuration using Microsoft.NET.Sdk.Razor
