using Microsoft.AspNetCore.Components;

namespace RazorConsole.Core.Rendering;

public class ConsoleNavigationManager : NavigationManager
{
    public ConsoleNavigationManager()
    {
        Initialize("app:///", "app:///");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        var absolute = ToAbsoluteUri(uri);
        Uri = absolute.ToString();

        NotifyLocationChanged(isInterceptedLink: false);
    }
}
