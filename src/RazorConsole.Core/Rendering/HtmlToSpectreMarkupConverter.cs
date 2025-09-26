using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Spectre.Console;

namespace RazorConsole.Core.Rendering;

public static class HtmlToSpectreMarkupConverter
{
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
                if (TryHandleStructuredBlock(element, builder))
                {
                    break;
                }

                AppendChildrenWithSpacing(element, builder);
                break;
            case "span":
                if (TryHandleTextSpan(element, builder))
                {
                    break;
                }

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

    private static bool TryHandleTextSpan(XElement element, StringBuilder builder)
    {
        if (element.Attribute("data-text") is null)
        {
            return false;
        }

        var style = element.Attribute("data-style")?.Value;
        var isMarkupValue = element.Attribute("data-ismarkup")?.Value;
        var isMarkup = bool.TryParse(isMarkupValue, out var parsed) && parsed;

        var content = element.Value ?? string.Empty;
        AppendStyledContent(builder, style, content, requiresEscape: !isMarkup);

        return true;
    }

    private static bool TryHandleStructuredBlock(XElement element, StringBuilder builder)
    {
        if (element.Attribute("data-newline") is not null)
        {
            var count = Math.Max(GetIntAttribute(element, "data-count", 1), 0);
            for (var i = 0; i < count; i++)
            {
                builder.AppendLine();
            }

            return true;
        }

        if (element.Attribute("data-spacer") is not null)
        {
            var lines = Math.Max(GetIntAttribute(element, "data-lines", 1), 0);
            var fill = element.Attribute("data-fill")?.Value;

            if (lines == 0)
            {
                return true;
            }

            if (string.IsNullOrEmpty(fill))
            {
                for (var i = 0; i < lines; i++)
                {
                    builder.AppendLine();
                }
            }
            else
            {
                var fillChar = fill[0].ToString();
                for (var i = 0; i < lines; i++)
                {
                    AppendStyledContent(builder, null, fillChar, requiresEscape: true);
                    builder.AppendLine();
                }
            }

            return true;
        }

        if (element.Attribute("data-spinner") is not null)
        {
            var message = element.Attribute("data-message")?.Value ?? string.Empty;
            var style = element.Attribute("data-style")?.Value;
            var spinnerType = element.Attribute("data-spinner-type")?.Value;

            var glyph = ResolveSpinnerGlyph(spinnerType);
            var content = string.IsNullOrWhiteSpace(message)
                ? glyph
                : string.Concat(glyph, " ", message);

            AppendStyledContent(builder, style, content, requiresEscape: true);
            return true;
        }

        return false;
    }

    private static void AppendStyledContent(StringBuilder builder, string? style, string content, bool requiresEscape)
    {
        if (requiresEscape)
        {
            content = Markup.Escape(content);
        }

        if (string.IsNullOrWhiteSpace(style))
        {
            builder.Append(content);
            return;
        }

        builder.Append('[');
        builder.Append(style);
        builder.Append(']');
        builder.Append(content);
        builder.Append("[/]");
    }

    private static int GetIntAttribute(XElement element, string attributeName, int fallback)
    {
        var raw = element.Attribute(attributeName)?.Value;
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return fallback;
    }

    private static string ResolveSpinnerGlyph(string? spinnerType)
    {
        static string FirstFrame(Spinner spinner)
            => spinner.Frames?.FirstOrDefault() ?? "⠋";

        if (!string.IsNullOrWhiteSpace(spinnerType))
        {
            var property = typeof(Spinner.Known)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(p => string.Equals(p.Name, spinnerType, StringComparison.OrdinalIgnoreCase));

            if (property?.GetValue(null) is Spinner spinner)
            {
                return FirstFrame(spinner);
            }
        }

        return FirstFrame(Spinner.Known.Dots);
    }
}
