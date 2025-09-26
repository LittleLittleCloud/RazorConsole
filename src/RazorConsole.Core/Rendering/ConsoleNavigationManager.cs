using Microsoft.AspNetCore.Components;

namespace RazorConsole.Core.Rendering;

public sealed class ConsoleNavigationManager : NavigationManager
{
    public ConsoleNavigationManager()
    {
        Initialize("app://local/", "app://local/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        // Navigation is not supported in the console environment.
    }
}
