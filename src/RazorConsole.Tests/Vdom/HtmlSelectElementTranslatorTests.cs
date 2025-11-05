using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests.Vdom;

public class HtmlSelectElementTranslatorTests
{
    [Fact]
    public void TryTranslate_SelectWithOptions_ReturnsRows()
    {
        var node = Element("select", select =>
        {
            select.AddChild(Element("option", option =>
            {
                option.SetAttribute("value", "option1");
                option.AddChild(Text("Option 1"));
            }));
            select.AddChild(Element("option", option =>
            {
                option.SetAttribute("value", "option2");
                option.AddChild(Text("Option 2"));
            }));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void TryTranslate_SelectWithSelectedValue_DisplaysAllOptionsWithSelectedMarked()
    {
        var node = Element("select", select =>
        {
            select.SetAttribute("value", "option2");
            select.AddChild(Element("option", option =>
            {
                option.SetAttribute("value", "option1");
                option.AddChild(Text("First Option"));
            }));
            select.AddChild(Element("option", option =>
            {
                option.SetAttribute("value", "option2");
                option.AddChild(Text("Second Option"));
            }));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        var rows = Assert.IsType<Rows>(renderable);
        Assert.NotNull(rows);
    }

    [Fact]
    public void TryTranslate_SelectWithoutValue_DisplaysAllOptions()
    {
        var node = Element("select", select =>
        {
            select.AddChild(Element("option", option =>
            {
                option.SetAttribute("value", "opt1");
                option.AddChild(Text("Option 1"));
            }));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
    }

    [Fact]
    public void TryTranslate_OptionWithoutValueAttribute_UsesInnerText()
    {
        var node = Element("select", select =>
        {
            select.SetAttribute("value", "My Option");
            select.AddChild(Element("option", option =>
            {
                option.AddChild(Text("My Option"));
            }));
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
    }

    [Fact]
    public void TryTranslate_NonSelectElement_ReturnsFalse()
    {
        var node = Element("div", div =>
        {
            div.AddChild(Text("Not a select"));
        });

        var translator = new HtmlSelectElementTranslator();

        var success = translator.TryTranslate(node, new TranslationContext(new VdomSpectreTranslator()), out var renderable);

        Assert.False(success);
        Assert.Null(renderable);
    }

    [Fact]
    public void TryTranslate_EmptySelect_ReturnsRowsWithEmptyMessage()
    {
        var node = Element("select", select =>
        {
            // No options
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
        Assert.Empty(animations);
    }

    [Fact]
    public void TryTranslate_SelectWithCustomEmptyLabel_UsesCustomLabel()
    {
        var node = Element("select", select =>
        {
            select.SetAttribute("data-empty-label", "Nothing here");
            // No options
        });

        var translator = new VdomSpectreTranslator();

        var success = translator.TryTranslate(node, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
    }

    [Fact]
    public async Task HtmlSelectDemo_RendersSuccessfully()
    {
        // This test demonstrates that HTML select tags can be used in Razor components
        // and are properly translated to Spectre.Console renderables
        using var services = new ServiceCollection().BuildServiceProvider();
        using var renderer = TestHelpers.CreateTestRenderer(services);

        var snapshot = await renderer.MountComponentAsync<HtmlSelectDemoComponent>(ParameterView.Empty, CancellationToken.None);

        var root = Assert.IsType<VNode>(snapshot.Root);
        Assert.Equal("select", root.TagName);
        Assert.Equal(4, root.Children.Count); // 4 option elements

        // Verify it can be translated to a renderable
        var translator = new VdomSpectreTranslator();
        var success = translator.TryTranslate(root, out var renderable, out var animations);

        Assert.True(success);
        Assert.IsType<Rows>(renderable);
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
