using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup
{
    internal sealed class RowsRenderableConverter : IRenderableConverter
    {
        public bool TryConvert(XElement element, out IRenderable renderable)
        {
            if (!IsRowsElement(element))
            {
                renderable = default!;
                return false;
            }

            var gap = Math.Max(ComponentMarkupUtilities.GetIntAttribute(element, "data-gap", 0), 0);
            var separator = element.Attribute("data-separator")?.Value;
            var children = LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()).ToList();

            if (children.Count == 0)
            {
                renderable = new Rows(Array.Empty<IRenderable>());
                return true;
            }

            var spacedChildren = ApplySpacing(children, gap, separator).ToArray();
            var rows = new Rows(spacedChildren);

            if (string.Equals(element.Attribute("data-expand")?.Value, "true", StringComparison.OrdinalIgnoreCase))
            {
                rows.Expand();
            }

            renderable = rows;
            return true;
        }

        private static IEnumerable<IRenderable> ApplySpacing(IReadOnlyList<IRenderable> children, int gap, string? separator)
        {
            if (children.Count == 0)
            {
                yield break;
            }

            for (var i = 0; i < children.Count; i++)
            {
                if (i > 0)
                {
                    if (!string.IsNullOrWhiteSpace(separator))
                    {
                        yield return new Markup(separator);
                    }

                    if (gap > 0)
                    {
                        var newlineBuilder = string.Concat(Enumerable.Repeat(Environment.NewLine, gap));
                        if (newlineBuilder.Length > 0)
                        {
                            yield return new Markup(newlineBuilder);
                        }
                    }
                }

                yield return children[i];
            }
        }

        private static bool IsRowsElement(XElement element)
            => string.Equals(element.Attribute("data-rows")?.Value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
