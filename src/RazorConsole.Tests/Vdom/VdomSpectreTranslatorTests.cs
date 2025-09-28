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
        var node = new VTextNode("Hello");

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Text>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_TextSpan_UsesVdomTranslator()
    {
        var node = new VElementNode(
            "span",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-text", "true" },
                { "data-style", "red" },
            },
            new List<VNode> { new VTextNode("Hello") });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Markup>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_Spacer_DoesNotUseHtmlFallback()
    {
        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-spacer", "true" },
                { "data-lines", "2" },
            },
            Array.Empty<VNode>());

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Markup>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_PanelNode_ReturnsPanelRenderable()
    {
        var child = new VElementNode(
            "span",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-text", "true" },
                { "data-style", "green" },
            },
            new List<VNode> { new VTextNode("Panel body") });

        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-panel", "true" },
                { "data-panel-border", "rounded" },
                { "data-panel-padding", "1 1 1 1" },
                { "data-header", "Title" },
            },
            new List<VNode> { child });

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
        var child = new VElementNode(
            "span",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-text", "true" },
            },
            new List<VNode> { new VTextNode("Row content") });

        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-rows", "true" },
                { "data-expand", "true" },
            },
            new List<VNode> { child });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_ColumnsNode_ReturnsPadderWrappedColumnsWhenSpacingProvided()
    {
        var child = new VElementNode(
            "span",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-text", "true" },
            },
            new List<VNode> { new VTextNode("Col1") });

        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-columns", "true" },
                { "data-spacing", "2" },
            },
            new List<VNode> { child });

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
            new VElementNode(
                "span",
                new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    { "data-text", "true" },
                },
                new List<VNode> { new VTextNode("Cell1") }),
            new VElementNode(
                "span",
                new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    { "data-text", "true" },
                },
                new List<VNode> { new VTextNode("Cell2") }),
        };

        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-grid", "true" },
                { "data-columns", "2" },
            },
            nodes);

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Grid>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_PadderNode_ReturnsPadderRenderable()
    {
        var child = new VElementNode(
            "span",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-text", "true" },
            },
            new List<VNode> { new VTextNode("Padded") });

        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-padder", "true" },
                { "data-padding", "1" },
            },
            new List<VNode> { child });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Padder>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_NewlineNode_ReturnsMarkup()
    {
        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-newline", "true" },
                { "data-count", "2" },
            },
            Array.Empty<VNode>());

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Markup>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_SpinnerNode_ReturnsAnimatedRenderable()
    {
        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-spinner", "true" },
                { "data-message", "Loading" },
                { "data-style", "yellow" },
                { "data-spinner-type", "Dots" },
            },
            Array.Empty<VNode>());

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
        var child = new VElementNode(
            "span",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-text", "true" },
            },
            new List<VNode> { new VTextNode("Aligned") });

        var node = new VElementNode(
            "div",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "data-align", "true" },
                { "data-horizontal", "center" },
                { "data-vertical", "middle" },
                { "data-width", "40" },
                { "data-height", "5" },
            },
            new List<VNode> { child });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Align>(renderable);
        Assert.Empty(animations);
    }
}
