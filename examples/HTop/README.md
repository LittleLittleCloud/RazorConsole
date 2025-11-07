# HTop Example

A system monitoring tool inspired by htop, built with RazorConsole. This example demonstrates real-time system performance tracking with an interactive terminal UI that closely matches the htop interface.

## Features

- **CPU Usage Bars**: Visual bar representation for each CPU core with color-coded segments (blue/green/yellow)
- **Memory and Swap Bars**: Visual representations of memory and swap usage
- **System Information Panel**: Displays tasks count, load average, and system uptime
- **Comprehensive Process Table**: View detailed information about running processes including:
  - PID (Process ID)
  - USER (process owner)
  - PRI (priority)
  - NI (nice value)
  - VIRT (virtual memory)
  - RES (resident memory)
  - S (state: R=Running, S=Sleeping, D=Uninterruptible)
  - CPU% (CPU usage percentage)
  - MEM% (memory usage percentage)
  - TIME+ (CPU time)
  - Command (process name)
- **Interactive Sorting**: Click column headers to sort processes by different criteria
- **Color-Coded Display**: 
  - Green header row like htop
  - Color-coded CPU/memory bars
  - State indicators (Green for Running, White for Sleeping, Red for blocked)
- **Function Key Menu**: htop-style function key bar at the bottom (F1-F10)
- **Auto-refresh**: Updates every 2 seconds to show current system state

## Running the Example

```bash
cd examples/HTop
dotnet run
```

## UI Layout

The interface matches htop's layout:
- **Top section**: CPU bars on the left, system info (Tasks, Load average, Uptime) on the right
- **Middle section**: Scrollable process table with green header row
- **Bottom**: Function key menu bar

## Key Implementation Details

### Components Used

- `Columns` and `Rows`: Layout management for header section
- `SpectreTable`: Structured data display with no borders (matching htop)
- `TextButton`: Interactive column headers with green background
- `Markup`: Styled text with color coding and markup support
- `Newline`: Spacing between sections

### Real-time Updates

The application uses a `Timer` to refresh metrics every 2 seconds:
- CPU usage calculated by tracking process CPU time between updates
- Per-core CPU usage simulated based on overall system activity
- Memory metrics gathered from `Process.WorkingSet64` for each process
- Process state determined from CPU activity and responsiveness

### Visual Elements

CPU and memory bars are built using Spectre.Console markup with color-coded segments:
- Blue/Green/Yellow segments for CPU bars
- Green/Yellow segments for memory bars
- Bar length of 25 characters matching htop style

### Cross-platform Considerations

The example uses standard .NET APIs that work across Windows, Linux, and macOS:
- `Process.GetProcesses()` for process enumeration
- `GC.GetGCMemoryInfo()` for memory statistics
- `Process.TotalProcessorTime` for CPU metrics
- Simplified values for fields not easily accessible cross-platform (priority, nice, swap)

## Keyboard Controls

- **Tab**: Navigate between interactive elements (column header buttons)
- **Enter**: Activate the focused button to change sort order
- **Ctrl+C**: Exit the application

## What You'll Learn

This example demonstrates:
1. Creating htop-like terminal UIs with RazorConsole
2. Building visual progress bars with Spectre.Console markup
3. Implementing real-time updates with timers
4. Using color coding for visual feedback matching system monitoring tools
5. Building complex tables without borders
6. Proper resource cleanup with `IDisposable`
7. System metrics collection using .NET APIs
