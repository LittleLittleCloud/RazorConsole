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

            root = ConvertElement(element, new[] { 0 });
            return true;
        }
        catch
        {
            root = null;
            return false;
        }
    }

    private static VElementNode ConvertElement(XElement element, IReadOnlyList<int> path)
    {
        var tagName = element.Name.LocalName.ToLowerInvariant();
        var attributes = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var attribute in element.Attributes())
        {
            attributes[attribute.Name.LocalName] = attribute.Value;
        }

        var children = new List<VNode>();
        var childElementIndex = 0;
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText text when string.IsNullOrEmpty(text.Value):
                    break;
                case XText text:
                    children.Add(new VTextNode(text.Value));
                    break;
                case XElement childElement:
                {
                    var childPath = new List<int>(path) { childElementIndex };
                    children.Add(ConvertElement(childElement, childPath));
                    childElementIndex++;
                    break;
                }
            }
        }

        var key = ResolveKey(element, path);

        return new VElementNode(tagName, attributes, children, key);
    }

    private static string ResolveKey(XElement element, IReadOnlyList<int> path)
    {
        var explicitKey = element.Attribute("data-key")?.Value ?? element.Attribute("key")?.Value;
        if (!string.IsNullOrWhiteSpace(explicitKey))
        {
            return explicitKey;
        }

        if (path.Count == 0)
        {
            return "0";
        }

        return string.Join('.', path);
    }
}
