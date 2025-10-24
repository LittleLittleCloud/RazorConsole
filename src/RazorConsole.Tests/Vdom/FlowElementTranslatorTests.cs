using System;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests.Vdom;

public class FlowElementTranslatorTests
{
    [Fact]
    public void TryTranslate_WithoutDataFlowAttribute_ReturnsFalse()
    {
        var translator = new FlowElementTranslator();
        var node = Element("div", div =>
        {
            div.AddChild(Text("Content"));
        });

        var context = CreateContext();
        var result = translator.TryTranslate(node, context, out var renderable);

        Assert.False(result);
        Assert.Null(renderable);
    }

    [Fact]
    public void TryTranslate_WithDataFlowAttribute_ReturnsBlockInlineRenderable()
    {
        var translator = new FlowElementTranslator();
        var node = Element("div", div =>
        {
            div.SetAttribute("data-flow", "mixed");
            div.AddChild(Element("span", span =>
            {
                span.SetAttribute("data-text", "true");
                span.AddChild(Text("Inline"));
            }));
        });

        var context = CreateContext();
        var result = translator.TryTranslate(node, context, out var renderable);

        Assert.True(result);
        Assert.IsType<BlockInlineRenderable>(renderable);
    }

    [Fact]
    public void TryTranslate_MixedBlockAndInlineElements_CreatesCorrectItems()
    {
        var translator = new FlowElementTranslator();
        var node = Element("div", div =>
        {
            div.SetAttribute("data-flow", "mixed");
            
            // Add a block element (div)
            div.AddChild(Element("div", blockDiv =>
            {
                blockDiv.SetAttribute("data-text", "true");
                blockDiv.AddChild(Text("Block"));
            }));
            
            // Add an inline element (span)
            div.AddChild(Element("span", span =>
            {
                span.SetAttribute("data-text", "true");
                span.AddChild(Text("Inline"));
            }));
        });

        var context = CreateContext();
        var result = translator.TryTranslate(node, context, out var renderable);

        Assert.True(result);
        Assert.IsType<BlockInlineRenderable>(renderable);
    }

    [Fact]
    public void TryTranslate_WithExplicitDataDisplay_RespectsAttribute()
    {
        var translator = new FlowElementTranslator();
        var node = Element("div", div =>
        {
            div.SetAttribute("data-flow", "mixed");
            
            // Force span to be block with data-display
            div.AddChild(Element("span", span =>
            {
                span.SetAttribute("data-text", "true");
                span.SetAttribute("data-display", "block");
                span.AddChild(Text("Forced block"));
            }));
            
            // Force div to be inline with data-display
            div.AddChild(Element("div", inlineDiv =>
            {
                inlineDiv.SetAttribute("data-text", "true");
                inlineDiv.SetAttribute("data-display", "inline");
                inlineDiv.AddChild(Text("Forced inline"));
            }));
        });

        var context = CreateContext();
        var result = translator.TryTranslate(node, context, out var renderable);

        Assert.True(result);
        Assert.IsType<BlockInlineRenderable>(renderable);
    }

    [Fact]
    public void TryTranslate_WithNoTranslatableChildren_ReturnsFalse()
    {
        var translator = new FlowElementTranslator();
        var node = Element("div", div =>
        {
            div.SetAttribute("data-flow", "mixed");
            // Add empty flow (no children)
        });

        var context = CreateContext();
        var result = translator.TryTranslate(node, context, out var renderable);

        Assert.False(result);
    }

    [Fact]
    public void Priority_ReturnsExpectedValue()
    {
        var translator = new FlowElementTranslator();
        Assert.Equal(45, translator.Priority);
    }

    private static VNode Element(string tagName, Action<VNode>? configure = null)
    {
        var node = VNode.CreateElement(tagName);
        configure?.Invoke(node);
        return node;
    }

    private static VNode Text(string? value) => VNode.CreateText(value);

    private static TranslationContext CreateContext()
    {
        var vdomTranslator = new VdomSpectreTranslator();
        return new TranslationContext(vdomTranslator);
    }
}
