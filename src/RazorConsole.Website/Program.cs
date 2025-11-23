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
    static void RegisterComponent(string componentName)
    {
        Console.WriteLine($"Registering component: {componentName}");
        if (componentName == "Align")
        {
            _renderers[componentName] = new RazorConsoleRenderer<Align1>(componentName);
        }

        if (!_renderers.TryGetValue(componentName, out var renderer))
        {
            return;
        }
        
        if (!_subscriptions.Add(componentName))
        {
            return;
        }

        renderer.SnapshotRendered += (ansiString) =>
        {
            Console.WriteLine($"Writing to terminal from .NET: {ansiString}");
            XTermInterop.WriteToTerminal(componentName, ansiString);
        };
    }
}

[SupportedOSPlatform("browser")]
public partial class XTermInterop
{
    [JSImport("writeToTerminal", "main.js")]
    public static partial void WriteToTerminal(string componentName, string data);
}
