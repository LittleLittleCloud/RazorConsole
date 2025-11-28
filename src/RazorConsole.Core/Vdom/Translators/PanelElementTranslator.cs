// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Extensions;
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
        var panel = new Panel(content);

        if (node.GetAttributeValue(nameof(Components.Panel.Expand), false))
        {
            panel = panel.Expand();
        }

        panel.Border = node.GetAttributeValue(nameof(Components.Panel.Border), BoxBorder.Square);

        panel.Padding = node.GetAttributeValue<Padding?>(nameof(Components.Panel.Padding));
        panel.Height = node.GetAttributeValue<int?>(nameof(Components.Panel.Height));

        panel.Width = node.GetAttributeValue<int?>(nameof(Components.Panel.Width));

        ApplyHeader(node, panel);
        ApplyBorderColor(node, panel);

        renderable = panel;
        return true;
    }

    private static void ApplyHeader(VNode node, Spectre.Console.Panel panel)
    {
        if (!node.TryGetAttributeValue<string>(nameof(Components.Panel.Title), out var title) || string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        if (node.TryGetAttributeValue<Color>(nameof(Components.Panel.TitleColor), out var titleColor))
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
        if (!node.TryGetAttributeValue<Color>(nameof(Components.Panel.BorderColor), out var borderColor))
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
