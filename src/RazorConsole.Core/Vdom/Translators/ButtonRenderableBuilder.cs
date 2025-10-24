using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal static class ButtonRenderableBuilder
{
    private static readonly Padding DefaultPadding = new(1, 0, 1, 0);

    private static readonly IReadOnlyDictionary<string, ButtonStylePreset> VariantStyles = new Dictionary<string, ButtonStylePreset>(StringComparer.OrdinalIgnoreCase)
    {
        ["neutral"] = new ButtonStylePreset("grey93 on grey23", "grey70"),
        ["primary"] = new ButtonStylePreset("black on dodgerblue2", "dodgerblue2"),
        ["success"] = new ButtonStylePreset("black on chartreuse3", "chartreuse3"),
        ["warning"] = new ButtonStylePreset("black on lightgoldenrod3", "lightgoldenrod3"),
        ["danger"] = new ButtonStylePreset("white on red3", "red3"),
    };

    public static IRenderable Build(ButtonRenderableDescriptor descriptor, IRenderable content)
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        var border = descriptor.IsActive
            ? BoxBorder.Double
            : descriptor.IsDefault ? BoxBorder.Heavy : BoxBorder.Rounded;

        var panel = new Panel(content)
        {
            Border = border,
            Padding = descriptor.Padding ?? DefaultPadding,
        };

        if (descriptor.Expand)
        {
            panel = panel.Expand();
        }

        if (descriptor.Width is > 0)
        {
            panel.Width = descriptor.Width.Value;
        }

        if (TryParseStyle(ResolveBorderStyle(descriptor), out var borderStyle))
        {
            panel.BorderStyle(borderStyle);
        }

        return panel;
    }

    private static IRenderable ComposeContent(ButtonRenderableDescriptor descriptor, IReadOnlyList<IRenderable> children)
    {
        if (children.Count > 0)
        {
            return children.Count == 1 ? children[0] : new Rows(children);
        }

        var text = BuildDisplayText(descriptor);
        var style = ResolveContentStyle(descriptor);
        var markup = ComponentMarkupUtilities.CreateStyledMarkup(style, text, requiresEscape: true);
        return new Markup(markup);
    }

    private static string BuildDisplayText(ButtonRenderableDescriptor descriptor)
    {
        var segments = new List<string>(3);

        if (!string.IsNullOrWhiteSpace(descriptor.Icon))
        {
            segments.Add(descriptor.Icon!);
        }

        if (!string.IsNullOrWhiteSpace(descriptor.Label))
        {
            segments.Add(descriptor.Label!);
        }

        var text = string.Join(' ', segments);

        if (!string.IsNullOrWhiteSpace(descriptor.HotKey))
        {
            var hotKey = descriptor.HotKey!;
            text = string.IsNullOrWhiteSpace(text)
                ? hotKey
                : string.Concat(text, " (", hotKey, ")");
        }
        else if (descriptor.IsDefault)
        {
            text = string.IsNullOrWhiteSpace(text)
                ? "Enter"
                : string.Concat(text, " (Enter)");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return "Button";
        }

        return text;
    }

    private static string? ResolveContentStyle(ButtonRenderableDescriptor descriptor)
    {
        var preset = GetVariantPreset(descriptor.Variant);

        var baseStyle = !string.IsNullOrWhiteSpace(descriptor.Style)
            ? descriptor.Style
            : preset.ContentStyle;

        if (descriptor.Disabled)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.DisabledStyle))
            {
                return descriptor.DisabledStyle;
            }

            return string.IsNullOrWhiteSpace(baseStyle)
                ? "grey58 on grey19"
                : AppendModifier(baseStyle, "dim");
        }

        if (descriptor.IsActive)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.ActiveStyle))
            {
                return descriptor.ActiveStyle;
            }

            return string.IsNullOrWhiteSpace(baseStyle)
                ? "black on gold3"
                : AppendModifier(baseStyle, "bold");
        }

        return baseStyle;
    }

    private static string? ResolveBorderStyle(ButtonRenderableDescriptor descriptor)
    {
        var preset = GetVariantPreset(descriptor.Variant);

        var baseStyle = !string.IsNullOrWhiteSpace(descriptor.BorderStyle)
            ? descriptor.BorderStyle
            : preset.BorderStyle;

        if (descriptor.Disabled)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.DisabledBorderStyle))
            {
                return descriptor.DisabledBorderStyle;
            }

            return string.IsNullOrWhiteSpace(baseStyle)
                ? "grey39"
                : AppendModifier(baseStyle, "dim");
        }

        if (descriptor.IsActive)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.ActiveBorderStyle))
            {
                return descriptor.ActiveBorderStyle;
            }

            return string.IsNullOrWhiteSpace(baseStyle)
                ? "gold3"
                : AppendModifier(baseStyle, "bold");
        }

        if (descriptor.IsDefault && string.IsNullOrWhiteSpace(baseStyle))
        {
            return "slateblue3";
        }

        return baseStyle;
    }

    private static string AppendModifier(string style, string modifier)
    {
        if (style.Contains(modifier, StringComparison.OrdinalIgnoreCase))
        {
            return style;
        }

        return string.Concat(style, " ", modifier);
    }

    private static ButtonStylePreset GetVariantPreset(string? variant)
    {
        if (!string.IsNullOrWhiteSpace(variant) && VariantStyles.TryGetValue(variant, out var preset))
        {
            return preset;
        }

        return VariantStyles["neutral"];
    }

    private static bool TryParseStyle(string? raw, out Style style)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            style = default!;
            return false;
        }

        try
        {
            style = Style.Parse(raw);
            return true;
        }
        catch
        {
            style = default!;
            return false;
        }
    }

    private sealed record ButtonStylePreset(string? ContentStyle, string? BorderStyle);
}

