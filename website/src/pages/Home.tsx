import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Link } from "react-router-dom"
import { Package, Zap, Code, Github, Terminal, ArrowRight } from "lucide-react"

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      {/* Hero Section */}
      <div className="container mx-auto px-4 py-16">
        <div className="text-center mb-16">
          <h1 className="text-5xl font-bold mb-4 bg-gradient-to-r from-blue-600 to-violet-600 bg-clip-text text-transparent">
            🚀 RazorConsole
          </h1>
          <p className="text-xl text-slate-600 dark:text-slate-300 mb-8 max-w-2xl mx-auto">
            Build rich, interactive console applications using familiar Razor syntax and the power of Spectre.Console
          </p>
          <div className="flex gap-4 justify-center flex-wrap">
            <Link to="/quick-start">
              <Button size="lg" className="gap-2">
                <Terminal className="w-4 h-4" />
                Quick Start
              </Button>
            </Link>
            <Link to="/components">
              <Button size="lg" variant="outline" className="gap-2">
                <Package className="w-4 h-4" />
                Browse Components
              </Button>
            </Link>
            <a href="https://github.com/LittleLittleCloud/RazorConsole" target="_blank" rel="noopener noreferrer">
              <Button size="lg" variant="secondary" className="gap-2">
                <Github className="w-4 h-4" />
                GitHub
              </Button>
            </a>
          </div>
        </div>

        {/* Features Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-16">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Code className="w-5 h-5 text-blue-600" />
                Component-Based
              </CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Build your console UI using familiar Razor components with full support for data binding, event handling, and component lifecycle methods.
              </CardDescription>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Zap className="w-5 h-5 text-violet-600" />
                Interactive
              </CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Create engaging user experiences with interactive elements like buttons, text inputs, selectors, and keyboard navigation.
              </CardDescription>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Package className="w-5 h-5 text-green-600" />
                15+ Built-in Components
              </CardTitle>
            </CardHeader>
            <CardContent>
              <CardDescription>
                Get started quickly with pre-built components covering layout, input, display, and navigation needs.
              </CardDescription>
            </CardContent>
          </Card>
        </div>

        {/* Quick Start Preview */}
        <Card className="mb-16">
          <CardHeader>
            <CardTitle>Quick Start</CardTitle>
            <CardDescription>Get up and running in minutes</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <h3 className="font-semibold mb-2">1. Install the package</h3>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>dotnet add package RazorConsole.Core</code>
              </pre>
            </div>
            <div>
              <h3 className="font-semibold mb-2">2. Update your project file</h3>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>{`<Project Sdk="Microsoft.NET.Sdk.Razor">
    <!-- your project settings -->
</Project>`}</code>
              </pre>
            </div>
            <div>
              <h3 className="font-semibold mb-2">3. Create your first component</h3>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>{`// Counter.razor
@using RazorConsole.Components

<Columns>
    <p>Current count</p>
    <Markup Content="@currentCount.ToString()" 
            Foreground="@Spectre.Console.Color.Green" />
</Columns>
<TextButton Content="Click me"
            OnClick="IncrementCount"
            BackgroundColor="@Spectre.Console.Color.Grey"
            FocusedColor="@Spectre.Console.Color.Blue" />

@code {
    private int currentCount = 0;
    private void IncrementCount() => currentCount++;
}`}</code>
              </pre>
            </div>
            <Link to="/quick-start">
              <Button className="gap-2">
                View Full Tutorial
                <ArrowRight className="w-4 h-4" />
              </Button>
            </Link>
          </CardContent>
        </Card>

        {/* Advanced Topics Preview */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Advanced Topics</CardTitle>
              <CardDescription>Dive deeper into RazorConsole</CardDescription>
            </CardHeader>
            <CardContent className="space-y-2">
              <Link to="/advanced#hot-reload" className="block hover:text-blue-600 transition-colors">
                • Hot Reload Support
              </Link>
              <Link to="/advanced#custom-translators" className="block hover:text-blue-600 transition-colors">
                • Custom Translators
              </Link>
              <Link to="/advanced#keyboard-events" className="block hover:text-blue-600 transition-colors">
                • Keyboard Events
              </Link>
              <Link to="/advanced#focus-management" className="block hover:text-blue-600 transition-colors">
                • Focus Management
              </Link>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Examples</CardTitle>
              <CardDescription>Real-world applications</CardDescription>
            </CardHeader>
            <CardContent className="space-y-2">
              <a href="https://github.com/LittleLittleCloud/RazorConsole/tree/main/examples/LLMAgentTUI" 
                 target="_blank" 
                 rel="noopener noreferrer"
                 className="block hover:text-blue-600 transition-colors">
                • LLM Agent TUI - Claude Code-inspired chat interface
              </a>
              <a href="https://github.com/LittleLittleCloud/RazorConsole/tree/main/src/RazorConsole.Gallery" 
                 target="_blank" 
                 rel="noopener noreferrer"
                 className="block hover:text-blue-600 transition-colors">
                • Interactive Component Gallery
              </a>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
