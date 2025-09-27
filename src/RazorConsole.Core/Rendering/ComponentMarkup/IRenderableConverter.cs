using System.Xml.Linq;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

public interface IRenderableConverter
{
    bool TryConvert(XElement element, out IRenderable renderable);
}

public class RenderableConverterMetadata
{
    public string? ConverterType { get; }
}

internal interface IMarkupConverter
{
    bool TryConvert(XElement element, out string markup);
}
