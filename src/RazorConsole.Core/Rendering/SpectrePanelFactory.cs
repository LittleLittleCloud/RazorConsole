using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public static class SpectrePanelFactory
{
    public static bool TryCreatePanel(string html, out Panel? panel)
    {
        panel = null;

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

            var borderMarker = root.Attribute("data-border")?.Value;
            if (!string.Equals(borderMarker, "panel", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            panel = CreatePanelFromElement(root);
            return panel is not null;
        }
        catch
        {
            panel = null;
            return false;
        }
    }

    private static Panel? CreatePanelFromElement(XElement element)
    {
        var content = BuildPanelContent(element.Nodes());
        var panel = new Panel(content)
            .Expand()
            .SquareBorder();

        var header = element.Attribute("data-header")?.Value;
        if (!string.IsNullOrWhiteSpace(header))
        {
            var headerColor = element.Attribute("data-header-color")?.Value;
            var headerMarkup = !string.IsNullOrWhiteSpace(headerColor)
                ? $"[{headerColor}]{Markup.Escape(header)}[/]"
                : Markup.Escape(header);

            panel.Header = new PanelHeader(headerMarkup);
        }

        var borderColorValue = element.Attribute("data-border-color")?.Value;
        if (!string.IsNullOrWhiteSpace(borderColorValue))
        {
            try
            {
                var style = Style.Parse(borderColorValue);
                panel.BorderStyle(style);
            }
            catch (Exception)
            {
                // Ignore invalid color specifications and keep default styling.
            }
        }

        return panel;
    }

    private static IRenderable BuildPanelContent(IEnumerable<XNode> nodes)
    {
        var renderables = BuildRenderables(nodes).ToList();

        if (renderables.Count == 0)
        {
            return new Markup(string.Empty);
        }

        if (renderables.Count == 1)
        {
            return renderables[0];
        }

        return new Rows(renderables);
    }

    private static IEnumerable<IRenderable> BuildRenderables(IEnumerable<XNode> nodes)
    {
        foreach (var node in nodes)
        {
            foreach (var renderable in BuildRenderable(node))
            {
                yield return renderable;
            }
        }
    }

    private static IEnumerable<IRenderable> BuildRenderable(XNode node)
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
                if (TryCreatePanelFromElement(element, out var nestedPanel) && nestedPanel is not null)
                {
                    yield return nestedPanel;
                    yield break;
                }

                var markup = HtmlToSpectreMarkupConverter.ConvertNodes(new XNode[] { element });
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

    private static bool TryCreatePanelFromElement(XElement element, out Panel? panel)
    {
        panel = null;

        var marker = element.Attribute("data-border")?.Value;
        if (!string.Equals(marker, "panel", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        panel = CreatePanelFromElement(element);
        return panel is not null;
    }
}
