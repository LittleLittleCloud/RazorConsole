using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;

namespace RazorConsole.Core.Rendering;

public static class HtmlToSpectreMarkupConverter
{
    private static readonly IReadOnlyList<IComponentMarkupConverter> ComponentConverters = new IComponentMarkupConverter[]
    {
        new TextComponentMarkupConverter(),
        new NewlineComponentMarkupConverter(),
        new SpacerComponentMarkupConverter(),
        new SpinnerComponentMarkupConverter(),
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
        if (TryConvertUsingComponentConverters(element, builder))
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

    internal static bool TryConvertToRenderable(XElement element, out ComponentRenderable renderable)
    {
        foreach (var converter in ComponentConverters)
        {
            if (converter.TryConvert(element, out renderable))
            {
                return true;
            }
        }

        renderable = default;
        return false;
    }

    private static bool TryConvertUsingComponentConverters(XElement element, StringBuilder builder)
    {
        if (TryConvertToRenderable(element, out var renderable))
        {
            builder.Append(renderable.Markup);
            return true;
        }

        return false;
    }
}
