# Markdown Viewer Example

A glow-like TUI application for viewing markdown files in the console. This example demonstrates how to build an interactive markdown viewer that lets you browse and preview markdown files in your current directory and its subdirectories.

## Running the Example

From the repository root:

```bash
dotnet run --project examples/MarkdownViewer
```

Or from this directory:

```bash
dotnet run
```

## Features Demonstrated

- **Markdown File Discovery**: Automatically finds all markdown files (`.md`, `.markdown`, `.mdown`, `.mkd`) in the current directory
- **Directory Navigation**: Browse subdirectories that contain markdown files
- **Live Preview**: View rendered markdown with syntax highlighting for code blocks
- **File Details**: Display file path, size, and modification date
- **Keyboard Navigation**: Navigate through files and directories using keyboard
- **Split View**: File list on the left, preview on the right (glow-like interface)

## Key Components Used

- `Figlet` - ASCII art title
- `Panel` - Bordered containers with titles for organized layout
- `Markdown` - Renders markdown content with full support for formatting, lists, code blocks, and more
- `Rows` & `Columns` - Layout organization for split-view interface
- `TextButton` - Interactive buttons for file selection
- `Markup` - Styled text with colors for metadata
- `Newline` - Spacing between elements

## Code Structure

- `MarkdownViewer.razor` - Main component with markdown browsing and preview logic
- `Program.cs` - Application entry point
- `MarkdownViewer.csproj` - Project configuration using Microsoft.NET.Sdk.Razor

## Implementation Details

### Markdown File Discovery

The viewer:
- Searches for common markdown extensions: `.md`, `.markdown`, `.mdown`, `.mkd`
- Scans the current directory for markdown files
- Identifies subdirectories containing markdown files
- Sorts files alphabetically for easy browsing

### Split-View Interface

Similar to glow's interface:
- **Left Panel**: List of markdown files with clickable buttons
- **Right Panel**: Live preview of the selected markdown file
- **Bottom Panel**: File metadata (path, size, last modified)

### Markdown Rendering

The example uses RazorConsole's built-in Markdown component which:
- Parses markdown using Markdig
- Renders formatted text with styling
- Highlights code blocks with syntax highlighting
- Supports headings, lists, quotes, and more

### Navigation Features

- **Parent Directory Access**: Navigate up the directory tree
- **Subdirectory Navigation**: Click on directory entries to explore
- **File Selection**: Click on any markdown file to view its content
- **Visual Feedback**: Selected file is highlighted in the list

## Supported Markdown Features

The viewer supports all standard markdown features:
- Headings (H1-H6)
- Bold and italic text
- Code blocks with syntax highlighting
- Inline code
- Bullet and numbered lists
- Blockquotes
- Horizontal rules
- And more!

## Technologies Used

- **RazorConsole.Core** - Console UI framework with Razor components
- **Spectre.Console** - Rich console rendering
- **System.IO** - File system operations
- **Microsoft.Extensions.Hosting** - Application hosting
- **Markdig** - Markdown parsing (via RazorConsole.Core)
