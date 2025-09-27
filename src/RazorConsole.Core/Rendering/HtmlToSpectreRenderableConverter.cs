using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public static class HtmlToSpectreRenderableConverter
{
    private static readonly PanelRenderableConverter PanelConverter = new();
    private static readonly RowsRenderableConverter RowsConverter = new();
    private static readonly ColumnsRenderableConverter ColumnsConverter = new();
    private static readonly GridRenderableConverter GridConverter = new();
    private static readonly PadderRenderableConverter PadderConverter = new();
    private static readonly AlignRenderableConverter AlignConverter = new();
    private static readonly TextRenderableConverter TextConverter = new();
    private static readonly NewlineRenderableConverter NewlineConverter = new();
    private static readonly SpacerRenderableConverter SpacerConverter = new();
    private static readonly SpinnerRenderableConverter SpinnerConverter = new();

    private static readonly IReadOnlyList<IRenderableConverter> RenderableConverters = new IRenderableConverter[]
    {
        PanelConverter,
        RowsConverter,
        ColumnsConverter,
        GridConverter,
        PadderConverter,
        AlignConverter,
        TextConverter,
        NewlineConverter,
        SpacerConverter,
        SpinnerConverter,
    };

    private static readonly IReadOnlyList<IMarkupConverter> MarkupConverters = new IMarkupConverter[]
    {
        TextConverter,
        NewlineConverter,
        SpacerConverter,
        SpinnerConverter,
    };

    public static string Convert(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var wrapped = $"<root>{html}</root>";
        var document = XDocument.Parse(wrapped, LoadOptions.None);

        return ConvertNodes(document.Root!.Nodes());
    }

    public static string ConvertNodes(IEnumerable<XNode> nodes)
    {
        var builder = new StringBuilder();
        foreach (var node in nodes)
        {
            AppendNode(node, builder);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendNode(XNode node, StringBuilder builder)
    {
        switch (node)
        {
            case XText text:
                AppendText(text, builder);
                break;
            case XElement element:
                AppendElement(element, builder);
                break;
        }
    }

    private static void AppendText(XText text, StringBuilder builder)
    {
        var value = text.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append(Markup.Escape(value));
    }

    private static void AppendElement(XElement element, StringBuilder builder)
    {
        if (TryConvertUsingMarkupConverters(element, builder))
        {
            return;
        }

        var name = element.Name.LocalName.ToLowerInvariant();

        switch (name)
        {
            case "h1":
                builder.Append("[bold underline]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                builder.AppendLine();
                break;
            case "h2":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                builder.AppendLine();
                break;
            case "h3":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                builder.AppendLine();
                break;
            case "label":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]: ");
                break;
            case "p":
                string text = element.Value;
                text = text.Replace(Environment.NewLine, " ").Trim();
                element.Value = text;
                AppendChildren(element, builder);
                break;
            case "input":
                var value = element.Attribute("value")?.Value ?? string.Empty;
                var placeholder = element.Attribute("placeholder")?.Value ?? string.Empty;
                var display = !string.IsNullOrWhiteSpace(value)
                    ? Markup.Escape(value)
                    : (!string.IsNullOrWhiteSpace(placeholder) ? Markup.Escape(placeholder) : string.Empty);

                builder.Append("[grey53]<");
                builder.Append("input");
                if (!string.IsNullOrEmpty(display))
                {
                    builder.Append($" : {display}");
                }
                builder.AppendLine(">[/]");
                break;
            case "time":
                AppendChildren(element, builder);
                break;
            case "strong":
                builder.Append("[bold]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                break;
            case "em":
                builder.Append("[italic]");
                AppendChildren(element, builder);
                builder.Append("[/]");
                break;
            case "ul":
                foreach (var child in element.Elements("li"))
                {
                    builder.Append(" [green]•[/] ");
                    AppendChildren(child, builder);
                    builder.AppendLine();
                }
                builder.AppendLine();
                break;
            case "li":
                builder.Append(" [green]•[/] ");
                AppendChildren(element, builder);
                builder.AppendLine();
                break;
            case "br":
                builder.AppendLine();
                break;
            case "div":
                AppendChildrenWithSpacing(element, builder);
                break;
            case "span":
                AppendChildrenWithSpacing(element, builder);
                break;
            case "section":
            case "header":
            case "main":
            case "article":
            case "footer":
                AppendChildrenWithSpacing(element, builder);
                break;
            default:
                AppendChildren(element, builder);
                break;
        }
    }

    private static void AppendChildren(XElement element, StringBuilder builder)
    {
        foreach (var child in element.Nodes())
        {
            AppendNode(child, builder);
        }
    }

    private static void AppendChildrenWithSpacing(XElement element, StringBuilder builder)
    {
        var children = element.Nodes().ToList();
        for (var i = 0; i < children.Count; i++)
        {
            AppendNode(children[i], builder);
            if (i < children.Count - 1)
            {
                builder.AppendLine();
            }
        }
    }

    internal static bool TryConvertToRenderable(XElement element, out IRenderable? renderable)
    {
        foreach (var converter in RenderableConverters)
        {
            if (converter.TryConvert(element, out var candidate))
            {
                renderable = candidate;
                return true;
            }
        }

        renderable = null;
        return false;
    }

    private static bool TryConvertUsingMarkupConverters(XElement element, StringBuilder builder)
    {
        foreach (var converter in MarkupConverters)
        {
            if (converter.TryConvert(element, out var markup))
            {
                builder.Append(markup);
                return true;
            }
        }

        return false;
    }
}
