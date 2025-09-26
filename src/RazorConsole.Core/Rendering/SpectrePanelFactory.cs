using System;
using System.Xml.Linq;
using Spectre.Console;

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

            var contentMarkup = HtmlToSpectreMarkupConverter.ConvertNodes(root.Nodes());
            panel = new Panel(new Markup(contentMarkup))
                .Expand()
                .SquareBorder();

            var header = root.Attribute("data-header")?.Value;
            if (!string.IsNullOrWhiteSpace(header))
            {
                var headerColor = root.Attribute("data-header-color")?.Value;
                var headerMarkup = !string.IsNullOrWhiteSpace(headerColor)
                    ? $"[{headerColor}]{Markup.Escape(header)}[/]"
                    : Markup.Escape(header);

                panel.Header = new PanelHeader(headerMarkup);
            }

            var borderColorValue = root.Attribute("data-border-color")?.Value;
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

            return true;
        }
        catch
        {
            panel = null;
            return false;
        }
    }
}
