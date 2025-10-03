using System;
using System.Collections.Generic;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests.Vdom;

public class VdomSpectreTranslatorTests
{
    [Fact]
    public void Translate_TextNode_ReturnsTextRenderable()
    {
        var translator = new VdomSpectreTranslator();
        var node = VNode.CreateText("Hello");

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Text>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_TextSpan_UsesVdomTranslator()
    {
        var node = Element("span", span =>
        {
            span.SetAttribute("data-text", "true");
            span.SetAttribute("data-style", "red");
            span.AddChild(Text("Hello"));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Markup>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_ParagraphNode_ReturnsRowsRenderable()
    {
        var node = Element("p", paragraph =>
        {
            paragraph.AddChild(Text("Paragraph content"));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Markup>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_Spacer_DoesNotUseHtmlFallback()
    {
        var node = Element("div", spacer =>
        {
            spacer.SetAttribute("data-spacer", "true");
            spacer.SetAttribute("data-lines", "2");
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Markup>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_PanelNode_ReturnsPanelRenderable()
    {
        var child = Element("span", span =>
        {
            span.SetAttribute("data-text", "true");
            span.SetAttribute("data-style", "green");
            span.AddChild(Text("Panel body"));
        });

        var node = Element("div", panel =>
        {
            panel.SetAttribute("data-panel", "true");
            panel.SetAttribute("data-panel-border", "rounded");
            panel.SetAttribute("data-panel-padding", "1 1 1 1");
            panel.SetAttribute("data-header", "Title");
            panel.AddChild(child);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        var panel = Assert.IsType<Panel>(renderable);
        Assert.NotNull(panel.Header);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_RowsNode_ReturnsRowsRenderable()
    {
        var child = Element("span", span =>
        {
            span.SetAttribute("data-text", "true");
            span.AddChild(Text("Row content"));
        });

        var node = Element("div", rows =>
        {
            rows.SetAttribute("data-rows", "true");
            rows.SetAttribute("data-expand", "true");
            rows.AddChild(child);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_HtmlButtonNode_ReturnsPanelRenderable()
    {
        var node = Element("button", button =>
        {
            button.AddChild(Text("Click me"));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Panel>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_ColumnsNode_ReturnsPadderWrappedColumnsWhenSpacingProvided()
    {
        var child = Element("span", span =>
        {
            span.SetAttribute("data-text", "true");
            span.AddChild(Text("Col1"));
        });

        var node = Element("div", columns =>
        {
            columns.SetAttribute("data-columns", "true");
            columns.SetAttribute("data-spacing", "2");
            columns.AddChild(child);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Padder>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_GridNode_ReturnsGridRenderable()
    {
        var nodes = new List<VNode>
        {
            Element("span", span =>
            {
                span.SetAttribute("data-text", "true");
                span.AddChild(Text("Cell1"));
            }),
            Element("span", span =>
            {
                span.SetAttribute("data-text", "true");
                span.AddChild(Text("Cell2"));
            }),
        };

        var node = Element("div", grid =>
        {
            grid.SetAttribute("data-grid", "true");
            grid.SetAttribute("data-columns", "2");
            foreach (var child in nodes)
            {
                grid.AddChild(child);
            }
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Grid>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_GenericDivNode_ReturnsRowsRenderable()
    {
        var node = Element("div", div =>
        {
            div.AddChild(Text("Line 1"));
            div.AddChild(Text("Line 2"));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_PadderNode_ReturnsPadderRenderable()
    {
        var child = Element("span", span =>
        {
            span.SetAttribute("data-text", "true");
            span.AddChild(Text("Padded"));
        });

        var node = Element("div", padder =>
        {
            padder.SetAttribute("data-padder", "true");
            padder.SetAttribute("data-padding", "1");
            padder.AddChild(child);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Padder>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_NewlineNode_ReturnsMarkup()
    {
        var node = Element("div", newline =>
        {
            newline.SetAttribute("data-newline", "true");
            newline.SetAttribute("data-count", "2");
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Markup>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_SpinnerNode_ReturnsAnimatedRenderable()
    {
        var node = Element("div", spinner =>
        {
            spinner.SetAttribute("data-spinner", "true");
            spinner.SetAttribute("data-message", "Loading");
            spinner.SetAttribute("data-style", "yellow");
            spinner.SetAttribute("data-spinner-type", "Dots");
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        var animated = Assert.IsAssignableFrom<IAnimatedConsoleRenderable>(renderable);
        var registered = Assert.Single(animations);
        Assert.Same(animated, registered);
    }

    [Fact]
    public void Translate_AlignNode_ReturnsAlignRenderable()
    {
        var child = Element("span", span =>
        {
            span.SetAttribute("data-text", "true");
            span.AddChild(Text("Aligned"));
        });

        var node = Element("div", align =>
        {
            align.SetAttribute("data-align", "true");
            align.SetAttribute("data-horizontal", "center");
            align.SetAttribute("data-vertical", "middle");
            align.SetAttribute("data-width", "40");
            align.SetAttribute("data-height", "5");
            align.AddChild(child);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Align>(renderable);
        Assert.Empty(animations);
    }

    private static VNode Element(string tagName, Action<VNode>? configure = null)
    {
        var node = VNode.CreateElement(tagName);
        configure?.Invoke(node);
        return node;
    }

    private static VNode Text(string? value) => VNode.CreateText(value);
}
