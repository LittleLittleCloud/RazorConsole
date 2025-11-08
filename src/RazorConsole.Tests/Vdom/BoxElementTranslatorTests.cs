using System;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests.Vdom;

public class BoxElementTranslatorTests
{
    [Fact]
    public void Translate_BoxNode_ReturnsBoxRenderable()
    {
        var node = Element("div", div =>
        {
            div.SetAttribute("class", "box");
            div.SetAttribute("data-flex-direction", "row");
            div.AddChild(Element("span", span => span.AddChild(Text("Item 1"))));
            div.AddChild(Element("span", span => span.AddChild(Text("Item 2"))));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        // BoxRenderable may fall back to Columns if Yoga native lib isn't available
        Assert.True(renderable is BoxRenderable || renderable is Columns);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_BoxNode_WithColumnDirection_ReturnsBoxRenderable()
    {
        var node = Element("div", div =>
        {
            div.SetAttribute("class", "box");
            div.SetAttribute("data-flex-direction", "column");
            div.AddChild(Element("span", span => span.AddChild(Text("Item 1"))));
            div.AddChild(Element("span", span => span.AddChild(Text("Item 2"))));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        // BoxRenderable may fall back to Rows if Yoga native lib isn't available
        Assert.True(renderable is BoxRenderable || renderable is Rows);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_BoxNode_WithWidth_ReturnsBoxRenderable()
    {
        var node = Element("div", div =>
        {
            div.SetAttribute("class", "box");
            div.SetAttribute("data-width", "100");
            div.SetAttribute("data-height", "50");
            div.AddChild(Element("span", span => span.AddChild(Text("Content"))));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.True(renderable is BoxRenderable || renderable is Columns);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_BoxNode_WithGap_ReturnsBoxRenderable()
    {
        var node = Element("div", div =>
        {
            div.SetAttribute("class", "box");
            div.SetAttribute("data-gap", "2");
            div.AddChild(Element("span", span => span.AddChild(Text("Item 1"))));
            div.AddChild(Element("span", span => span.AddChild(Text("Item 2"))));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.True(renderable is BoxRenderable || renderable is Columns);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_NonBoxNode_ReturnsFalse()
    {
        var node = Element("div", div =>
        {
            div.SetAttribute("class", "not-a-box");
            div.AddChild(Text("Content"));
        });

        var translator = new BoxElementTranslator();
        var context = new TranslationContext(new VdomSpectreTranslator());

        var success = translator.TryTranslate(node, context, out var renderable);

        Assert.False(success);
        Assert.Null(renderable);
    }

    private static VNode Element(string tagName, Action<VNode> configure)
    {
        var node = VNode.CreateElement(tagName);
        configure(node);
        return node;
    }

    private static VNode Text(string text)
    {
        return VNode.CreateText(text);
    }
}