internal static class ButtonRenderableDescriptorFactory
{
    public static ButtonRenderableDescriptor Create(Func<string, string?> getAttribute)
    {
        if (getAttribute is null)
        {
            throw new ArgumentNullException(nameof(getAttribute));
        }

        return new ButtonRenderableDescriptor(
            Label: EmptyOrNull(getAttribute("data-button-label")),
            Icon: EmptyOrNull(getAttribute("data-button-icon")),
            HotKey: EmptyOrNull(getAttribute("data-button-hotkey")),
            Variant: EmptyOrNull(getAttribute("data-button-variant")),
            Style: EmptyOrNull(getAttribute("data-button-style")),
            ActiveStyle: EmptyOrNull(getAttribute("data-button-active-style")),
            DisabledStyle: EmptyOrNull(getAttribute("data-button-disabled-style")),
            BorderStyle: EmptyOrNull(getAttribute("data-button-border-style")),
            ActiveBorderStyle: EmptyOrNull(getAttribute("data-button-active-border-style")),
            DisabledBorderStyle: EmptyOrNull(getAttribute("data-button-disabled-border-style")),
            IsActive: ParseBool(getAttribute("data-button-active")),
            Disabled: ParseBool(getAttribute("data-button-disabled")),
            Expand: ParseBool(getAttribute("data-button-expand")),
            Width: ParsePositiveInt(getAttribute("data-button-width")),
            Padding: ParsePadding(getAttribute("data-button-padding")),
            IsDefault: ParseBool(getAttribute("data-button-default")));
    }

    private static string? EmptyOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;

    private static bool ParseBool(string? value)
        => !string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out var parsed) && parsed;

    private static int? ParsePositiveInt(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        return null;
    }

    private static Padding? ParsePadding(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        var values = parts
            .Select(part => int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? Math.Max(number, 0) : 0)
            .Take(4)
            .ToArray();

        return values.Length switch
        {
            1 => new Padding(values[0], values[0], values[0], values[0]),
            2 => new Padding(values[0], values[1], values[0], values[1]),
            3 => new Padding(values[0], values[1], values[2], values[1]),
            4 => new Padding(values[0], values[1], values[2], values[3]),
            _ => null,
        };
    }
}

internal sealed record ButtonRenderableDescriptor(
    string? Label,
    string? Icon,
    string? HotKey,
    string? Variant,
    string? Style,
    string? ActiveStyle,
    string? DisabledStyle,
    string? BorderStyle,
    string? ActiveBorderStyle,
    string? DisabledBorderStyle,
    bool IsActive,
    bool Disabled,
    bool Expand,
    int? Width,
    Padding? Padding,
    bool IsDefault);
