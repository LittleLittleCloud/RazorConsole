using System;
using System.Globalization;
using Facebook.Yoga;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class BoxElementTranslator : IVdomElementTranslator
{
    public int Priority => 115;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (!IsBoxNode(node))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        // Parse flex direction
        var flexDirectionStr = VdomSpectreTranslator.GetAttribute(node, "data-flex-direction") ?? "row";
        var flexDirection = ParseFlexDirection(flexDirectionStr);

        // Parse justify content
        var justifyContentStr = VdomSpectreTranslator.GetAttribute(node, "data-justify-content") ?? "flex-start";
        var justifyContent = ParseJustifyContent(justifyContentStr);

        // Parse align items
        var alignItemsStr = VdomSpectreTranslator.GetAttribute(node, "data-align-items") ?? "stretch";
        var alignItems = ParseAlignItems(alignItemsStr);

        // Parse flex wrap
        var flexWrapStr = VdomSpectreTranslator.GetAttribute(node, "data-flex-wrap") ?? "nowrap";
        var flexWrap = ParseFlexWrap(flexWrapStr);

        // Parse width and height
        int? width = null;
        var widthStr = VdomSpectreTranslator.GetAttribute(node, "data-width");
        if (!string.IsNullOrEmpty(widthStr) && int.TryParse(widthStr, out var w))
        {
            width = w;
        }

        int? height = null;
        var heightStr = VdomSpectreTranslator.GetAttribute(node, "data-height");
        if (!string.IsNullOrEmpty(heightStr) && int.TryParse(heightStr, out var h))
        {
            height = h;
        }

        // Parse padding
        var paddingStr = VdomSpectreTranslator.GetAttribute(node, "data-padding") ?? "0";
        var padding = ParsePadding(paddingStr);

        // Parse gap
        var gapStr = VdomSpectreTranslator.GetAttribute(node, "data-gap") ?? "0";
        var gap = int.TryParse(gapStr, out var g) ? g : 0;

        try
        {
            renderable = new BoxRenderable(
                children,
                flexDirection,
                justifyContent,
                alignItems,
                flexWrap,
                width,
                height,
                padding,
                gap);
        }
        catch (Exception)
        {
            // If BoxRenderable fails (e.g., native library not available),
            // fall back to simple row or column layout
            if (flexDirection == YogaFlexDirection.Row || flexDirection == YogaFlexDirection.RowReverse)
            {
                renderable = new Columns(children);
            }
            else
            {
                renderable = new Rows(children);
            }
        }

        return true;
    }

    private static bool IsBoxNode(VNode node)
    {
        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (node.Attributes.TryGetValue("class", out var value) && string.Equals(value, "box", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static YogaFlexDirection ParseFlexDirection(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "column" => YogaFlexDirection.Column,
            "column-reverse" => YogaFlexDirection.ColumnReverse,
            "row-reverse" => YogaFlexDirection.RowReverse,
            _ => YogaFlexDirection.Row,
        };
    }

    private static YogaJustify ParseJustifyContent(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "flex-end" => YogaJustify.FlexEnd,
            "center" => YogaJustify.Center,
            "space-between" => YogaJustify.SpaceBetween,
            "space-around" => YogaJustify.SpaceAround,
            _ => YogaJustify.FlexStart,
        };
    }

    private static YogaAlign ParseAlignItems(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "flex-start" => YogaAlign.FlexStart,
            "flex-end" => YogaAlign.FlexEnd,
            "center" => YogaAlign.Center,
            "baseline" => YogaAlign.Baseline,
            "stretch" => YogaAlign.Stretch,
            _ => YogaAlign.Stretch,
        };
    }

    private static YogaWrap ParseFlexWrap(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "wrap" => YogaWrap.Wrap,
            "wrap-reverse" => YogaWrap.WrapReverse,
            _ => YogaWrap.NoWrap,
        };
    }

    private static Padding ParsePadding(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new Padding(0, 0, 0, 0);
        }

        var parts = value.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1 && int.TryParse(parts[0], out var all))
        {
            return new Padding(all, all, all, all);
        }
        else if (parts.Length == 2 && int.TryParse(parts[0], out var vertical) && int.TryParse(parts[1], out var horizontal))
        {
            return new Padding(horizontal, vertical, horizontal, vertical);
        }
        else if (parts.Length == 4 &&
                 int.TryParse(parts[0], out var left) &&
                 int.TryParse(parts[1], out var top) &&
                 int.TryParse(parts[2], out var right) &&
                 int.TryParse(parts[3], out var bottom))
        {
            return new Padding(left, top, right, bottom);
        }

        return new Padding(0, 0, 0, 0);
    }
}
