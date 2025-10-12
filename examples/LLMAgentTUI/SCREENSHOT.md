# Screenshot Mockup - LLM Agent TUI

Since we cannot run the actual application without an LLM API key or Ollama instance, this file demonstrates what the interface looks like based on the component structure.

## Initial State (No Messages)

```
 _     _     __  __      _                    _   
| |   | |   |  \/  |    / \   __ _  ___ _ __ | |_ 
| |   | |   | |\/| |   / _ \ / _` |/ _ \ '_ \| __|
| |___| |___| |  | |  / ___ \ (_| |  __/ | | | |_ 
|_____|_____|_|  |_| /_/   \_\__, |\___|_| |_|\__|
                             |___/                 

AI-Powered Console Chat • Tab to change focus • Enter to submit • Ctrl+C to exit

┌─ Chat ─────────────────────────────────────────────────────────────┐
│                                                                    │
│  No messages yet. Type a message below to start chatting.         │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘

┌─ Input ────────────────────────────────────────────────────────────┐
│                                                                    │
│  Type your message here...                                         │
│                                                                    │
│  ┌──────┐                                                          │
│  │ Send │                                                          │
│  └──────┘                                                          │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

## Active Conversation

```
 _     _     __  __      _                    _   
| |   | |   |  \/  |    / \   __ _  ___ _ __ | |_ 
| |   | |   | |\/| |   / _ \ / _` |/ _ \ '_ \| __|
| |___| |___| |  | |  / ___ \ (_| |  __/ | | | |_ 
|_____|_____|_|  |_| /_/   \_\__, |\___|_| |_|\__|
                             |___/                 

AI-Powered Console Chat • Tab to change focus • Enter to submit • Ctrl+C to exit

┌─ Chat ─────────────────────────────────────────────────────────────┐
│                                                                    │
│  ╭──────────────────────────────────────────────────────────────╮ │
│  │ You:                                                         │ │
│  │ Hello! Can you help me write a simple sorting algorithm?    │ │
│  ╰──────────────────────────────────────────────────────────────╯ │
│                                                                    │
│  ╭──────────────────────────────────────────────────────────────╮ │
│  │ AI:                                                          │ │
│  │ Of course! I'd be happy to help. Here's a simple            │ │
│  │ implementation of the bubble sort algorithm in C#:           │ │
│  │                                                              │ │
│  │ public static void BubbleSort(int[] arr) {                   │ │
│  │     for (int i = 0; i < arr.Length - 1; i++) {              │ │
│  │         for (int j = 0; j < arr.Length - i - 1; j++) {      │ │
│  │             if (arr[j] > arr[j + 1]) {                       │ │
│  │                 int temp = arr[j];                           │ │
│  │                 arr[j] = arr[j + 1];                         │ │
│  │                 arr[j + 1] = temp;                           │ │
│  │             }                                                │ │
│  │         }                                                    │ │
│  │     }                                                        │ │
│  │ }                                                            │ │
│  ╰──────────────────────────────────────────────────────────────╯ │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘

┌─ Input ────────────────────────────────────────────────────────────┐
│                                                                    │
│  Can you explain how it works?                                     │
│                                                                    │
│  ┌──────┐                                                          │
│  │ Send │                                                          │
│  └──────┘                                                          │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

## Loading State

```
 _     _     __  __      _                    _   
| |   | |   |  \/  |    / \   __ _  ___ _ __ | |_ 
| |   | |   | |\/| |   / _ \ / _` |/ _ \ '_ \| __|
| |___| |___| |  | |  / ___ \ (_| |  __/ | | | |_ 
|_____|_____|_|  |_| /_/   \_\__, |\___|_| |_|\__|
                             |___/                 

AI-Powered Console Chat • Tab to change focus • Enter to submit • Ctrl+C to exit

┌─ Chat ─────────────────────────────────────────────────────────────┐
│                                                                    │
│  ╭──────────────────────────────────────────────────────────────╮ │
│  │ You:                                                         │ │
│  │ What's the time complexity of this algorithm?                │ │
│  ╰──────────────────────────────────────────────────────────────╯ │
│                                                                    │
│  ⣾ AI is thinking...                                              │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘

┌─ Input ────────────────────────────────────────────────────────────┐
│                                                                    │
│  Type your message here...                                         │
│                                                                    │
│  ┌──────┐                                                          │
│  │ Send │                                                          │
│  └──────┘                                                          │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

## Key Features Demonstrated

1. **Figlet ASCII Art Header**: Large "LLM Agent" title at the top
2. **Color-Coded Messages**: 
   - User messages in green rounded borders
   - AI responses in blue rounded borders
3. **Loading Animation**: Spinner with "AI is thinking..." message
4. **Input Section**: Clean text input with Send button
5. **Responsive Layout**: Panels expand to fill terminal width
6. **Clear Instructions**: Help text shows keyboard shortcuts

## Component Usage

The interface leverages these RazorConsole components:
- **Figlet**: ASCII art banner
- **Align**: Centered help text
- **Panel**: Bordered sections for Chat and Input
- **Border**: Individual message containers
- **Rows/Columns**: Layout management
- **Padder**: Spacing and margins
- **Markup**: Styled text with colors
- **TextInput**: User message entry
- **TextButton**: Send button
- **Spinner**: Loading indicator

All styling follows the design patterns documented in DESIGN.md.
