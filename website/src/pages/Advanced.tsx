import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Zap, Code, Keyboard, Focus } from "lucide-react"

export default function Advanced() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="container mx-auto px-4 py-16 max-w-4xl">
        <div className="mb-8">
          <h1 className="text-4xl font-bold mb-4">Advanced Topics</h1>
          <p className="text-slate-600 dark:text-slate-300 text-lg">
            Deep dive into advanced RazorConsole features
          </p>
        </div>

        <div className="space-y-8">
          {/* Hot Reload */}
          <Card id="hot-reload">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Zap className="w-5 h-5 text-orange-600" />
                Hot Reload Support
              </CardTitle>
              <CardDescription>Experience rapid development with built-in hot reload</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>
                RazorConsole supports hot reload via metadata update handler, allowing you to see your UI changes instantly without restarting your application.
              </p>
              <p className="font-semibold">How it works:</p>
              <ul className="list-disc pl-6 space-y-2">
                <li>Run your application with <code className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">dotnet watch</code></li>
                <li>Make changes to your Razor components</li>
                <li>Save the file</li>
                <li>The UI updates automatically in the running console application</li>
              </ul>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto mt-4">
                <code>dotnet watch run</code>
              </pre>
              <p className="text-sm text-slate-600 dark:text-slate-400 mt-2">
                Note: Hot reload works best with component updates. Some changes may require a full restart.
              </p>
            </CardContent>
          </Card>

          {/* Custom Translators */}
          <Card id="custom-translators">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Code className="w-5 h-5 text-blue-600" />
                Custom Translators
              </CardTitle>
              <CardDescription>Extend RazorConsole with custom rendering logic</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>
                RazorConsole uses a Virtual DOM (VDOM) translation system to convert Razor components into Spectre.Console renderables. 
                You can extend this system by creating custom translators.
              </p>
              
              <div>
                <h4 className="font-semibold mb-2">Creating a Custom Translator</h4>
                <p className="mb-2">Implement the <code className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">IVdomElementTranslator</code> interface:</p>
                <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto text-sm">
                  <code>{`using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

public sealed class OverflowElementTranslator : IVdomElementTranslator
{
    // Lower priority values are processed first (1-1000+)
    public int Priority => 85;

    public bool TryTranslate(
        VNode node, 
        TranslationContext context, 
        out IRenderable? renderable)
    {
        renderable = null;

        // Check if this is a div with overflow attribute
        if (node.Kind != VNodeKind.Element || 
            !string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-overflow", out var overflowType))
        {
            return false;
        }

        // Translate child nodes
        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(
            node.Children, context, out var children))
        {
            return false;
        }

        var content = VdomSpectreTranslator.ComposeChildContent(children);

        // Create renderable with overflow handling
        renderable = overflowType?.ToLowerInvariant() switch
        {
            "ellipsis" => new Padder(content).Overflow(Overflow.Ellipsis),
            "crop" => new Padder(content).Overflow(Overflow.Crop),
            "fold" => new Padder(content).Overflow(Overflow.Fold),
            _ => content
        };

        return true;
    }
}`}</code>
                </pre>
              </div>

              <div className="mt-6">
                <h4 className="font-semibold mb-2">Registering a Custom Translator</h4>
                <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto text-sm">
                  <code>{`using RazorConsole.Core;
using RazorConsole.Core.Vdom;

var app = AppHost.Create<MyComponent>(builder =>
{
    // Register your custom translator
    builder.Services.AddVdomTranslator<OverflowElementTranslator>();
});

await app.RunAsync();`}</code>
                </pre>
              </div>

              <div className="mt-6">
                <h4 className="font-semibold mb-2">Using in Components</h4>
                <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto text-sm">
                  <code>{`<div data-overflow="ellipsis">
    This text will be truncated with ellipsis if it's too long
</div>`}</code>
                </pre>
              </div>

              <p className="text-sm text-slate-600 dark:text-slate-400 mt-4">
                For comprehensive documentation, see the{" "}
                <a href="https://github.com/LittleLittleCloud/RazorConsole/blob/main/design-doc/custom-translators.md" 
                   target="_blank" 
                   rel="noopener noreferrer"
                   className="text-blue-600 hover:underline">
                  custom translators guide
                </a>.
              </p>
            </CardContent>
          </Card>

          {/* Keyboard Events */}
          <Card id="keyboard-events">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Keyboard className="w-5 h-5 text-green-600" />
                Keyboard Events
              </CardTitle>
              <CardDescription>Handle keyboard input in your components</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>
                RazorConsole provides full keyboard event support for interactive components. 
                You can handle key presses using familiar event handlers.
              </p>
              
              <div>
                <h4 className="font-semibold mb-2">Example: Custom Key Handling</h4>
                <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto text-sm">
                  <code>{`<div @onkeydown="HandleKeyPress">
    <Markup Content="@message" />
</div>

@code {
    private string message = "Press any key...";
    
    private void HandleKeyPress(KeyboardEventArgs e)
    {
        message = $"You pressed: {e.Key}";
        StateHasChanged();
    }
}`}</code>
                </pre>
              </div>

              <div className="mt-4">
                <h4 className="font-semibold mb-2">Supported Events</h4>
                <ul className="list-disc pl-6 space-y-1">
                  <li><code className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">@onkeydown</code> - Fired when a key is pressed</li>
                  <li><code className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">@onkeyup</code> - Fired when a key is released</li>
                  <li><code className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">@onkeypress</code> - Fired when a key is pressed and released</li>
                </ul>
              </div>
            </CardContent>
          </Card>

          {/* Focus Management */}
          <Card id="focus-management">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Focus className="w-5 h-5 text-violet-600" />
                Focus Management
              </CardTitle>
              <CardDescription>Navigate between interactive elements</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>
                RazorConsole automatically manages focus for interactive components. Users can navigate 
                between focusable elements using Tab (forward) and Shift+Tab (backward).
              </p>
              
              <div>
                <h4 className="font-semibold mb-2">Setting Focus Order</h4>
                <p className="mb-2">Use the <code className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded">FocusOrder</code> parameter to control navigation order:</p>
                <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto text-sm">
                  <code>{`<TextInput Value="@name" 
           ValueChanged="@((v) => name = v)"
           FocusOrder="1" />

<TextInput Value="@email" 
           ValueChanged="@((v) => email = v)"
           FocusOrder="2" />

<TextButton Content="Submit"
            OnClick="HandleSubmit"
            FocusOrder="3" />`}</code>
                </pre>
              </div>

              <div className="mt-4">
                <h4 className="font-semibold mb-2">Focus Events</h4>
                <p className="mb-2">Handle focus changes with event callbacks:</p>
                <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto text-sm">
                  <code>{`<div @onfocus="OnFocus" @onfocusout="OnFocusOut">
    <Markup Content="@message" />
</div>

@code {
    private string message = "Not focused";
    
    private void OnFocus(FocusEventArgs e)
    {
        message = "Focused!";
        StateHasChanged();
    }
    
    private void OnFocusOut(FocusEventArgs e)
    {
        message = "Focus lost";
        StateHasChanged();
    }
}`}</code>
                </pre>
              </div>
            </CardContent>
          </Card>

          {/* Component Gallery */}
          <Card>
            <CardHeader>
              <CardTitle>Interactive Component Gallery</CardTitle>
              <CardDescription>Try all components hands-on</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p>
                Explore all components interactively with the RazorConsole Component Gallery. 
                Install the tool globally and run it from any terminal:
              </p>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto">
                <code>dotnet tool install --global RazorConsole.Gallery</code>
              </pre>
              <pre className="bg-slate-900 text-slate-100 p-4 rounded-md overflow-x-auto mt-2">
                <code>razorconsole-gallery</code>
              </pre>
              <p className="text-sm text-slate-600 dark:text-slate-400">
                The gallery provides live examples of every component in action.
              </p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
