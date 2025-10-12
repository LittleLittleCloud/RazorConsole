# Screenshot Mockup - LLM Agent TUI

Since we cannot run the actual application without an LLM API key or Ollama instance, this file demonstrates what the interface looks like based on the component structure.

## Initial State (No Messages)

```
  ____ _           _   ____        _   
 / ___| |__   __ _| |_| __ )  ___ | |_ 
| |   | '_ \ / _` | __|  _ \ / _ \| __|
| |___| | | | (_| | |_| |_) | (_) | |_ 
 \____|_| |_|\__,_|\__|____/ \___/ \__|

AI-Powered Console ChatBot • Tab to change focus • Enter to submit • Ctrl+C to exit

No messages yet. Type a message below to start chatting.


Type your message here...

┌──────┐
│ Send │
└──────┘
```

## Active Conversation

```
  ____ _           _   ____        _   
 / ___| |__   __ _| |_| __ )  ___ | |_ 
| |   | '_ \ / _` | __|  _ \ / _ \| __|
| |___| | | | (_| | |_| |_) | (_) | |_ 
 \____|_| |_|\__,_|\__|____/ \___/ \__|

AI-Powered Console ChatBot • Tab to change focus • Enter to submit • Ctrl+C to exit


You
Hello! Can you help me write a simple sorting algorithm?

Bot
Of course! I'd be happy to help. Here's a simple implementation of 
the bubble sort algorithm in C#:

public static void BubbleSort(int[] arr) {
    for (int i = 0; i < arr.Length - 1; i++) {
        for (int j = 0; j < arr.Length - i - 1; j++) {
            if (arr[j] > arr[j + 1]) {
                int temp = arr[j];
                arr[j] = arr[j + 1];
                arr[j + 1] = temp;
            }
        }
    }
}


Can you explain how it works?

┌──────┐
│ Send │
└──────┘
```

## Loading State

```
  ____ _           _   ____        _   
 / ___| |__   __ _| |_| __ )  ___ | |_ 
| |   | '_ \ / _` | __|  _ \ / _ \| __|
| |___| | | | (_| | |_| |_) | (_) | |_ 
 \____|_| |_|\__,_|\__|____/ \___/ \__|

AI-Powered Console ChatBot • Tab to change focus • Enter to submit • Ctrl+C to exit


You
What's the time complexity of this algorithm?

⣾ AI is thinking...


Type your message here...

┌──────┐
│ Send │
└──────┘
```

## Key Features Demonstrated

1. **Figlet ASCII Art Header**: Large "ChatBot" title at the top
2. **Color-Coded Messages**: 
   - User messages labeled "You" in green
   - Bot responses labeled "Bot" in blue
   - Message content in grey for user, default color for bot
3. **Loading Animation**: Spinner with "AI is thinking..." message in italic grey
4. **Clean Layout**: Simplified interface without borders or panels
5. **Input Section**: Text input with Send button below messages
6. **Clear Instructions**: Help text shows keyboard shortcuts

## Component Usage

The interface leverages these RazorConsole components:
- **Figlet**: ASCII art banner for "ChatBot" title
- **Align**: Centered help text
- **Markup**: Color-coded message labels and content
- **Rows/Columns**: Layout management
- **Padder**: Spacing between messages and sections
- **TextInput**: User message entry field with expand=true
- **TextButton**: Send button with blue/DodgerBlue1 colors
- **Spinner**: Loading indicator with italic grey text

The design features a clean, minimalist approach without heavy borders, making it easier to read chat conversations.
