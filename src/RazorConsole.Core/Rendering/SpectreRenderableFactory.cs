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

            if (HtmlToSpectreRenderableConverter.TryConvertToRenderable(root, out var componentRenderable))
            {
                renderable = componentRenderable.Renderable;
                return true;
            }

            var markup = HtmlToSpectreRenderableConverter.Convert(html);
            if (string.IsNullOrWhiteSpace(markup))
            {
                return false;
            }

            renderable = new Markup(markup);
            return true;
        }
        catch
        {
            renderable = null;
            return false;
        }
    }

}
