# LLM Agent TUI - Interface Design

This document describes the user interface layout and design of the LLM Agent TUI application.

## Layout Overview

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│   _     _     __  __      _                    _             │
│  | |   | |   |  \/  |    / \   __ _  ___ _ __ | |_           │
│  | |   | |   | |\/| |   / _ \ / _` |/ _ \ '_ \| __|          │
│  | |___| |___| |  | |  / ___ \ (_| |  __/ | | | |_           │
│  |_____|_____|_|  |_| /_/   \_\__, |\___|_| |_|\__|          │
│                                |___/                          │
│                                                              │
│  AI-Powered Console Chat • Tab to change focus • Enter to   │
│                    submit • Ctrl+C to exit                   │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  Chat                                                        │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ You:                                                 │   │
│  │ Hello! How can you help me?                          │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ AI:                                                  │   │
│  │ Hello! I'm an AI assistant. I can help you with     │   │
│  │ various tasks such as answering questions,           │   │
│  │ providing information, and assisting with coding.    │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ⣾ AI is thinking...                                        │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  Input                                                       │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Type your message here...                                   │
│                                                              │
│  ┌──────┐                                                    │
│  │ Send │                                                    │
│  └──────┘                                                    │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

## Component Breakdown

### Header Section
- **Figlet Banner**: Large ASCII art title "LLM Agent"
- **Instructions**: Centered help text with keyboard shortcuts
  - Tab / Ctrl+Tab to change focus
  - Enter to submit messages
  - Ctrl+C to exit the application

### Chat Panel (Main Display)
- **Border**: Blue colored panel border
- **Message Display**: Scrollable area showing conversation history
  - User messages: Green bordered boxes
  - AI messages: Blue bordered boxes
  - Each message shows the sender (You/AI) in bold
- **Loading Indicator**: Animated spinner when waiting for AI response
  - Shows "AI is thinking..." message

### Input Panel (Bottom Section)
- **Border**: Green colored panel border
- **Text Input**: Single-line input field
  - Placeholder text: "Type your message here..."
  - Focus highlight changes color
- **Send Button**: Blue button to submit messages
  - Can be activated with mouse or Enter key

## Color Scheme

| Element | Color | Purpose |
|---------|-------|---------|
| Header Instructions | Grey58 | Subtle help text |
| User Messages | Green | Indicates outbound messages |
| AI Messages | Blue | Indicates AI responses |
| Chat Panel Border | Blue | Main conversation area |
| Input Panel Border | Green | Input/action area |
| Send Button | Blue/DodgerBlue1 | Primary action |
| Loading Spinner | Default | Activity indicator |

## Interactive Features

1. **Focus Management**
   - Tab key cycles through focusable elements
   - Focused elements are highlighted
   - Currently: TextInput and Send button

2. **Message Submission**
   - Enter key in text input sends message
   - Click Send button
   - Input clears after sending

3. **Conversation Flow**
   - Messages appear in order
   - User message → Loading indicator → AI response
   - Conversation history persists during session

4. **Error Handling**
   - Errors displayed as red text in AI message format
   - User can continue conversation after errors

## Responsive Design

- Panels expand to fill available terminal width
- Message content wraps to fit panel width
- Vertical scrolling for long conversations (handled by terminal)

## Keyboard Shortcuts

- **Tab**: Change focus between input and button
- **Enter**: Submit message (when input is focused)
- **Ctrl+C**: Exit application
- **Ctrl+Tab**: Reverse focus direction

## Architecture Notes

The interface uses RazorConsole components:
- `Figlet`: ASCII art header
- `Align`: Center-aligned content
- `Panel`: Bordered sections
- `Rows` / `Columns`: Layout containers
- `Padder`: Spacing control
- `Border`: Message containers
- `Markup`: Styled text with Spectre.Console markup
- `TextInput`: User input field
- `TextButton`: Interactive button
- `Spinner`: Loading animation

All components are reactive and update in real-time as the application state changes.
