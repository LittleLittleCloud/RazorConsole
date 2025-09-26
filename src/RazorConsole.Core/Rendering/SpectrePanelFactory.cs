using System;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public static class SpectrePanelFactory
{
    [Obsolete("Use SpectreRenderableFactory.TryCreateRenderable instead")]
    public static bool TryCreatePanel(string html, out Panel? panel)
    {
        panel = null;

        if (!SpectreRenderableFactory.TryCreateRenderable(html, out var renderable) || renderable is null)
        {
            return false;
        }

        if (renderable is Panel spectrePanel)
        {
            panel = spectrePanel;
            return true;
        }

        return false;
    }
}
