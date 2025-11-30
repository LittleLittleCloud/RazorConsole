// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Extensions;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class GridElementTranslator : IVdomElementTranslator
{
    public int Priority => 130;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(RazorConsole.Components.Grid))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        var columnCount = node.GetAttributeValue(nameof(RazorConsole.Components.Grid.Columns), 2);
        var expand = node.GetAttributeValue(nameof(RazorConsole.Components.Grid.Expand), false);
        var grid = new Grid()
        {
            Expand = expand
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

        grid.Width = node.GetAttributeValue<int?>(nameof(RazorConsole.Components.Grid.Width));

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
