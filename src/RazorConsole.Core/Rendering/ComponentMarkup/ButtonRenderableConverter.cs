using System;
using System.Linq;
using System.Xml.Linq;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[RenderableConverterExport(typeof(ButtonRenderableConverter))]
public sealed class ButtonRenderableConverter : IRenderableConverter
{
    public bool TryConvert(XElement element, out IRenderable renderable)
    {
        if (!IsButtonElement(element))
        {
            renderable = default!;
            return false;
        }

        var descriptor = ButtonRenderableDescriptorFactory.Create(name => element.Attribute(name)?.Value);
        var children = LayoutRenderableUtilities.ConvertChildNodesToRenderables(element.Nodes()).ToList();
        renderable = ButtonRenderableBuilder.Build(descriptor, children);
        return true;
    }

    private static bool IsButtonElement(XElement element)
        => string.Equals(element.Attribute("data-button")?.Value, "true", StringComparison.OrdinalIgnoreCase);
}
