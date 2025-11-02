using System;
using System.Collections.Generic;
using System.Linq;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class HtmlSelectElementTranslator : IVdomElementTranslator
{
    public int Priority => 80;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element || !string.Equals(node.TagName, "select", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var options = ExtractOptions(node);
        var selectedValue = VdomSpectreTranslator.GetAttribute(node, "value");
        var placeholder = VdomSpectreTranslator.GetAttribute(node, "placeholder") ?? "Select an option";
        var disabled = node.Attributes.ContainsKey("disabled");
        var expand = VdomSpectreTranslator.TryGetBoolAttribute(node, "data-expand", out var expandValue) && expandValue;

        // Determine display value and style
        string displayValue;
        string displayStyle;

        if (!string.IsNullOrEmpty(selectedValue) && options.Any(o => string.Equals(o.Value, selectedValue, StringComparison.Ordinal)))
        {
            var selectedOption = options.First(o => string.Equals(o.Value, selectedValue, StringComparison.Ordinal));
            displayValue = selectedOption.Label;
            var colorAttr = VdomSpectreTranslator.GetAttribute(node, "data-value-color");
            displayStyle = !string.IsNullOrWhiteSpace(colorAttr) ? colorAttr : "white";
        }
        else
        {
            displayValue = placeholder;
            var colorAttr = VdomSpectreTranslator.GetAttribute(node, "data-placeholder-color");
            var baseColor = !string.IsNullOrWhiteSpace(colorAttr) ? colorAttr : "grey70";
            displayStyle = $"{baseColor} italic dim";
        }

        // Build the display markup
        var markupText = ComponentMarkupUtilities.CreateStyledMarkup(displayStyle, displayValue, requiresEscape: true);

        // Determine border color
        var borderColorAttr = disabled
            ? VdomSpectreTranslator.GetAttribute(node, "data-disabled-border-color") ?? "grey19"
            : VdomSpectreTranslator.GetAttribute(node, "data-border-color") ?? "grey37";

        // Determine border style
        var borderStyleAttr = VdomSpectreTranslator.GetAttribute(node, "data-border-style");
        var borderStyle = ResolveBorderStyle(borderStyleAttr);

        // Create panel
        var panel = new Panel(new Markup(markupText))
        {
            Border = borderStyle,
            Padding = new Padding(0, 0, 0, 0),
        };

        // Apply border color
        if (!string.IsNullOrWhiteSpace(borderColorAttr))
        {
            try
            {
                var style = Style.Parse(borderColorAttr);
                panel.BorderStyle(style);
            }
            catch (Exception)
            {
                // Ignore invalid style specifications
            }
        }

        if (expand)
        {
            panel = panel.Expand();
        }

        renderable = panel;
        return true;
    }

    private static List<SelectOption> ExtractOptions(VNode node)
    {
        var options = new List<SelectOption>();

        foreach (var child in node.Children)
        {
            if (child.Kind == VNodeKind.Element && string.Equals(child.TagName, "option", StringComparison.OrdinalIgnoreCase))
            {
                var value = VdomSpectreTranslator.GetAttribute(child, "value") ?? VdomSpectreTranslator.CollectInnerText(child);
                var label = VdomSpectreTranslator.CollectInnerText(child) ?? value ?? string.Empty;
                var disabled = child.Attributes.ContainsKey("disabled");

                if (!string.IsNullOrEmpty(value))
                {
                    options.Add(new SelectOption(value, label, disabled));
                }
            }
        }

        return options;
    }

    private static BoxBorder ResolveBorderStyle(string? borderStyleAttr)
    {
        if (string.IsNullOrWhiteSpace(borderStyleAttr))
        {
            return BoxBorder.Rounded;
        }

        return borderStyleAttr.ToLowerInvariant() switch
        {
            "none" => BoxBorder.None,
            "square" => BoxBorder.Square,
            "rounded" => BoxBorder.Rounded,
            "heavy" => BoxBorder.Heavy,
            "double" => BoxBorder.Double,
            "ascii" => BoxBorder.Ascii,
            _ => BoxBorder.Rounded,
        };
    }

    private readonly struct SelectOption
    {
        public SelectOption(string value, string label, bool disabled)
        {
            Value = value;
            Label = label;
            Disabled = disabled;
        }

        public string Value { get; }
        public string Label { get; }
        public bool Disabled { get; }
    }
}
