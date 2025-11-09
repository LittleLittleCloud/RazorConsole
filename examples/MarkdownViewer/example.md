# RazorConsole Markdown Viewer

Welcome to the **RazorConsole Markdown Viewer**! This is a _glow-like_ TUI application that lets you browse and preview markdown files right in your terminal.

## What is Glow?

[Glow](https://github.com/charmbracelet/glow) is a popular terminal markdown reader created by Charm. This viewer is inspired by its clean, split-view interface.

## Key Features

### 📁 File Discovery
- Automatically finds all markdown files in the current directory
- Supports multiple extensions: `.md`, `.markdown`, `.mdown`, `.mkd`
- Browse subdirectories with markdown content

### 👁️ Live Preview
- Real-time markdown rendering
- Syntax highlighting for code blocks
- Beautiful formatting for all markdown elements

### ⌨️ Keyboard Navigation
- Use `Tab` to navigate between files
- Press `Enter` to select and preview a file
- Press `Ctrl+C` to exit

## Code Examples

### C# Example

```csharp
using System;
using System.IO;

public class MarkdownReader
{
    public string ReadFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Markdown file not found", path);
        }
        
        return File.ReadAllText(path);
    }
}
```

### Python Example

```python
def read_markdown_file(path):
    """Read and return the contents of a markdown file."""
    try:
        with open(path, 'r', encoding='utf-8') as file:
            return file.read()
    except FileNotFoundError:
        print(f"Error: File {path} not found")
        return None
```

### JavaScript Example

```javascript
const fs = require('fs').promises;

async function readMarkdownFile(path) {
    try {
        const content = await fs.readFile(path, 'utf-8');
        return content;
    } catch (error) {
        console.error(`Error reading file: ${error.message}`);
        return null;
    }
}
```

## Markdown Elements

### Lists

#### Unordered Lists
- First item
  - Nested item 1
  - Nested item 2
- Second item
- Third item

#### Ordered Lists
1. First step
2. Second step
   1. Sub-step 2.1
   2. Sub-step 2.2
3. Third step

### Tables

| Feature | Status | Priority |
|---------|--------|----------|
| File browsing | ✅ Complete | High |
| Live preview | ✅ Complete | High |
| Syntax highlighting | ✅ Complete | Medium |
| Search | ⏳ Planned | Low |

### Blockquotes

> "The only way to do great work is to love what you do."
> 
> - Steve Jobs

> **Note:** This viewer supports nested blockquotes and rich formatting!

### Horizontal Rules

---

## Technologies Used

This markdown viewer is built with:

- **RazorConsole.Core** - Console UI framework
- **Spectre.Console** - Rich terminal rendering
- **Markdig** - Markdown parsing
- **ColorCode** - Syntax highlighting

---

## Try It Out!

1. Navigate to any directory with markdown files
2. Run `dotnet run` from the MarkdownViewer example directory
3. Use keyboard navigation to browse and preview files

Happy markdown viewing! 🎉
