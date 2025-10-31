using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;


namespace RazorConsole.Core.Rendering.Vdom;

public sealed class CanvasElementTranslator : IVdomElementTranslator
{
    public int Priority => 100;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;


        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-canvas", out var canvasAttr)
            || !string.Equals(canvasAttr, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var width = GetIntAttribute(node, "data-width");

        var height = GetIntAttribute(node, "data-height");

        var pixelWidth = GetIntAttribute(node, "data-pixelwidth", 2);

        var scale = GetBoolAttribute(node, "data-scale", false);

        var maxWidth = GetNullableIntAttribute(node, "data-maxwidth");

        var pixelsDataIdAttribute = VdomSpectreTranslator.GetAttribute(node, "data-canvas-data-id");

        var canvas = new Canvas(width, height);


        if (maxWidth.HasValue)
            canvas.MaxWidth = maxWidth.Value;

        canvas.PixelWidth = pixelWidth;


        canvas.Scale = scale;


        if (!string.IsNullOrWhiteSpace(pixelsDataIdAttribute) &&
            Guid.TryParse(pixelsDataIdAttribute, out var dataId))
        {
            var dataInDictionary = CanvasDataRegistry.TryGetData(dataId, out var pixels);

            if (!dataInDictionary)
                return false;

            foreach (var p in pixels!)


            {
                canvas.SetPixel(p.Item1, p.Item2, p.Item3);
            }
        }
        else
            return false;

        renderable = canvas;
        return true;
    }


    private static int GetIntAttribute(VNode node, string name, int? defaultValue = null)
    {
        if (node.Attributes.TryGetValue(name, out var value)
            && int.TryParse(value, out var result))
            return result;

        if (defaultValue.HasValue)
            return defaultValue.Value;

        throw new InvalidOperationException($"Required canvas attribute '{name}' is missing or has an invalid value.");
    }


    private static bool GetBoolAttribute(VNode node, string name, bool defaultValue)
    {
        if (node.Attributes.TryGetValue(name, out var value))
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        return defaultValue;
    }


    private static int? GetNullableIntAttribute(VNode node, string name)
    {
        if (node.Attributes.TryGetValue(name, out var value)
            && int.TryParse(value, out var result))
        {
            return result;
        }

        return null;
    }
}
