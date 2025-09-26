using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class BorderComponentMarkupConverter : IComponentMarkupConverter
{
    public bool TryConvert(XElement element, StringBuilder builder)
    {
        if (!IsBorderComponent(element))
        {
            return false;
        }

        var innerMarkup = HtmlToSpectreMarkupConverter.ConvertNodes(element.Nodes());
        var lines = SplitLines(innerMarkup);
        if (lines.Length == 0)
        {
            lines = new[] { string.Empty };
        }

        var header = element.Attribute("data-header")?.Value;
        var headerColor = element.Attribute("data-header-color")?.Value;
        var headerMarkup = CreateHeaderMarkup(header, headerColor);

        var borderStyle = element.Attribute("data-border-color")?.Value;
        if (string.IsNullOrWhiteSpace(borderStyle))
        {
            borderStyle = "grey53";
        }

        var width = CalculateContentWidth(lines, headerMarkup);
        AppendBorderTop(builder, borderStyle, width);

        if (headerMarkup is not null)
        {
            AppendBorderContent(builder, borderStyle, headerMarkup, width, center: true);
        }

        foreach (var line in lines)
        {
            AppendBorderContent(builder, borderStyle, line, width, center: false);
        }

        AppendBorderBottom(builder, borderStyle, width);
        return true;
    }

    private static bool IsBorderComponent(XElement element)
        => string.Equals(element.Name.LocalName, "div", StringComparison.OrdinalIgnoreCase)
           && string.Equals(element.Attribute("data-border")?.Value, "panel", StringComparison.OrdinalIgnoreCase);

    private static string[] SplitLines(string markup)
        => markup.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

    private static string? CreateHeaderMarkup(string? header, string? headerColor)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        var escapedHeader = Markup.Escape(header);
        if (string.IsNullOrWhiteSpace(headerColor))
        {
            return escapedHeader;
        }

        return $"[{headerColor}]{escapedHeader}[/]";
    }

    private static int CalculateContentWidth(string[] lines, string? headerMarkup)
    {
        var max = lines.Length == 0 ? 0 : lines.Max(line => Markup.Remove(line ?? string.Empty).Length);
        if (headerMarkup is not null)
        {
            max = Math.Max(max, Markup.Remove(headerMarkup).Length);
        }

        return Math.Max(max, 0);
    }

    private static void AppendBorderTop(StringBuilder builder, string borderStyle, int width)
    {
        var top = $"╭{new string('─', width + 2)}╮";
        AppendStyled(builder, borderStyle, top);
        builder.AppendLine();
    }

    private static void AppendBorderBottom(StringBuilder builder, string borderStyle, int width)
    {
        var bottom = $"╰{new string('─', width + 2)}╯";
        AppendStyled(builder, borderStyle, bottom);
        builder.AppendLine();
    }

    private static void AppendBorderContent(StringBuilder builder, string borderStyle, string content, int width, bool center)
    {
        content ??= string.Empty;
        var plainLength = Markup.Remove(content).Length;
        var padding = Math.Max(width - plainLength, 0);

        int leftPadding;
        int rightPadding;
        if (center)
        {
            leftPadding = padding / 2;
            rightPadding = padding - leftPadding;
        }
        else
        {
            leftPadding = 0;
            rightPadding = padding;
        }

        var lineBuilder = new StringBuilder();
        lineBuilder.Append("│ ");
        lineBuilder.Append(new string(' ', leftPadding));
        lineBuilder.Append(content);
        lineBuilder.Append(new string(' ', rightPadding));
        lineBuilder.Append(" │");

        AppendStyled(builder, borderStyle, lineBuilder.ToString());
        builder.AppendLine();
    }

    private static void AppendStyled(StringBuilder builder, string style, string content)
    {
        if (string.IsNullOrWhiteSpace(style))
        {
            builder.Append(content);
            return;
        }

        ComponentMarkupUtilities.AppendStyledContent(builder, style, content, requiresEscape: false);
    }
}
