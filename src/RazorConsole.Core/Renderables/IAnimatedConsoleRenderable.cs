using System;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal interface IAnimatedConsoleRenderable
{
    TimeSpan RefreshInterval { get; }
}
