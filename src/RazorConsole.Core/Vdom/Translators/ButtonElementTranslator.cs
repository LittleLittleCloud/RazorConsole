// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class ButtonElementTranslator : IVdomElementTranslator
{
    public int Priority => 70;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!string.Equals(node.TagName, "div", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(node.TagName, "button", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!node.Attributes.TryGetValue("data-button", out var value) || !string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!VdomSpectreTranslator.TryConvertChildrenToBlockInlineRenderable(node.Children, context, out var children) || children is null)
        {
            return false;
        }

        var descriptor = ButtonRenderableDescriptorFactory.Create(name => VdomSpectreTranslator.GetAttribute(node, name));
        renderable = ButtonRenderableBuilder.Build(descriptor, children);
        return true;
    }
}
