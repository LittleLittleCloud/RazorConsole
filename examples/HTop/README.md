# HTop Example

A system monitoring tool inspired by htop, built with RazorConsole. This example demonstrates real-time system performance tracking with an interactive terminal UI.

## Features

- **Real-time System Metrics**: Monitor CPU usage, memory consumption, process count, and system uptime
- **Process List**: View detailed information about running processes including:
  - Process ID (PID)
  - Process name
  - CPU usage percentage
  - Memory consumption in MB
  - Process status (Running/Not Responding)
- **Interactive Sorting**: Click column headers to sort processes by different criteria:
  - PID
  - Process Name
  - CPU Usage
  - Memory Usage
  - Status
- **Color-Coded Display**: Visual indicators for resource usage levels:
  - Green: Low usage (healthy)
  - Yellow: Moderate usage (warning)
  - Red: High usage (critical)
- **Auto-refresh**: Updates every 2 seconds to show current system state

## Running the Example

```bash
cd examples/HTop
dotnet run
```

## Key Implementation Details

### Components Used

- `Figlet`: Large title text
- `Panel`: Organized sections for system overview and process list
- `Rows` and `Columns`: Layout management
- `SpectreTable`: Structured data display with headers and rows
- `TextButton`: Interactive column headers for sorting
- `Markup`: Styled text with color coding

### Real-time Updates

The application uses a `Timer` to refresh metrics every 2 seconds:
- CPU usage is calculated by tracking process CPU time between updates
- Memory metrics are gathered from `Process.WorkingSet64` for each process
- Process status is determined from the `Process.Responding` property

### State Management

- Implements `IDisposable` to properly clean up the timer
- Uses `StateHasChanged()` to trigger UI updates when metrics refresh
- Maintains previous CPU readings to calculate usage percentages

### Cross-platform Considerations

The example uses standard .NET APIs that work across Windows, Linux, and macOS:
- `Process.GetProcesses()` for process enumeration
- `GC.GetGCMemoryInfo()` for memory statistics
- `Process.TotalProcessorTime` for CPU metrics

## Keyboard Controls

- **Tab**: Navigate between interactive elements (column header buttons)
- **Enter**: Activate the focused button to change sort order
- **Ctrl+C**: Exit the application

## What You'll Learn

This example demonstrates:
1. Creating data-driven UIs with RazorConsole
2. Implementing real-time updates with timers
3. Using color coding for visual feedback
4. Building interactive tables with sortable columns
5. Proper resource cleanup with `IDisposable`
6. System metrics collection using .NET APIs
