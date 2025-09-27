using System;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public static class SpectreRenderableFactory
{
    public static bool TryCreateRenderable(string html, out IRenderable? renderable)
    {
        renderable = null;

        if (string.IsNullOrWhiteSpace(html))
        {
            return false;
        }

        try
        {
            var document = XDocument.Parse(html, LoadOptions.None);
            var root = document.Root;
            if (root is null)
            {
                return false;
            }

            if (HtmlToSpectreRenderableConverter.TryConvertToRenderable(root, out var candidate))
            {
                renderable = candidate;
                return true;
            }

            return false;
        }
        catch
        {
            renderable = null;
            return false;
        }
    }

}
