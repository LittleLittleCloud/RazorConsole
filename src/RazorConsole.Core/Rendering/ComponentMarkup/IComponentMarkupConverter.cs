using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal interface IComponentMarkupConverter
{
    bool TryConvert(XElement element, out ComponentRenderable renderable);
}
