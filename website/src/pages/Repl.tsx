import { useState, useEffect } from "react";
import Editor from "@monaco-editor/react";
import XTermPreview from "@/components/XTermPreview";
import { useTheme } from "@/components/ThemeProvider";
import { compileAndRegisterComponent } from "@/lib/xtermConsole";

// Sample templates for the REPL
const sampleTemplates = {
  counter: {
    name: "Counter Example",
    code: `@using Spectre.Console
@using RazorConsole.Components

<Panel Title="Counter" Border="BoxBorder.Rounded">
    <Rows>
        <Columns>
            <Markup Content="Count:" Foreground="Color.Grey70" />
            <Markup Content="@count.ToString()" Foreground="Color.Green" />
        </Columns>
        <TextButton Content="Click me" 
                    OnClick="Increment" 
                    BackgroundColor="Color.Grey" 
                    FocusedColor="Color.Blue" />
    </Rows>
</Panel>

@code {
    private int count = 0;
    private void Increment() => count++;
}`,
  },
  textButton: {
    name: "Text Button",
    code: `@using Spectre.Console
@using RazorConsole.Components

<p>Count: @count</p>
<TextButton Content="Click me" 
            OnClick="HandleClick" 
            BackgroundColor="Color.Grey" 
            FocusedColor="Color.Blue" />
<Markup Content="Press Enter to click" Foreground="Color.Green" />

@code {
    private int count = 0;
    private void HandleClick() {
        count++;
        StateHasChanged();
    }
}`,
  },
  textInput: {
    name: "Text Input",
    code: `@using Spectre.Console
@using RazorConsole.Components

<Panel Title="Text Input Example">
    <Rows>
        <Markup Content="Enter your name:" />
        <TextInput Placeholder="Type here..." 
                   Value="@name" 
                   OnValueChanged="OnNameChanged" />
        <Markup Content="Hello, @name!" Foreground="Color.Green" />
    </Rows>
</Panel>

@code {
    private string name = "World";
    private void OnNameChanged(string newValue) => name = newValue;
}`,
  },
  select: {
    name: "Select Component",
    code: `@using Spectre.Console
@using RazorConsole.Components

<Panel Title="Select Example">
    <Rows>
        <Markup Content="Choose a color:" />
        <Select Options="@options" 
                SelectedOption="@selected" 
                OnSelectionChanged="OnSelectionChanged" />
        <Markup Content="Selected: @selected" Foreground="Color.Blue" />
    </Rows>
</Panel>

@code {
    private string[] options = { "Red", "Green", "Blue", "Yellow" };
    private string selected = "Red";
    private void OnSelectionChanged(string value) => selected = value;
}`,
  },
  markup: {
    name: "Styled Text (Markup)",
    code: `@using Spectre.Console
@using RazorConsole.Components

<Panel Title="Markup Styles">
    <Rows>
        <Markup Content="[red]Red text[/]" />
        <Markup Content="[green bold]Bold green text[/]" />
        <Markup Content="[blue underline]Underlined blue[/]" />
        <Markup Content="[yellow on purple]Yellow on purple[/]" />
    </Rows>
</Panel>`,
  },
};

