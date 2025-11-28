// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class PanelElementTranslator : IVdomElementTranslator
{
    public int Priority => 150;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(Components.Panel))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToRenderables(node.Children, context, out var children))
        {
            return false;
        }

        var content = VdomSpectreTranslator.ComposeChildContent(children);
        var panel = new Spectre.Console.Panel(content);

        node.Attrs.TryGetValue(nameof(Components.Panel.Expand), out var expandObj);
        if (expandObj is bool expand && expand)
        {
            panel = panel.Expand();
        }

        node.Attrs.TryGetValue(nameof(Components.Panel.Border), out var borderObj);
        if (borderObj is BoxBorder border)
        {
            panel.Border = border;
        }
        else
        {
            panel.Border = BoxBorder.Square;
        }

        node.Attrs.TryGetValue(nameof(Components.Panel.Padding), out var paddingObj);
        if (paddingObj is Padding padding)
        {
            panel.Padding = padding;
        }

        node.Attrs.TryGetValue(nameof(Components.Panel.Height), out var heightObj);
        if (heightObj is int height && height > 0)
        {
            panel.Height = height;
        }

        node.Attrs.TryGetValue(nameof(Components.Panel.Width), out var widthObj);
        if (widthObj is int width && width > 0)
        {
            panel.Width = width;
        }

        ApplyHeader(node, panel);
        ApplyBorderColor(node, panel);

        renderable = panel;
        return true;
    }

    private static void ApplyHeader(VNode node, Spectre.Console.Panel panel)
    {
        if (!node.Attrs.TryGetValue(nameof(Components.Panel.Title), out var titleObj) || titleObj is not string title || string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        node.Attrs.TryGetValue(nameof(Components.Panel.TitleColor), out var titleColorObj);
        if (titleColorObj is Color titleColor)
        {
            var markup = ComponentMarkupUtilities.CreateStyledMarkup(titleColor.ToMarkup(), title, requiresEscape: true);
            panel.Header = new PanelHeader(markup);
        }
        else
        {
            panel.Header = new PanelHeader(Spectre.Console.Markup.Escape(title));
        }
    }

    private static void ApplyBorderColor(VNode node, Spectre.Console.Panel panel)
    {
        if (!node.Attrs.TryGetValue(nameof(Components.Panel.BorderColor), out var borderColorObj) || borderColorObj is not Color borderColor)
        {
            return;
        }

        try
        {
            var style = new Style(borderColor);
            panel.BorderStyle(style);
        }
        catch (Exception)
        {
            // Ignore invalid style specifications.
        }
    }
}
