// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components.Routing;

namespace RazorConsole.Core.Rendering;

public class NoopNavigationInterception : INavigationInterception
{
    public Task EnableNavigationInterceptionAsync()
        => Task.CompletedTask;
}

