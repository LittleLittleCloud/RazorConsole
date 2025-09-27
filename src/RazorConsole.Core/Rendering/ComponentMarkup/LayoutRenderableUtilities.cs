using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal static class LayoutRenderableUtilities
{
    public static IEnumerable<IRenderable> ConvertChildNodesToRenderables(IEnumerable<XNode> nodes)
    {
        foreach (var node in nodes)
        {
            foreach (var renderable in ConvertNode(node))
            {
                yield return renderable;
            }
        }
    }

    private static IEnumerable<IRenderable> ConvertNode(XNode node)
    {
        switch (node)
        {
            case XText text:
                {
                    var value = text.Value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return new Markup(Markup.Escape(value));
                    }

                    yield break;
                }
            case XElement element:
                {
                    if (HtmlToSpectreRenderableConverter.TryConvertToRenderable(element, out var renderable) && renderable is not null)
                    {
                        yield return renderable;
                        yield break;
                    }

                    var markup = HtmlToSpectreRenderableConverter.ConvertNodes(new[] { element });
                    if (!string.IsNullOrWhiteSpace(markup))
                    {
                        yield return new Markup(markup);
                    }

                    yield break;
                }
            default:
                yield break;
        }
    }

    public static bool TryParseJustify(string? value, out Justify justify)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            justify = Justify.Left;
            return false;
        }

        if (Enum.TryParse(value, true, out justify))
        {
            return true;
        }

        // Allow aliases that map better to Spectre's justified values.
        if (string.Equals(value, "start", StringComparison.OrdinalIgnoreCase))
        {
            justify = Justify.Left;
            return true;
        }

        if (string.Equals(value, "end", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "right", StringComparison.OrdinalIgnoreCase))
        {
            justify = Justify.Right;
            return true;
        }

        if (string.Equals(value, "center", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "centre", StringComparison.OrdinalIgnoreCase))
        {
            justify = Justify.Center;
            return true;
        }

        justify = Justify.Left;
        return false;
    }
}
