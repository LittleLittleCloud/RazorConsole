using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal interface IRenderableConverter
{
    bool TryConvert(XElement element, out ComponentRenderable renderable);
}
