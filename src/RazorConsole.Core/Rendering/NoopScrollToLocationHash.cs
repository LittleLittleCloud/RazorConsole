using Microsoft.AspNetCore.Components.Routing;

namespace RazorConsole.Core.Rendering;

public class NoopScrollToLocationHash : IScrollToLocationHash
{
    public Task RefreshScrollPositionForHash(string locationAbsolute)
        => Task.CompletedTask;
    public ValueTask ScrollToLocationHashAsync(string uriWithHash)
        => ValueTask.CompletedTask;
}

