using System;
using System.Collections.Generic;
using System.Composition;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class GridElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!node.Attributes.TryGetValue("data-grid", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var columnCount = Math.Clamp(TryGetIntAttribute(node, "data-columns", 2), 1, 4);
            var grid = new Grid();
            for (var i = 0; i < columnCount; i++)
            {
                grid.AddColumn();
            }

            if (children.Count > 0)
            {
                foreach (var row in Chunk(children, columnCount))
                {
                    grid.AddRow(row);
                }
            }

            if (string.Equals(GetAttribute(node, "data-grid-expand") ?? GetAttribute(node, "data-expand"), "true", StringComparison.OrdinalIgnoreCase))
            {
                grid.Expand();
            }

            if (TryParsePositiveInt(GetAttribute(node, "data-grid-width") ?? GetAttribute(node, "data-width"), out var width))
            {
                grid.Width = width;
            }

            renderable = grid;
            return true;
        }

        private static IEnumerable<IRenderable[]> Chunk(IReadOnlyList<IRenderable> items, int size)
        {
            var buffer = new List<IRenderable>(size);
            foreach (var item in items)
            {
                buffer.Add(item);
                if (buffer.Count == size)
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                while (buffer.Count < size)
                {
                    buffer.Add(new Markup(string.Empty));
                }

                yield return buffer.ToArray();
            }
        }
    }
}
