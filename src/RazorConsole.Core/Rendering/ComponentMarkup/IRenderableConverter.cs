using System.Xml.Linq;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal interface IRenderableConverter
{
    bool TryConvert(XElement element, out IRenderable renderable);
}

internal interface IMarkupConverter
{
    bool TryConvert(XElement element, out string markup);
}
