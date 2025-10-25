import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Terminal, Code, Play } from "lucide-react"

export default function QuickStart() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="container mx-auto px-4 py-16 max-w-4xl">
        <div className="mb-8">
          <h1 className="text-4xl font-bold mb-4">Quick Start Guide</h1>
          <p className="text-slate-600 dark:text-slate-300 text-lg">
            Get started with RazorConsole in just a few steps
          </p>
        </div>

        <div className="space-y-8">
          {/* Installation */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Terminal className="w-5 h-5" />
                Step 1: Installation
              </CardTitle>
              <CardDescription>Install RazorConsole via NuGet</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>Add the RazorConsole.Core package to your project:</p>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>dotnet add package RazorConsole.Core</code>
              </pre>
            </CardContent>
          </Card>

          {/* Project Setup */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Code className="w-5 h-5" />
                Step 2: Project Setup
              </CardTitle>
              <CardDescription>Configure your project to use the Razor SDK</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>RazorConsole requires the Microsoft.NET.Sdk.Razor SDK to compile Razor components. Update your project file (`.csproj`):</p>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>{`<Project Sdk="Microsoft.NET.Sdk.Razor">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
    </PropertyGroup>
</Project>`}</code>
              </pre>
            </CardContent>
          </Card>

          {/* Basic Example */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Play className="w-5 h-5" />
                Step 3: Create Your First Component
              </CardTitle>
              <CardDescription>Build a simple counter application</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>Create a file called `Counter.razor`:</p>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>{`@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
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
    
    private void IncrementCount()
    {
        currentCount++;
    }
}`}</code>
              </pre>
              
              <p className="mt-4">Then in your `Program.cs`:</p>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>{`using Microsoft.Extensions.Hosting;
using RazorConsole.Core;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<Counter>();
    
IHost host = hostBuilder.Build();
await host.RunAsync();`}</code>
              </pre>
            </CardContent>
          </Card>

          {/* Run */}
          <Card>
            <CardHeader>
              <CardTitle>Step 4: Run Your Application</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>Build and run your application:</p>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>dotnet run</code>
              </pre>
              <p className="text-sm text-slate-600 dark:text-slate-400">
                You should see an interactive counter in your terminal! Use Tab to navigate between elements and press Enter or Space to click the button.
              </p>
            </CardContent>
          </Card>

          {/* Next Steps */}
          <Card>
            <CardHeader>
              <CardTitle>Next Steps</CardTitle>
              <CardDescription>Continue your journey with RazorConsole</CardDescription>
            </CardHeader>
            <CardContent>
              <ul className="space-y-2">
                <li>
                  <a href="/components" className="text-blue-600 hover:underline">
                    • Explore all built-in components
                  </a>
                </li>
                <li>
                  <a href="/advanced" className="text-blue-600 hover:underline">
                    • Learn about advanced features like Hot Reload and Custom Translators
                  </a>
                </li>
                <li>
                  <a href="https://github.com/LittleLittleCloud/RazorConsole/tree/main/examples" 
                     target="_blank" 
                     rel="noopener noreferrer"
                     className="text-blue-600 hover:underline">
                    • Check out example projects
                  </a>
                </li>
                <li>
                  <a href="https://github.com/LittleLittleCloud/RazorConsole" 
                     target="_blank" 
                     rel="noopener noreferrer"
                     className="text-blue-600 hover:underline">
                    • View the source code on GitHub
                  </a>
                </li>
              </ul>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
