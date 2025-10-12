using System;
using System.Collections.Generic;
using System.Reflection;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Syntax;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
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
    public void Translate_SyntaxHighlighterNode_ReturnsSyntaxRenderable()
    {
        var model = new SyntaxHighlightRenderModel(new[] { "[blue]Console.WriteLine()[/]" }, false, string.Empty, SyntaxOptions.Default.PlaceholderMarkup);
        var payload = SyntaxHighlightingService.EncodePayload(model);

        var node = Element("div", div =>
        {
            div.SetAttribute("class", "syntax-highlighter");
            div.SetAttribute("data-payload", payload);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<SyntaxRenderable>(renderable);
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
            panel.SetAttribute("class", "panel");
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
    public void Translate_RowsNode_ReturnsColumnsRenderable()
    {
        var child = Element("span", span =>
        {
            span.SetAttribute("class", "row");
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
        Assert.IsType<Columns>(renderable);
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
            grid.SetAttribute("class", "grid");
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
    public void Translate_GenericDivNode_ReturnsColumnsRenderable()
    {
        var node = Element("div", div =>
        {
            div.AddChild(Text("Line 1"));
            div.AddChild(Text("Line 2"));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Columns>(renderable);
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
            padder.SetAttribute("class", "padder");
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
            newline.SetAttribute("class", "newline");
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
            spinner.SetAttribute("class", "spinner");
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
            span.SetAttribute("class", "text");
            span.AddChild(Text("Aligned"));
        });

        var node = Element("div", align =>
        {
            align.SetAttribute("class", "align");
            align.SetAttribute("data-horizontal", "center");
            align.SetAttribute("data-vertical", "middle");
            align.SetAttribute("data-width", "40");
            align.SetAttribute("data-height", "5");
            align.AddChild(child);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<MeasuredAlign>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_TableNode_ReturnsTableRenderable()
    {
        var headerRow = Element("tr", tr =>
        {
            tr.AddChild(HeaderCell("Stage", "left"));
            tr.AddChild(HeaderCell("Duration", "center"));
            tr.AddChild(HeaderCell("Result", "right"));
        });

        var thead = Element("thead", head =>
        {
            head.AddChild(headerRow);
        });

        var bodyRow = Element("tr", tr =>
        {
            tr.SetAttribute("data-style", "fg=grey");
            tr.AddChild(DataCell("Compile"));
            tr.AddChild(DataCell("00:03:12"));
            tr.AddChild(DataCell("✔"));
        });

        var tbody = Element("tbody", body =>
        {
            body.AddChild(bodyRow);
        });

        var tableNode = Element("table", table =>
        {
            table.SetAttribute("class", "table");
            table.SetAttribute("data-expand", "true");
            table.SetAttribute("data-title", "Build status");
            table.AddChild(thead);
            table.AddChild(tbody);
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(tableNode, out var renderable, out var animations);

        Assert.True(success);
        var table = Assert.IsType<Table>(renderable);
        Assert.True(table.Expand);
        Assert.True(table.ShowHeaders);
        Assert.Equal(3, table.Columns.Count);
        Assert.Equal(Justify.Right, table.Columns[2].Alignment);
        Assert.Empty(animations);

        static VNode HeaderCell(string content, string align) => Element("th", th =>
        {
            th.SetAttribute("data-align", align);
            th.AddChild(Text(content));
        });

        static VNode DataCell(string content) => Element("td", td =>
        {
            td.AddChild(Text(content));
        });
    }

    [Fact]
    public void Translate_StrongElement_ReturnsBoldMarkup()
    {
        var node = Element("strong", strong =>
        {
            strong.AddChild(Text("Important"));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        var markup = Assert.IsType<Markup>(renderable);
        Assert.Equal("[bold]Important[/]", BuildInlineMarkupLiteral(node));
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_NestedInlineElements_ComposesMarkup()
    {
        var node = Element("strong", strong =>
        {
            strong.AddChild(Text("Hello "));
            strong.AddChild(Element("em", em =>
            {
                em.AddChild(Text("world"));
            }));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        var markup = Assert.IsType<Markup>(renderable);
        Assert.Equal("[bold]Hello [italic]world[/][/]", BuildInlineMarkupLiteral(node));
        Assert.Empty(animations);
    }

    [Fact]
    public void Translate_CodeElement_DisablesNestedFormatting()
    {
        var node = Element("code", code =>
        {
            code.AddChild(Text("var value = 42;"));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        var markup = Assert.IsType<Markup>(renderable);
        Assert.Equal("[grey53 on #1f1f1f]var value = 42;[/]", BuildInlineMarkupLiteral(node));
        Assert.Empty(animations);
    }

    [Fact]
    public void ConvertChildren_TextNodes_NormalizeWhitespace()
    {
        var translator = new VdomSpectreTranslator();
        var context = new VdomSpectreTranslator.TranslationContext(translator);
        var children = new List<VNode>
        {
            VNode.CreateText("  Hello   world"),
            VNode.CreateText("\n\tand\t friends  "),
        };

        var renderables = InvokeTryConvertChildren(children, context);
        Assert.Equal(2, renderables.Count);
    }

    [Fact]
    public void ConvertChildren_WhitespaceBetweenTextNodes_ProducesSingleSpacer()
    {
        var translator = new VdomSpectreTranslator();
        var context = new VdomSpectreTranslator.TranslationContext(translator);
        var children = new List<VNode>
        {
            VNode.CreateText("Hello"),
            VNode.CreateText("   \r\n\t "),
            VNode.CreateText("world"),
        };

        var renderables = InvokeTryConvertChildren(children, context);
        Assert.Equal(2, renderables.Count);
    }

    [Fact]
    public void ConvertChildren_TrailingWhitespace_IsDiscarded()
    {
        var translator = new VdomSpectreTranslator();
        var context = new VdomSpectreTranslator.TranslationContext(translator);
        var children = new List<VNode>
        {
            VNode.CreateText("Hello "),
        };

        var renderables = InvokeTryConvertChildren(children, context);

        var markup = Assert.Single(renderables);
        AssertMarkupText(markup);
    }

    private static VNode Element(string tagName, Action<VNode>? configure = null)
    {
        var node = VNode.CreateElement(tagName);
        configure?.Invoke(node);
        return node;
    }

    private static VNode Text(string? value) => VNode.CreateText(value);

    private static string BuildInlineMarkupLiteral(VNode node)
    {
        var translatorType = typeof(VdomSpectreTranslator).GetNestedType("HtmlInlineTextElementTranslator", BindingFlags.NonPublic);
        Assert.NotNull(translatorType);

        var method = translatorType!.GetMethod("TryBuildMarkup", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var arguments = new object?[] { node, null };
        var success = (bool)method!.Invoke(null, arguments)!;
        Assert.True(success);

        return Assert.IsType<string>(arguments[1]);
    }

    private static List<IRenderable> InvokeTryConvertChildren(IReadOnlyList<VNode> children, VdomSpectreTranslator.TranslationContext context)
    {
        var method = typeof(VdomSpectreTranslator).GetMethod("TryConvertChildrenToRenderables", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var arguments = new object?[] { children, context, null };
        var success = (bool)method!.Invoke(null, arguments)!;
        Assert.True(success);

        var renderables = Assert.IsType<List<IRenderable>>(arguments[2]);
        return renderables;
    }

    private static void AssertMarkupText(IRenderable renderable)
    {
        // TODO think about better way to extract text from markup
        var markup = Assert.IsType<Markup>(renderable);
    }
}
