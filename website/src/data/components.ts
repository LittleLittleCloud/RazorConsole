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
  // paths to example files
  // the files are located in razor-console/src/RazorConsole.Website/Components/
  // e.g., ["Align_1.razor"]
  examples: string[]
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
    examples: ["Align_1.razor"]
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
    examples: ["Border_1.razor"]
  },
{
    name: "BarChart",
    description: "Renders a horizontal bar chart with optional label, colors and value display.",
    category: "Display",
    parameters: [
        {
            name: "BarChartItems",
            type: "List<IBarChartItem>",
            description: "The data items to display. Each item must have Label and Value, Color is optional. Required."
        },
        {
            name: "Width",
            type: "int?",
            default: "null",
            description: "Chart width in characters. If null = full available console width."
        },
        {
            name: "Label",
            type: "string?",
            default: "null",
            description: "Optional title displayed above the chart."
        },
        {
            name: "LabelForeground",
            type: "Color",
            default: "Style.Plain.Foreground",
            description: "Text color of the chart label."
        },
        {
            name: "LabelBackground",
            type: "Color",
            default: "Style.Plain.Background",
            description: "Background color of the chart label."
        },
        {
            name: "LabelDecoration",
            type: "Decoration",
            default: "Decoration.None",
            description: "Text decoration for the label (Bold, Italic, Underline, etc.)."
        },
        {
            name: "LabelAlignment",
            type: "Justify?",
            default: "null",
            description: "Alignment of the label: Left, Center or Right."
        },
        {
            name: "MaxValue",
            type: "double?",
            default: "null",
            description: "Fixed maximum value for scaling (useful for 0–100% progress-style charts)."
        },
        {
            name: "ShowValues",
            type: "bool",
            default: "false",
            description: "If true, displays the numeric value next to each bar."
        },
        {
            name: "Culture",
            type: "CultureInfo",
            default: "CultureInfo.CurrentCulture",
            description: "Culture used to format numbers."
        },
    ],
    examples: ["BarChart_1.razor"]
},
{
    name: "BreakdownChart",
    description: "Renders a colorful breakdown (pie-style) chart showing proportions with optional legend and values.",
    category: "Display",
    parameters: [
        {
            name: "BreakdownChartItems",
            type: "List<IBreakdownChartItem>",
            description: "The data items to display. Each item must have Label, Value and Color. Required."
        },
        {
            name: "Compact",
            type: "bool",
            default: "false",
            description: "If true, renders the chart and tags in compact mode with reduced spacing."
        },
        {
            name: "Culture",
            type: "CultureInfo",
            default: "CultureInfo.CurrentCulture",
            description: "Culture used to format numbers and percentages."
        },
        {
            name: "Expand",
            type: "bool",
            default: "false",
            description: "If true, the chart expands to fill all available horizontal space."
        },
        {
            name: "Width",
            type: "int?",
            default: "null",
            description: "Fixed width in characters. If null, width is calculated automatically."
        },
        {
            name: "ShowTags",
            type: "bool",
            default: "false",
            description: "If true, displays a colored legend (tags) below the chart."
        },
        {
            name: "ShowTagValues",
            type: "bool",
            default: "false",
            description: "If true, shows absolute values next to each tag (e.g. 3,200)."
        },
        {
            name: "ShowTagValuesPercentage",
            type: "bool",
            default: "false",
            description: "If true, shows percentage values next to each tag (e.g. 32.0%)."
        },
        {
            name: "ValueColor",
            type: "Color?",
            default: "null",
            description: "Color used for numeric values in tags. If null, uses default console foreground."
        }
    ],
    examples: ["BreakdownChart_1.razor"]
},
{
    name: "Scrollable",
    description: "Renders a limited portion of a large collection (`PageSize`) with keyboard navigation.",
    category: "Layout",
    parameters: [
        {
            "name": "Items",
            "type": "IReadOnlyList<TItem>",
            "default": "Array.Empty<TItem>()",
            "description": "Full data source to scroll through."
        },
        {
            "name": "PageSize",
            "type": "int",
            "default": "1",
            "description": "Number of items visible at once."
        },
        {
            "name": "ChildContent",
            "type": "RenderFragment<ScrollContext<TItem>>",
            "description": "Markup for the current page. Receives `context` with visible items and helpers."
        },
        {
            "name": "ScrollOffset",
            "type": "int",
            "default": "0",
            "description": "Two-way bindable start index of the current page."
        },
        {
            "name": "ScrollOffsetChanged",
            "type": "EventCallback<int>",
            "default": "null",
            "description": "Invoked when offset changes (e.g. via keyboard)."
        }
    ],
    examples: ["Scrollable_1.razor"]
},
{
    name: "StepChart",
    description: "Renders a terminal step chart using Unicode box-drawing characters. Perfect for displaying discrete value changes over time or categories.",
    category: "Display",
    parameters: [
        {
            "name": "Width",
            "type": "int",
            "default": "60",
            "description": "Width of the chart area in terminal columns (excluding axes when shown)."
        },
        {
            "name": "Height",
            "type": "int",
            "default": "20",
            "description": "Height of the chart area in terminal rows (excluding title and axes)."
        },
        {
            "name": "ShowAxes",
            "type": "bool",
            "default": "true",
            "description": "When true, draws X/Y axes with tick marks and automatic numeric labels."
        },
        {
            "name": "AxesColor",
            "type": "Color",
            "default": "Color.Grey",
            "description": "Color of the axis lines and tick marks."
        },
        {
            "name": "LabelsColor",
            "type": "Color",
            "default": "Color.Grey",
            "description": "Color of the numeric labels displayed on the axes."
        },
        {
            "name": "Title",
            "type": "string?",
            "default": "null",
            "description": "Optional chart title rendered above the plot area."
        },
        {
            "name": "TitleColor",
            "type": "Color",
            "default": "Color.Grey",
            "description": "Color of the title text."
        },
        {
            "name": "Series",
            "type": "List<ChartSeries>",
            "description": "Collection of data series to plot. Each series can have its own color and points."
        }
    ],
    examples: ["StepChart_1.razor"]
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
    examples: ["Columns_1.razor"]
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
    examples: ["Rows_1.razor"]
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
    examples: ["Grid_1.razor"]
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
    examples: ["Padder_1.razor"]
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
    examples: ["TextButton_1.razor"]
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
    examples: ["TextInput_1.razor"]
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
      },
      {
        "name": "OnSelected",
        "type": "EventCallback<string?>",
        "description": "Fired after a user confirms an option."
      },
      {
        "name": "OnClear",
        "type": "EventCallback",
        "description": "Fired when selection is cleared."
      },
      {
        "name": "Expand",
        "type": "bool",
        "default": "false",
        "description": "Stretch the control horizontally."
      },
      {
        "name": "FocusOrder",
        "type": "int?",
        "default": "null",
        "description": "Order key for focus navigation."
      },
      {
        "name": "BorderStyle",
        "type": "BoxBorder",
        "default": "BoxBorder.Rounded",
        "description": "Border style for the input panel."
      },
    
      // --- Appearance Parameters ---
      {
        "name": "PlaceholderColor",
        "type": "Color",
        "default": "Color.Grey70",
        "description": "Styling when no selection exists (Color)."
      },
      {
        "name": "PlaceholderDecoration",
        "type": "Decoration",
        "default": "Decoration.Italic",
        "description": "Styling when no selection exists (Decoration)."
      },
      {
        "name": "ValueColor",
        "type": "Color",
        "default": "Color.White",
        "description": "Styling for selected value (Color)."
      },
      {
        "name": "ValueDecoration",
        "type": "Decoration",
        "default": "Decoration.None",
        "description": "Styling for selected value (Decoration)."
      },
      {
        "name": "EmptyLabel",
        "type": "string",
        "default": "\"No options available\"",
        "description": "Displayed when Options is empty."
      },
      {
        "name": "EmptyForeground",
        "type": "Color",
        "default": "Color.Grey70",
        "description": "Color for empty state text."
      },
      {
        "name": "EmptyDecoration",
        "type": "Decoration",
        "default": "Decoration.Italic",
        "description": "Decoration for empty state text."
      },
      {
        "name": "OptionForeground",
        "type": "Color",
        "default": "Color.White",
        "description": "Styling for list options (Color)."
      },
      {
        "name": "OptionDecoration",
        "type": "Decoration",
        "default": "Decoration.None",
        "description": "Styling for list options (Decoration)."
      },
      {
        "name": "SelectedOptionForeground",
        "type": "Color",
        "default": "Color.Chartreuse1",
        "description": "Highlight for the focused option (Color)."
      },
      {
        "name": "SelectedOptionDecoration",
        "type": "Decoration",
        "default": "Decoration.Bold",
        "description": "Highlight for the focused option (Decoration)."
      }
    ],
    examples: ["Select_1.razor"]
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
    examples: ["Markup_1.razor"]
  },
  {
    name: "Markdown",
    description: "Render markdown string.",
    category: "Display",
    examples: ["Markdown_1.razor"]
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
    examples: ["Panel_1.razor"]
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
    examples: ["Figlet_1.razor"]
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
    examples: ["SyntaxHighlighter_1.razor"]
  },
  {
    name: "Table",
    description: "Turns semantic HTML table markup into a Spectre.Console Table renderable.",
    category: "Display",
    examples: ["Table_1.razor"]
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
    examples: ["Spinner_1.razor"]
  },
  {
    name: "Newline",
    description: "Emits a single line break. No parameters.",
    category: "Utilities",
    examples: ["Newline_1.razor"]
  },
  {
    name: "SpectreCanvas",
    description: "Renders an array of pixels with different colors.",
    category: "Display",
    parameters: [
      {
        name: "Pixels",
        type: "(int x, int y, Color color)[]",
        description: "Array of (x, y, color) tuples. Required."
      },
      {
        name: "CanvasWidth",
        type: "int",
        description: "Canvas width. Required."
      },
      {
        name: "CanvasHeight",
        type: "int",
        description: "Canvas height. Required."
      },
      {
        name: "MaxWidth",
        type: "int?",
        default: "null",
        description: "Max width of the canvas."
      },
      {
        name: "PixelWidth",
        type: "int",
        default: "2",
        description: "Pixel width in characters. One character is half of the square so default value is 2."
      },
      {
        name: "Scale",
        type: "bool",
        default: "false",
        description: "The value indicating whether or not to scale the canvas when rendering."
      }
    ],
    examples: ["SpectreCanvas_1.razor"]
  }
]
