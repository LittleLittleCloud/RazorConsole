using System;
using System.Composition;
using System.Globalization;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class ColumnsElementTranslator : IVdomElementTranslator
    {
        private static readonly char[] PaddingSeparators = [',', ' '];

        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!IsColumnsNode(node))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var columns = new Columns(children);

            if (string.Equals(GetAttribute(node, "data-columns-expand") ?? GetAttribute(node, "data-expand"), "true", StringComparison.OrdinalIgnoreCase))
            {
                columns = columns.Expand();
            }

            var paddingValue = GetAttribute(node, "data-columns-padding") ?? GetAttribute(node, "data-padding");
            var spacing = Math.Max(TryGetIntAttribute(node, "data-spacing", 0), 0);

            IRenderable result = columns;
            if (!string.IsNullOrWhiteSpace(paddingValue) && TryParsePadding(paddingValue, out var padding))
            {
                result = new Padder(columns, padding);
            }
            else if (spacing > 0)
            {
                result = new Padder(columns, new Padding(spacing, 0, spacing, 0));
            }

            renderable = result;
            return true;
        }

        private static bool IsColumnsNode(VNode node)
        {
            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (node.Attributes.TryGetValue("data-columns-layout", out var layoutValue) && string.Equals(layoutValue, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (node.Attributes.TryGetValue("data-columns", out var value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool TryParsePadding(string raw, out Padding padding)
        {
            var parts = raw.Split(PaddingSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var values = parts
                .Select(part => int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? Math.Max(number, 0) : 0)
                .Take(4)
                .ToArray();

            padding = values.Length switch
            {
                0 => new Padding(0, 0, 0, 0),
                1 => new Padding(values[0], values[0], values[0], values[0]),
                2 => new Padding(values[0], values[1], values[0], values[1]),
                3 => new Padding(values[0], values[1], values[2], values[1]),
                4 => new Padding(values[0], values[1], values[2], values[3]),
                _ => new Padding(0, 0, 0, 0),
            };

            return true;
        }
    }
}
