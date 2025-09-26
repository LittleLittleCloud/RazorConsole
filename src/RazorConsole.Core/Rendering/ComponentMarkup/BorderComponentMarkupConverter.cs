using System;
using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[Obsolete("Border conversion is handled by SpectreRenderableFactory.")]
internal sealed class BorderComponentMarkupConverter : IComponentMarkupConverter
{
    public bool TryConvert(XElement element, out ComponentRenderable renderable)
    {
        renderable = default;
        _ = element;
        return false;
    }
}
