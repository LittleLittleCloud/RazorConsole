#if BROWSER
using RazorConsole.Gallery.Services;

namespace RazorConsole.Gallery.Platforms.Browser;

public class MockNuGetUpgradeChecker : INuGetUpgradeChecker
{
    public ValueTask<UpgradeCheckResult> CheckForUpgradeAsync(CancellationToken cancellationToken = default)
    {
        // In browser, we don't check for updates
        var result = UpgradeCheckResult.WithoutUpdate("0.0.0", null, null);
        return ValueTask.FromResult(result);
    }
}
#endif
