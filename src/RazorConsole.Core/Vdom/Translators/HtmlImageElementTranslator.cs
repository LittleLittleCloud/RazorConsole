using System;
using System.Composition;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

internal sealed partial class VdomSpectreTranslator
{
    [Export(typeof(IVdomElementTranslator))]
    internal sealed class HtmlImageElementTranslator : IVdomElementTranslator
    {
        public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
        {
            renderable = null;

            if (node.Kind != VNodeKind.Element)
            {
                return false;
            }

            if (!string.Equals(node.TagName, "img", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var src = GetAttribute(node, "src");
            if (string.IsNullOrWhiteSpace(src))
            {
                return false;
            }

            try
            {
                var image = new CanvasImage(src);

                // Apply width if specified
                if (TryParsePositiveInt(GetAttribute(node, "data-width"), out var width))
                {
                    image.MaxWidth = width;
                }

                // Apply height if specified - note: CanvasImage doesn't have direct height control
                // The aspect ratio is preserved when MaxWidth is set

                renderable = image;
                return true;
            }
            catch
            {
                // If image loading fails, return false to let other translators handle it
                return false;
            }
        }
    }
}
