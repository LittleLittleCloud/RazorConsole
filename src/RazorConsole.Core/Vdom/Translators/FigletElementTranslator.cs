// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Extensions;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class FigletElementTranslator : IVdomElementTranslator
{
    public int Priority => 160;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(RazorConsole.Components.Figlet))
        {
            return false;
        }

        if (!node.TryGetAttributeValue<string>(nameof(RazorConsole.Components.Figlet.Content), out var content) || string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        var justify = node.GetAttributeValue(nameof(RazorConsole.Components.Figlet.Justify), Justify.Center);
        var color = node.GetAttributeValue(nameof(RazorConsole.Components.Figlet.Color), Color.Default);

        var figlet = new FigletText(content)
        {
            Justification = justify,
            Color = color
        };

        renderable = figlet;
        return true;
    }
}
