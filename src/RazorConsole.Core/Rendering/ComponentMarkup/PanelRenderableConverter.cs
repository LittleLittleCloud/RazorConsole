using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RazorConsole.Core.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class PanelRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out ComponentRenderable renderable)
    {
        if (!IsPanelElement(element))
        {
            renderable = default;
            return false;
        }

        var panel = CreatePanel(element);
        var markup = BuildMarkupFallback(element);
        renderable = new ComponentRenderable(markup, panel);
        return true;
    }

    private static bool IsPanelElement(XElement element)
        => string.Equals(element.Attribute("data-border")?.Value, "panel", StringComparison.OrdinalIgnoreCase);

    private static Panel CreatePanel(XElement element)
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
                if (IsPanelElement(element))
                {
                    yield return CreatePanel(element);
                    yield break;
                }

                if (HtmlToSpectreRenderableConverter.TryConvertToRenderable(element, out var componentRenderable))
                {
                    yield return componentRenderable.Renderable;
                    yield break;
                }

                var markup = HtmlToSpectreRenderableConverter.ConvertNodes(new XNode[] { element });
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
