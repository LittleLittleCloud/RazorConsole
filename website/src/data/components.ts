export interface ComponentInfo {
  name: string
  description: string
  category: "Layout" | "Input" | "Display" | "Utilities"
  parameters?: Array<{
    name: string
    type: string
    default?: string
    description: string
  }>
  example: string
}

export const components: ComponentInfo[] = [
  {
    name: "Align",
    description: "Wraps child content in an alignment container.",
    category: "Layout",
    parameters: [
      {
        name: "ChildContent",
        type: "RenderFragment?",
        description: "Nested content to align."
      },
      {
        name: "Horizontal",
        type: "HorizontalAlignment",
        default: "Left",
        description: "Horizontal alignment (Left, Center, Right)."
      },
      {
        name: "Vertical",
        type: "VerticalAlignment",
        default: "Top",
        description: "Vertical alignment (Top, Middle, Bottom)."
      },
      {
        name: "Width",
        type: "int?",
        default: "null",
        description: "Fixed width in characters when greater than zero."
      },
      {
        name: "Height",
        type: "int?",
        default: "null",
        description: "Fixed height in rows when greater than zero."
      }
    ],
    example: `<Align Horizontal="Center" Vertical="Middle" Width="40" Height="10">
    <Markup Content="Centered content" />
</Align>`
  },
  {
    name: "Border",
    description: "Creates a bordered panel around its children.",
    category: "Display",
    parameters: [
      {
        name: "ChildContent",
        type: "RenderFragment?",
        description: "Content rendered inside the border."
      },
      {
        name: "BorderColor",
        type: "Color?",
        default: "null",
        description: "Optional border color."
      },
      {
        name: "BoxBorder",
        type: "BoxBorder",
        default: "Rounded",
        description: "Border style (Rounded, Square, Double, etc.)."
      },
      {
        name: "Padding",
        type: "Padding",
        default: "new(0,0,0,0)",
        description: "Inner padding inside the border."
      }
    ],
    example: `<Border BoxBorder="BoxBorder.Rounded" BorderColor="Color.Blue">
    <Markup Content="Content inside border" />
</Border>`
  },
  {
    name: "Columns",
    description: "Flow child renderables in Spectre.Console columns.",
    category: "Layout",
    parameters: [
      {
        name: "ChildContent",
        type: "RenderFragment?",
        description: "Column items."
      },
      {
        name: "Expand",
        type: "bool",
        default: "false",
        description: "When true, forces columns to fill the available width."
      }
    ],
    example: `<Columns>
    <Markup Content="Column 1" />
    <Markup Content="Column 2" />
    <Markup Content="Column 3" />
</Columns>`
  },
  {
    name: "Rows",
    description: "Stacks child renderables vertically.",
    category: "Layout",
    parameters: [
      {
        name: "ChildContent",
        type: "RenderFragment?",
        description: "Row items."
      },
      {
        name: "Expand",
        type: "bool",
        default: "false",
        description: "When true, rows fill the available height."
      }
    ],
    example: `<Rows>
    <Markup Content="Row 1" />
    <Markup Content="Row 2" />
    <Markup Content="Row 3" />
</Rows>`
  },
  {
    name: "Grid",
    description: "Builds a Spectre.Console grid with configurable columns.",
    category: "Layout",
    parameters: [
      {
        name: "ChildContent",
        type: "RenderFragment?",
        description: "Grid rows and cells."
      },
      {
        name: "Columns",
        type: "int",
        default: "2",
        description: "Number of columns in the grid."
      },
      {
        name: "Expand",
        type: "bool",
        default: "false",
        description: "Stretch grid to available width."
      },
      {
        name: "Width",
        type: "int?",
        default: "null",
        description: "Fixed width when greater than zero."
      }
    ],
    example: `<Grid Columns="3">
    <Markup Content="Cell 1" />
    <Markup Content="Cell 2" />
    <Markup Content="Cell 3" />
</Grid>`
  },
  {
    name: "Padder",
    description: "Adds padding around nested content.",
    category: "Layout",
    parameters: [
      {
        name: "ChildContent",
        type: "RenderFragment?",
        description: "Inner content."
      },
      {
        name: "Padding",
        type: "Padding",
        default: "new(0,0,0,0)",
        description: "Padding thickness (left, top, right, bottom)."
      }
    ],
    example: `<Padder Padding="new Padding(2, 1, 2, 1)">
    <Markup Content="Padded content" />
</Padder>`
  },
  {
    name: "TextButton",
    description: "Display clickable text with focus and pressed-state styling.",
    category: "Input",
    parameters: [
      {
        name: "Content",
        type: "string",
        default: "string.Empty",
        description: "Button text to display."
      },
      {
        name: "BackgroundColor",
        type: "Color",
        default: "Color.Default",
        description: "Background color when not focused."
      },
      {
        name: "FocusedColor",
        type: "Color",
        default: "Color.Yellow",
        description: "Background color when focused."
      },
      {
        name: "OnClick",
        type: "EventCallback",
        description: "Event handler for click events."
      }
    ],
    example: `<TextButton Content="Click me"
            OnClick="HandleClick"
            BackgroundColor="Color.Grey"
            FocusedColor="Color.Blue" />`
  },
  {
    name: "TextInput",
    description: "Capture user input with optional masking and change handlers.",
    category: "Input",
    parameters: [
      {
        name: "Value",
        type: "string?",
        description: "Current input value."
      },
      {
        name: "ValueChanged",
        type: "EventCallback<string?>",
        description: "Event raised when value changes."
      },
      {
        name: "Placeholder",
        type: "string",
        default: "\"Enter text...\"",
        description: "Placeholder text when empty."
      },
      {
        name: "IsPassword",
        type: "bool",
        default: "false",
        description: "Mask input characters for passwords."
      }
    ],
    example: `<TextInput Value="@inputValue"
           ValueChanged="@((v) => inputValue = v)"
           Placeholder="Enter your name" />`
  },
  {
    name: "Select",
    description: "Interactive dropdown for choosing a value with keyboard navigation.",
    category: "Input",
    parameters: [
      {
        name: "Options",
        type: "string[]",
        default: "Array.Empty<string>()",
        description: "Available options."
      },
      {
        name: "Value",
        type: "string?",
        default: "null",
        description: "Current selection."
      },
      {
        name: "ValueChanged",
        type: "EventCallback<string?>",
        description: "Raised on selection change."
      },
      {
        name: "Placeholder",
        type: "string",
        default: "\"Select an option\"",
        description: "Placeholder when no selection is set."
      }
    ],
    example: `<Select Options="@options"
        Value="@selectedValue"
        ValueChanged="@((v) => selectedValue = v)"
        Placeholder="Choose an option" />`
  },
  {
    name: "Markup",
    description: "Outputs Spectre markup with styling.",
    category: "Display",
    parameters: [
      {
        name: "Content",
        type: "string",
        description: "Text to render; automatically escaped. Required."
      },
      {
        name: "Foreground",
        type: "Color",
        default: "Style.Plain.Foreground",
        description: "Text color."
      },
      {
        name: "Background",
        type: "Color",
        default: "Style.Plain.Background",
        description: "Background color."
      },
      {
        name: "Decoration",
        type: "Decoration",
        default: "None",
        description: "Styling flags (Bold, Italic, etc.)."
      }
    ],
    example: `<Markup Content="Hello World"
        Foreground="Color.Green"
        Decoration="Decoration.Bold" />`
  },
  {
    name: "Markdown",
    description: "Render markdown string.",
    category: "Display",
    example: `<Markdown Content="@markdownText" />`
  },
  {
    name: "Panel",
    description: "Full-featured Spectre panel wrapper.",
    category: "Display",
    parameters: [
      {
        name: "ChildContent",
        type: "RenderFragment?",
        description: "Panel body content."
      },
      {
        name: "Title",
        type: "string?",
        default: "null",
        description: "Optional panel header text."
      },
      {
        name: "BorderColor",
        type: "Color?",
        default: "null",
        description: "Panel border color."
      },
      {
        name: "Expand",
        type: "bool",
        default: "false",
        description: "Stretch panel to available width."
      }
    ],
    example: `<Panel Title="Information" BorderColor="Color.Blue" Expand="true">
    <Markup Content="Panel content here" />
</Panel>`
  },
  {
    name: "Figlet",
    description: "Renders large ASCII art text using Figlet fonts.",
    category: "Display",
    parameters: [
      {
        name: "Content",
        type: "string",
        default: "string.Empty",
        description: "Text to render."
      },
      {
        name: "Color",
        type: "Color",
        default: "Color.Default",
        description: "Foreground color for the glyphs."
      }
    ],
    example: `<Figlet Content="RazorConsole" Color="Color.Blue" />`
  },
  {
    name: "SyntaxHighlighter",
    description: "Renders highlighted code blocks with SyntaxHighlightingService.",
    category: "Display",
    parameters: [
      {
        name: "Code",
        type: "string",
        default: "string.Empty",
        description: "Source text to highlight."
      },
      {
        name: "Language",
        type: "string?",
        default: "null",
        description: "Language identifier (e.g., 'csharp')."
      },
      {
        name: "ShowLineNumbers",
        type: "bool",
        default: "false",
        description: "Display line numbers when true."
      }
    ],
    example: `<SyntaxHighlighter Language="csharp"
                   Code="@codeSnippet"
                   ShowLineNumbers="true" />`
  },
  {
    name: "Table",
    description: "Turns semantic HTML table markup into a Spectre.Console Table renderable.",
    category: "Display",
    example: `<table class="table" data-expand="true" data-border="Rounded">
    <thead>
        <tr>
            <th data-align="left">Name</th>
            <th data-align="center">Value</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td><Markup Content="Item 1" /></td>
            <td><Markup Content="100" /></td>
        </tr>
    </tbody>
</table>`
  },
  {
    name: "Spinner",
    description: "Shows a Spectre spinner with optional message.",
    category: "Utilities",
    parameters: [
      {
        name: "SpinnerType",
        type: "Spinner",
        default: "Spinner.Known.Dots",
        description: "Spinner instance to render."
      },
      {
        name: "Message",
        type: "string?",
        default: "null",
        description: "Optional message displayed alongside the spinner."
      }
    ],
    example: `<Spinner Message="Loading..." />`
  },
  {
    name: "Newline",
    description: "Emits a single line break. No parameters.",
    category: "Utilities",
    example: `<Newline />`
  }
]
