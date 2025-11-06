# HTop Example Screenshot

The HTop example application shows a real-time system monitoring interface:

```
                         _   _   _____                    
                        | | | | |_   _|   ___    _ __    
                        | |_| |   | |    / _ \  | '_ \   
                        |  _  |   | |   | (_) | | |_) |  
                        |_| |_|   |_|    \___/  | .__/   
                                                 |_|      
              __  __                   _   _              
             |  \/  |   ___    _ __   (_) | |_    ___    _ __   
             | |\/| |  / _ \  | '_ \  | | | __|  / _ \  | '__|  
             | |  | | | (_) | | | | | | | | |_  | (_) | | |     
             |_|  |_|  \___/  |_| |_| |_|  \__|  \___/  |_|     
                                                                
 ╭─System Overview──────────────────────────────────────────────╮
 │ CPU Usage: 2.0%                                              │
 │ Memory: 2,792 MB / 15,995 MB (17.5%)                         │
 │ Processes: 172                                               │
 │ Uptime: 00:00:02                                             │
 ╰──────────────────────────────────────────────────────────────╯
 
 ╭─Top Processes (Sorted by CPU)────────────────────────────────╮
 │ ╭───────┬──────────────────┬────────┬──────────┬─────────╮  │
 │ │   PID │ Process Name     │  CPU % │ Mem (MB) │ Status  │  │
 │ ├───────┼──────────────────┼────────┼──────────┼─────────┤  │
 │ │  4246 │ dotnet           │   16.5 │    192.2 │ Running │  │
 │ │  4306 │ HTop             │    3.0 │     68.4 │ Running │  │
 │ │  1905 │ provjobd         │    0.2 │     33.6 │ Running │  │
 │ │  1950 │ Runner.Worker    │    0.1 │    129.3 │ Running │  │
 │ │  3266 │ node             │    0.1 │    242.2 │ Running │  │
 │ │     1 │ systemd          │    0.0 │     13.4 │ Running │  │
 │ │     2 │ kthreadd         │    0.0 │      0.0 │ Running │  │
 │ │   ... │ ...              │    ... │      ... │ ...     │  │
 │ ╰───────┴──────────────────┴────────┴──────────┴─────────╯  │
 ╰──────────────────────────────────────────────────────────────╯
 
 Click column headers to sort • Updates every 2 seconds • Press Ctrl+C to exit
```

## Features Shown

1. **Figlet Title**: Large ASCII art title "HTop Monitor"
2. **System Overview Panel**: 
   - CPU usage with color coding (green for low, yellow for moderate, red for high)
   - Memory usage showing used/total and percentage
   - Total process count
   - System uptime counter

3. **Process Table**:
   - Interactive column headers (PID, Process Name, CPU %, Memory, Status)
   - Sorted by CPU usage (default)
   - Top 20 processes displayed
   - Color-coded values based on resource usage
   - Process status indicator (Running/Not Responding)

4. **Real-time Updates**: The display automatically refreshes every 2 seconds

5. **Keyboard Navigation**: Tab to navigate between column header buttons, Enter to change sort order

## How it Works

The application uses:
- `Timer` to trigger updates every 2 seconds
- `Process.GetProcesses()` to enumerate all running processes
- CPU tracking by comparing `TotalProcessorTime` between updates
- Memory info from `Process.WorkingSet64` for each process
- Color coding based on thresholds for visual feedback
- `IDisposable` implementation for proper timer cleanup
