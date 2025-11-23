// Create a Main method to make tool chain happy.
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using RazorConsole.Components;
using RazorConsole.Website;
using RazorConsole.Website.RazorConsoleComponents;
[assembly:System.Runtime.Versioning.SupportedOSPlatform("browser")]


Console.WriteLine("Program.cs loaded");
[SupportedOSPlatform("browser")]
public partial class Registry
{
    private static readonly Dictionary<string, IRazorConsoleRenderer> _renderers = new();
    private static readonly HashSet<string> _subscriptions = new();

    [JSExport]
    [SupportedOSPlatform("browser")]
    public static void RegisterComponent(string elementID)
    {
        if (elementID == "Align")
        {
            _renderers[elementID] = new RazorConsoleRenderer<Align1>(elementID);
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
