using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RazorConsole.Core.Vdom;

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

    private static VNode ConvertElement(XElement element, IReadOnlyList<int> path)
    {
        var tagName = element.Name.LocalName.ToLowerInvariant();
        var vnode = VNode.CreateElement(tagName);

        foreach (var attribute in element.Attributes())
        {
            var name = attribute.Name.LocalName;
            var value = attribute.Value;
            vnode.SetAttribute(name, value);
            if (string.Equals(name, "key", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "data-key", StringComparison.OrdinalIgnoreCase))
            {
                vnode.SetKey(string.IsNullOrWhiteSpace(value) ? null : value);
            }
        }

        var childElementIndex = 0;
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText text when string.IsNullOrEmpty(text.Value):
                    break;
                case XText text:
                    vnode.AddChild(VNode.CreateText(text.Value));
                    break;
                case XElement childElement:
                    {
                        var childPath = new List<int>(path) { childElementIndex };
                        vnode.AddChild(ConvertElement(childElement, childPath));
                        childElementIndex++;
                        break;
                    }
            }
        }

        vnode.SetKey(ResolveKey(element, path));
        return vnode;
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
