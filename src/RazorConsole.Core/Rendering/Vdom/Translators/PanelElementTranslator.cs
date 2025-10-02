using System;
using System.Collections.Generic;
using System.Composition;
using RazorConsole.Core.Rendering.ComponentMarkup;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class PanelElementTranslator : IVdomElementTranslator
    {
        private static readonly IReadOnlyDictionary<string, BoxBorder> BorderLookup = new Dictionary<string, BoxBorder>(StringComparer.OrdinalIgnoreCase)
        {
            { "square", BoxBorder.Square },
            { "rounded", BoxBorder.Rounded },
            { "double", BoxBorder.Double },
            { "heavy", BoxBorder.Heavy },
            { "ascii", BoxBorder.Ascii },
            { "none", BoxBorder.None },
        };

        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!IsPanelNode(node))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var content = ComposeChildContent(children);
            var panel = new Panel(content);

            if (ShouldExpand(node))
            {
                panel = panel.Expand();
            }

            panel.Border = ResolveBorder(GetAttribute(node, "data-panel-border"));

            if (TryParsePadding(GetAttribute(node, "data-panel-padding"), out var padding))
            {
                panel.Padding = padding;
            }

            if (TryParsePositiveInt(GetAttribute(node, "data-panel-height"), out var height))
            {
                panel.Height = height;
            }

            if (TryParsePositiveInt(GetAttribute(node, "data-panel-width"), out var width))
            {
                panel.Width = width;
            }

            ApplyHeader(node, panel);
            ApplyBorderColor(node, panel);

            renderable = panel;
            return true;
        }

        private static bool IsPanelNode(VNode node)
        {
            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (node.Attributes.TryGetValue("data-panel", out var value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (node.Attributes.TryGetValue("data-border", out var borderValue) && string.Equals(borderValue, "panel", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool ShouldExpand(VNode node)
        {
            var value = GetAttribute(node, "data-panel-expand") ?? GetAttribute(node, "data-expand");
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static BoxBorder ResolveBorder(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return BoxBorder.Square;
            }

            return BorderLookup.TryGetValue(value, out var border) ? border : BoxBorder.Square;
        }

        private static void ApplyHeader(VNode node, Panel panel)
        {
            var header = GetAttribute(node, "data-header");
            if (string.IsNullOrWhiteSpace(header))
            {
                return;
            }

            var headerColor = GetAttribute(node, "data-header-color");
            var markup = string.IsNullOrWhiteSpace(headerColor)
                ? Markup.Escape(header)
                : ComponentMarkupUtilities.CreateStyledMarkup(headerColor, header, requiresEscape: true);

            panel.Header = new PanelHeader(markup);
        }

        private static void ApplyBorderColor(VNode node, Panel panel)
        {
            var borderColorValue = GetAttribute(node, "data-border-color");
            if (string.IsNullOrWhiteSpace(borderColorValue))
            {
                return;
            }

            try
            {
                var style = Style.Parse(borderColorValue);
                panel.BorderStyle(style);
            }
            catch (Exception)
            {
                // Ignore invalid style specifications.
            }
        }
    }
}
