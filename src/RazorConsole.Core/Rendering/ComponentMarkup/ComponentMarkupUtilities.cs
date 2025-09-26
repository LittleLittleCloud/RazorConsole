using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal static class ComponentMarkupUtilities
{
    public static int GetIntAttribute(XElement element, string attributeName, int fallback)
    {
        var raw = element.Attribute(attributeName)?.Value;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    public static void AppendStyledContent(StringBuilder builder, string? style, string content, bool requiresEscape)
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

    public static string ResolveSpinnerGlyph(string? spinnerType)
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
