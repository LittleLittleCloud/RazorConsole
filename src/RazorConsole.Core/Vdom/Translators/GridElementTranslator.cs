using System;
using System.Collections.Generic;
using System.Composition;
using RazorConsole.Core.Vdom;
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

            if (!node.Attributes.TryGetValue("class", out var value) || !string.Equals(value, "grid", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!TryConvertChildrenToRenderables(node.Children, context, out var children))
            {
                return false;
            }

            var columnCount = TryGetIntAttribute(node, "data-columns", 2);
            var isExpanded = !string.Equals(GetAttribute(node, "data-expand"), "false", StringComparison.OrdinalIgnoreCase);
            var grid = new Grid()
            {
                Expand = isExpanded
            };

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

            if (TryParsePositiveInt(GetAttribute(node, "data-width"), out var width))
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
