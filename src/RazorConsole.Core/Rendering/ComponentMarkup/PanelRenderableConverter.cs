using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class PanelRenderableConverter : IRenderableConverter, IMarkupConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsPanelElement(element))
        {
            renderable = default!;
            return false;
        }

        renderable = CreatePanel(element);
        return true;
    }

    public bool TryConvert(XElement element, out string markup)
    {
        if (!IsPanelElement(element))
        {
            markup = string.Empty;
            return false;
        }

        markup = BuildMarkupFallback(element);
        return true;
    }

    private static bool IsPanelElement(XElement element)
        => string.Equals(element.Attribute("data-border")?.Value, "panel", StringComparison.OrdinalIgnoreCase)
           || string.Equals(element.Attribute("data-panel")?.Value, "true", StringComparison.OrdinalIgnoreCase);

    private static Panel CreatePanel(XElement element)
    {
        var orientation = element.Attribute("data-panel-orientation")?.Value ?? element.Attribute("data-orientation")?.Value;
        var content = BuildPanelContent(element.Nodes(), orientation);
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

    private static IRenderable BuildPanelContent(IEnumerable<XNode> nodes, string? orientation)
    {
        var renderables = LayoutRenderableUtilities.ConvertChildNodesToRenderables(nodes).ToList();

        if (renderables.Count == 0)
        {
            return new Markup(string.Empty);
        }

        if (renderables.Count == 1)
        {
            return renderables[0];
        }

        var useHorizontal = string.Equals(orientation, "horizontal", StringComparison.OrdinalIgnoreCase);

        if (useHorizontal)
        {
            return new Columns(renderables);
        }

        return new Rows(renderables);
    }

    private static string BuildMarkupFallback(XElement element)
    {
        var builder = new StringBuilder();
        var header = element.Attribute("data-header")?.Value;
        if (!string.IsNullOrWhiteSpace(header))
        {
            var headerColor = element.Attribute("data-header-color")?.Value;
            var headerMarkup = !string.IsNullOrWhiteSpace(headerColor)
                ? $"[{headerColor}]{Markup.Escape(header)}[/]"
                : Markup.Escape(header);

            builder.Append(headerMarkup);
            builder.AppendLine();
        }

        var bodyMarkup = HtmlToSpectreRenderableConverter.ConvertNodes(element.Nodes());
        builder.Append(bodyMarkup);

        return builder.ToString().TrimEnd();
    }
}
