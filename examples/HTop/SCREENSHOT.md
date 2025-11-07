# HTop Example Screenshot

The HTop example application shows a real-time system monitoring interface modeled after htop:

```
 1 [                         ]   0.0%         Tasks: 179, 442 thr; 0 running
 2 [                         ]   0.0%         Load average: 0.00 0.00 0.00
 3 [                         ]   0.0%         Uptime: 00:00:02
 4 [                         ]   0.0%
Mem [||||                     ] 2657/15995MB
Swp [                         ]    0/8099MB

 PID USER     PRI  NI   VIRT   RES S    CPU%   MEM%      TIME+ Command
   1 user      20   0    22M   13M S     0.0    0.1    0:03.58 systemd
   2 user      20   0      0     0 S     0.0    0.0    0:00.00 kthreadd
4246 user      20   0   433M  192M R    16.5    1.2    0:02.15 dotnet
4306 user      20   0   175M   68M R     3.0    0.4    0:00.45 HTop
1905 user      20   0   108M   33M S     0.2    0.2    1:15.20 provjobd
1950 user      20   0   254M  129M S     0.1    0.8    0:42.15 Runner.Worker
3266 user      20   0   485M  242M S     0.1    1.5    2:18.05 node
   8 user      20   0      0     0 S     0.0    0.0    0:00.06 kworker/0:0
   9 user      20   0      0     0 S     0.0    0.0    0:00.00 kworker/0:0H
  12 user      20   0      0     0 S     0.0    0.0    0:00.00 kworker/R-mm
  13 user      20   0      0     0 S     0.0    0.0    0:00.00 rcu_tasks_rude
  ...

F1Help F2Setup F3Search F4Filter F5Tree F6SortBy F7Nice F8Nice+ F9Kill F10Quit
```

## Features Shown

1. **CPU Bars**: Per-core CPU usage bars (numbered 1-4 for each core)
   - Color-coded segments: blue (low), green (moderate), yellow (high)
   - Percentage display on the right

2. **Memory/Swap Bars**: Visual representation with colored segments
   - Shows used/total in MB format
   - Green for low usage, yellow for higher usage

3. **System Info Panel** (right side):
   - Tasks: total processes, thread count, running processes
   - Load average: 1-minute, 5-minute, 15-minute averages
   - Uptime: system uptime in HH:MM:SS or days format

4. **Process Table**:
   - Green header row (matching htop)
   - Interactive column headers (click to sort)
   - Columns: PID, USER, PRI, NI, VIRT, RES, S, CPU%, MEM%, TIME+, Command
   - Color-coded values:
     - Red for high CPU usage (>50%)
     - Yellow for moderate CPU usage (>20%)
     - Green for running processes
     - Aqua for memory values

5. **Function Key Menu**: htop-style menu bar at bottom
   - F1-F10 keys with green highlighting
   - Matches htop's interface design

6. **No Borders**: Table uses `TableBorder.None` to match htop's clean look

## How it Works

The application uses:
- `Columns` component to split header into left (bars) and right (info) sections
- `Rows` component to stack CPU bars and memory/swap bars
- `SpectreTable` with no borders for the process list
- `TextButton` components with green background for column headers
- `Markup` with Spectre.Console color tags for visual elements
- `Timer` updating every 2 seconds
- CPU tracking by comparing `TotalProcessorTime` between intervals
- Memory info from `Process.WorkingSet64` and `GC.GetGCMemoryInfo()`

## Key Differences from Original htop

Due to .NET API limitations and cross-platform compatibility:
- CPU per-core usage is simulated based on overall system activity
- Load average is estimated from top process CPU usage
- User names default to "user" (not easily accessible cross-platform)
- Priority and Nice values are simplified
- Swap usage is simulated (not easily accessible on all platforms)
