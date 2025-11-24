// Create a Main method to make tool chain happy.
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using RazorConsole.Components;
using RazorConsole.Website;
using RazorConsole.Website.Components;
[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]


Console.WriteLine("Program.cs loaded");
[SupportedOSPlatform("browser")]
public partial class Registry
{
    private static readonly Dictionary<string, IRazorConsoleRenderer> _renderers = new();
    private static readonly HashSet<string> _subscriptions = new();
    private static readonly Dictionary<string, Type> _dynamicComponents = new();

    [JSExport]
    [SupportedOSPlatform("browser")]
    public static void RegisterComponent(string elementID)
    {
        Console.WriteLine(elementID);
        switch (elementID)
        {
            case "Align":
                _renderers[elementID] = new RazorConsoleRenderer<Align_1>(elementID);
                break;
            case "Border":
                _renderers[elementID] = new RazorConsoleRenderer<Border_1>(elementID);
                break;
            case "Scrollable":
                _renderers[elementID] = new RazorConsoleRenderer<Scrollable_1>(elementID);
                break;
            case "Columns":
                _renderers[elementID] = new RazorConsoleRenderer<Columns_1>(elementID);
                break;
            case "Rows":
                _renderers[elementID] = new RazorConsoleRenderer<Rows_1>(elementID);
                break;
            case "Grid":
                _renderers[elementID] = new RazorConsoleRenderer<Grid_1>(elementID);
                break;
            case "Padder":
                _renderers[elementID] = new RazorConsoleRenderer<Padder_1>(elementID);
                break;
            case "TextButton":
                _renderers[elementID] = new RazorConsoleRenderer<TextButton_1>(elementID);
                break;
            case "TextInput":
                _renderers[elementID] = new RazorConsoleRenderer<TextInput_1>(elementID);
                break;
            case "Select":
                _renderers[elementID] = new RazorConsoleRenderer<Select_1>(elementID);
                break;
            case "Markup":
                _renderers[elementID] = new RazorConsoleRenderer<Markup_1>(elementID);
                break;
            case "Markdown":
                _renderers[elementID] = new RazorConsoleRenderer<Markdown_1>(elementID);
                break;
            case "Panel":
                _renderers[elementID] = new RazorConsoleRenderer<Panel_1>(elementID);
                break;
            case "Figlet":
                _renderers[elementID] = new RazorConsoleRenderer<Figlet_1>(elementID);
                break;
            case "SyntaxHighlighter":
                _renderers[elementID] = new RazorConsoleRenderer<SyntaxHighlighter_1>(elementID);
                break;
            case "Table":
                _renderers[elementID] = new RazorConsoleRenderer<Table_1>(elementID);
                break;
            case "Spinner":
                _renderers[elementID] = new RazorConsoleRenderer<Spinner_1>(elementID);
                break;
            case "Newline":
                _renderers[elementID] = new RazorConsoleRenderer<Newline_1>(elementID);
                break;
            case "SpectreCanvas":
                _renderers[elementID] = new RazorConsoleRenderer<SpectreCanvas_1>(elementID);
                break;
            case "BarChart":
                _renderers[elementID] = new RazorConsoleRenderer<BarChart_1>(elementID);
                break;
            case "BreakdownChart":
                _renderers[elementID] = new RazorConsoleRenderer<BreakdownChart_1>(elementID);
                break;
            case "counter":
                _renderers[elementID] = new RazorConsoleRenderer<Counter>(elementID);
                break;
            case "textButton":
                _renderers[elementID] = new RazorConsoleRenderer<TextButtonExample>(elementID);
                break;
            case "textInput":
                _renderers[elementID] = new RazorConsoleRenderer<TextInputExample>(elementID);
                break;
            case "select":
                _renderers[elementID] = new RazorConsoleRenderer<SelectExample>(elementID);
                break;
            case "markup":
                _renderers[elementID] = new RazorConsoleRenderer<MarkupExample>(elementID);
                break;
        }
    }

    [JSExport]
    [SupportedOSPlatform("browser")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2081", Justification = "Dynamic component type is preserved by DynamicallyAccessedMembers")]
    public static async Task<string> CompileAndRegisterComponent(string elementID, string razorCode)
    {
        try
        {
            Console.WriteLine($"Compiling component for {elementID}");
            var (componentType, error) = await DynamicComponentCompiler.CompileRazorComponentAsync(razorCode);
            
            if (error != null)
            {
                Console.WriteLine($"Compilation failed: {error}");
                return $"ERROR: {error}";
            }

            if (componentType == null)
            {
                return "ERROR: Component type is null";
            }

            // Store the dynamic component type
            _dynamicComponents[elementID] = componentType;
            
            // Create renderer using reflection
            var rendererType = typeof(RazorConsoleRenderer<>).MakeGenericType(componentType);
            var renderer = (IRazorConsoleRenderer)Activator.CreateInstance(rendererType, elementID)!;
            _renderers[elementID] = renderer;

            Console.WriteLine($"Successfully registered dynamic component: {elementID}");
            return "SUCCESS";
        }
        catch (Exception ex)
        {
            var errorMsg = $"ERROR: {ex.Message}\n{ex.StackTrace}";
            Console.WriteLine(errorMsg);
            return errorMsg;
        }
    }

    [JSExport]
    [SupportedOSPlatform("browser")]
    public static async Task HandleKeyboardEvent(string elementID, string xtermKey, string domKey, bool ctrlKey, bool altKey, bool shiftKey)
    {
        if (!_renderers.TryGetValue(elementID, out var renderer))
        {
            return;
        }
        await renderer.HandleKeyboardEventAsync(xtermKey, domKey, ctrlKey, altKey, shiftKey)
            .ConfigureAwait(false);
    }
}

[SupportedOSPlatform("browser")]
public partial class XTermInterop
{
    [JSImport("writeToTerminal", "main.js")]
    public static partial void WriteToTerminal(string componentName, string data);
}
