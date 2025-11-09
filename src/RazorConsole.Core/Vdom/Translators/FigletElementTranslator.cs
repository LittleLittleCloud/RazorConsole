using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using static RazorConsole.Core.Rendering.Vdom.VdomSpectreTranslator;

namespace RazorConsole.Core.Rendering.Vdom;


public sealed class FigletElementTranslator : IVdomElementTranslator
{
    public int Priority => 160;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("class", out var value) || !string.Equals(value, "figlet", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (node.Children is not { Count: 0 })
        {
            return false;
        }

        var content = VdomSpectreTranslator.GetAttribute(node, "data-content");

        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        var styleAttribute = VdomSpectreTranslator.GetAttribute(node, "data-style");
        var style = new Style(Color.Default);
        if (!string.IsNullOrWhiteSpace(styleAttribute))
        {
            style = Style.Parse(styleAttribute);
        }

        var justifyAttribute = VdomSpectreTranslator.GetAttribute(node, "data-justify");
        var justify = (justifyAttribute?.ToLowerInvariant()) switch
        {
            "left" => Justify.Left,
            "right" => Justify.Right,
            "center" => Justify.Center,
            _ => Justify.Left,
        };
        var figlet = new FigletText(content)
        {
            Justification = justify,
            Color = style.Foreground
        };

        renderable = figlet;
        return true;
    }
}
