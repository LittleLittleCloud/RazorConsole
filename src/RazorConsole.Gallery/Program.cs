// Entry point routing based on platform
namespace RazorConsole.Gallery;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
#if BROWSER
        return await Platforms.Browser.BrowserProgram.Run(args);
#else
        return await Platforms.Desktop.DesktopProgram.Run(args);
#endif
    }
}
