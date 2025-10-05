using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using static RazorConsole.Core.Rendering.Vdom.VdomSpectreTranslator;

namespace RazorConsole.Core.Rendering.Vdom;


internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class FigletElementTranslator : IVdomElementTranslator
    {
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

            var content = GetAttribute(node, "data-content");

            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            var justifyAttribution = GetAttribute(node, "data-justify");
            var justify = (justifyAttribution?.ToLowerInvariant()) switch
            {
                "left" => Justify.Left,
                "right" => Justify.Right,
                "center" => Justify.Center,
                _ => Justify.Left,
            };
            var figlet = new FigletText(content)
            {
                Justification = justify,
            };

            renderable = figlet;
            return true;
        }
    }
}
