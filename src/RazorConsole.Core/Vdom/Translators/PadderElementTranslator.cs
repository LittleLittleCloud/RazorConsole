// Copyright (c) RazorConsole. All rights reserved.

using System.Globalization;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class PadderElementTranslator : IVdomElementTranslator
{
    public int Priority => 140;

    private static readonly char[] PaddingSeparators = [',', ' '];

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("class", out var value) || !string.Equals(value, "padder", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        var content = VdomSpectreTranslator.ComposeChildContent(children);
        var padding = ParsePadding(VdomSpectreTranslator.GetAttribute(node, "data-padding"));
        var padder = new Padder(content, padding);

        renderable = padder;
        return true;
    }

    private static Padding ParsePadding(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new Padding(0, 0, 0, 0);
        }

        var parts = raw.Split(PaddingSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var values = parts
            .Select(part => int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? Math.Max(number, 0) : 0)
            .Take(4)
            .ToArray();

        return values.Length switch
        {
            0 => new Padding(0, 0, 0, 0),
            1 => new Padding(values[0], values[0], values[0], values[0]),
            2 => new Padding(values[0], values[1], values[0], values[1]),
            3 => new Padding(values[0], values[1], values[2], values[1]),
            4 => new Padding(values[0], values[1], values[2], values[3]),
            _ => new Padding(0, 0, 0, 0),
        };
    }
}