export default function Repl() {
  const [code, setCode] = useState(sampleTemplates.counter.code);
  const [selectedTemplate, setSelectedTemplate] = useState<keyof typeof sampleTemplates>("counter");
  const { theme } = useTheme();
  const [isDark, setIsDark] = useState(true);
  const [editorMounted, setEditorMounted] = useState(false);
  const [isCompiling, setIsCompiling] = useState(false);
  const [compilationResult, setCompilationResult] = useState<string | null>(null);
  const [dynamicComponentId, setDynamicComponentId] = useState<string>("dynamic");

  // Sync theme
  useEffect(() => {
    const checkTheme = () => {
      if (theme === "system") {
        setIsDark(window.matchMedia("(prefers-color-scheme: dark)").matches);
      } else {
        setIsDark(theme === "dark");
      }
    };
    checkTheme();

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handler = () => checkTheme();
    
    mediaQuery.addEventListener("change", handler);
    return () => mediaQuery.removeEventListener("change", handler);
  }, [theme]);

  const handleTemplateChange = (template: keyof typeof sampleTemplates) => {
    setSelectedTemplate(template);
    setCode(sampleTemplates[template].code);
    setCompilationResult(null);
  };

  const handleRunCode = async () => {
    setIsCompiling(true);
    setCompilationResult(null);
    
    try {
      // Generate a unique component ID for this compilation
      const componentId = `dynamic_${Date.now()}`;
      setDynamicComponentId(componentId);
      
      // Call the WASM compilation method via the proper export
      const result = await compileAndRegisterComponent(componentId, code);
      
      if (result.startsWith("ERROR:")) {
        setCompilationResult(result);
      } else {
        setCompilationResult("✓ Compilation successful!");
      }
    } catch (error) {
      setCompilationResult(`ERROR: ${error instanceof Error ? error.message : String(error)}`);
    } finally {
      setIsCompiling(false);
    }
  };

  return (
    <div className="flex flex-col h-screen">
      {/* Header */}
      <div className="border-b border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 px-6 py-4">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
              RazorConsole REPL
            </h1>
            <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
              Edit Razor components and see live preview
            </p>
          </div>
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <label className="text-sm font-medium text-slate-700 dark:text-slate-300">
                Template:
              </label>
              <select
                value={selectedTemplate}
                onChange={(e) => handleTemplateChange(e.target.value as keyof typeof sampleTemplates)}
                className="px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-800 text-slate-900 dark:text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {Object.entries(sampleTemplates).map(([key, { name }]) => (
                  <option key={key} value={key}>
                    {name}
                  </option>
                ))}
              </select>
            </div>
            <button
              onClick={handleRunCode}
              disabled={isCompiling}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white rounded-md text-sm font-medium focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
            >
              {isCompiling ? "Compiling..." : "Run"}
            </button>
          </div>
        </div>
      </div>

      {/* Editor and Preview Split */}
      <div className="flex-1 flex overflow-hidden">
        {/* Editor Panel */}
        <div className="w-1/2 border-r border-slate-200 dark:border-slate-800 flex flex-col">
          <div className="bg-slate-100 dark:bg-slate-800 px-4 py-2 border-b border-slate-200 dark:border-slate-700">
            <h2 className="text-sm font-semibold text-slate-900 dark:text-white">
              Editor
            </h2>
          </div>
          <div className="flex-1 overflow-hidden">
            {!editorMounted && (
              <div className="flex items-center justify-center h-full">
                <div className="text-slate-600 dark:text-slate-400">Loading editor...</div>
              </div>
            )}
            <Editor
              height="100%"
              defaultLanguage="csharp"
              language="csharp"
              value={code}
              onChange={(value) => setCode(value || "")}
              theme={isDark ? "vs-dark" : "light"}
              onMount={() => setEditorMounted(true)}
              options={{
                minimap: { enabled: false },
                fontSize: 14,
                lineNumbers: "on",
                roundedSelection: false,
                scrollBeyondLastLine: false,
                readOnly: false,
                automaticLayout: true,
                tabSize: 2,
              }}
            />
          </div>
          <div className="bg-slate-50 dark:bg-slate-800 px-4 py-2 border-t border-slate-200 dark:border-slate-700">
            {compilationResult ? (
              <p className={`text-xs font-medium ${
                compilationResult.startsWith("ERROR:") 
                  ? "text-red-600 dark:text-red-400" 
                  : "text-green-600 dark:text-green-400"
              }`}>
                {compilationResult}
              </p>
            ) : (
              <p className="text-xs text-slate-600 dark:text-slate-400">
                💡 Click "Run" to compile and preview your code
              </p>
            )}
          </div>
        </div>

        {/* Preview Panel */}
        <div className="w-1/2 flex flex-col bg-slate-50 dark:bg-slate-900">
          <div className="bg-slate-100 dark:bg-slate-800 px-4 py-2 border-b border-slate-200 dark:border-slate-700">
            <h2 className="text-sm font-semibold text-slate-900 dark:text-white">
              Live Preview
            </h2>
          </div>
          <div className="flex-1 p-4 overflow-auto">
            <XTermPreview
              elementId={compilationResult?.startsWith("✓") ? dynamicComponentId : selectedTemplate}
              className="h-full"
              key={compilationResult?.startsWith("✓") ? dynamicComponentId : selectedTemplate}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
