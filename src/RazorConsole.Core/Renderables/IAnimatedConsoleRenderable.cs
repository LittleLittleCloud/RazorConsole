// Copyright (c) RazorConsole. All rights reserved.

namespace RazorConsole.Core.Rendering.ComponentMarkup;

/// <summary>
/// Represents a console renderable that requires periodic animation updates.
/// </summary>
public interface IAnimatedConsoleRenderable
{
    /// <summary>
    /// Gets the refresh interval for animation updates.
    /// </summary>
    TimeSpan RefreshInterval { get; }
}
