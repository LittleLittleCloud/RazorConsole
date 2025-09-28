using RazorConsole.Core.Rendering.Vdom;
using Xunit;

namespace RazorConsole.Tests.Vdom;

public class HtmlVdomConverterTests
{
    [Fact]
    public void TryConvert_ReturnsRootNode_ForSimpleMarkup()
    {
        var success = HtmlVdomConverter.TryConvert("<div class='panel'><span>Hi</span></div>", out var root);

        Assert.True(success);
        var element = Assert.IsType<VElementNode>(root);
        Assert.Equal("div", element.TagName);
        Assert.Equal("panel", element.Attributes["class"]);
        Assert.Single(element.Children);
        var textElement = Assert.IsType<VElementNode>(element.Children[0]);
        Assert.Equal("span", textElement.TagName);
        var text = Assert.IsType<VTextNode>(textElement.Children[0]);
        Assert.Equal("Hi", text.Text);
    }

    [Fact]
    public void VdomComparer_DetectsStructuralDifferences()
    {
        HtmlVdomConverter.TryConvert("<p data-key='a'>Hello</p>", out var first);
        HtmlVdomConverter.TryConvert("<p data-key='a'>Hello</p>", out var second);
        HtmlVdomConverter.TryConvert("<p data-key='a'>World</p>", out var mutated);

        Assert.True(VdomComparer.AreEqual(first, second));
        Assert.False(VdomComparer.AreEqual(first, mutated));
    }

    [Fact]
    public void VdomDiffService_ReturnsTextMutation_ForTextChanges()
    {
        var service = new VdomDiffService();
        HtmlVdomConverter.TryConvert("<p>Hi</p>", out var previous);
        HtmlVdomConverter.TryConvert("<p>Bye</p>", out var current);

        var diff = service.Diff(previous, current);

        Assert.True(diff.HasChanges);
        Assert.Single(diff.Mutations);
        Assert.Equal(VdomMutationKind.UpdateText, diff.Mutations[0].Kind);
        Assert.Equal("Bye", diff.Mutations[0].Text);
    }
}
