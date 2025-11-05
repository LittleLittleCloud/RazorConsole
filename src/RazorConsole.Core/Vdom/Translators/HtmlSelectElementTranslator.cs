using System;
using System.Collections.Generic;
using System.Linq;
using RazorConsole.Core.Renderables;
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

        // Build rows with all options displayed
        var optionRenderables = new List<IRenderable>();

        if (options.Count == 0)
        {
            // Show empty state
            var emptyText = VdomSpectreTranslator.GetAttribute(node, "data-empty-label") ?? "No options available";
            var emptyStyle = VdomSpectreTranslator.GetAttribute(node, "data-empty-color") ?? "grey70";
            var emptyMarkup = ComponentMarkupUtilities.CreateStyledMarkup($"{emptyStyle} italic", emptyText, requiresEscape: true);
            optionRenderables.Add(new Markup(emptyMarkup));
        }
        else
        {
            foreach (var option in options)
            {
                var isSelected = !string.IsNullOrEmpty(selectedValue) &&
                                string.Equals(option.Value, selectedValue, StringComparison.Ordinal);

                // Determine indicator and style
                var indicator = isSelected ? "> " : "  ";
                var optionStyle = isSelected
                    ? VdomSpectreTranslator.GetAttribute(node, "data-selected-color") ?? "chartreuse1"
                    : VdomSpectreTranslator.GetAttribute(node, "data-option-color") ?? "white";

                if (isSelected)
                {
                    optionStyle = $"{optionStyle} bold";
                }

                var optionText = $"{indicator}{option.Label}";
                var optionMarkup = ComponentMarkupUtilities.CreateStyledMarkup(optionStyle, optionText, requiresEscape: true);
                optionRenderables.Add(new Markup(optionMarkup));
            }
        }

        // Create a Rows renderable with all options
        renderable = new Rows(optionRenderables);
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
