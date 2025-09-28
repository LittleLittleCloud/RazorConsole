using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.Vdom;

internal static class HtmlVdomConverter
{
    public static bool TryConvert(string html, out VNode? root)
    {
        root = null;

        if (string.IsNullOrWhiteSpace(html))
        {
            return false;
        }

        try
        {
            var document = XDocument.Parse(html, LoadOptions.PreserveWhitespace);
            var element = document.Root;
            if (element is null)
            {
                return false;
            }

            root = ConvertElement(element);
            return true;
        }
        catch
        {
            root = null;
            return false;
        }
    }

    private static VNode? ConvertNode(XNode node)
        => node switch
        {
            XElement element => ConvertElement(element),
            XText text => new VTextNode(text.Value),
            _ => null,
        };

    private static VElementNode ConvertElement(XElement element)
    {
        var tagName = element.Name.LocalName.ToLowerInvariant();
        var attributes = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var attribute in element.Attributes())
        {
            attributes[attribute.Name.LocalName] = attribute.Value;
        }

        var children = new List<VNode>();
        foreach (var node in element.Nodes())
        {
            if (node is XText text && string.IsNullOrEmpty(text.Value))
            {
                continue;
            }

            var converted = ConvertNode(node);
            if (converted is not null)
            {
                children.Add(converted);
            }
        }

        var key = element.Attribute("data-key")?.Value ?? element.Attribute("key")?.Value;

        return new VElementNode(tagName, attributes, children, key);
    }
}
