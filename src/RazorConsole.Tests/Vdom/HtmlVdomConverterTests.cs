// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Vdom;

namespace RazorConsole.Tests.Vdom;

public class HtmlVdomConverterTests
{
    [Fact]
    public void TryConvert_ReturnsRootNode_ForSimpleMarkup()
    {
        var success = HtmlVdomConverter.TryConvert("<div class='panel'><span>Hi</span></div>", out var root);

        success.ShouldBeTrue();
        root.ShouldNotBeNull();
        root.Kind.ShouldBe(VNodeKind.Element);
        root.TagName.ShouldBe("div");
        root.Attributes["class"].ShouldBe("panel");
        root.Children.ShouldHaveSingleItem();
        var textElement = root.Children[0];
        textElement.Kind.ShouldBe(VNodeKind.Element);
        textElement.TagName.ShouldBe("span");
        var text = textElement.Children[0];
        text.Kind.ShouldBe(VNodeKind.Text);
        text.Text.ShouldBe("Hi");
    }

    [Fact]
    public void VdomComparer_DetectsStructuralDifferences()
    {
        HtmlVdomConverter.TryConvert("<p data-key='a'>Hello</p>", out var first);
        HtmlVdomConverter.TryConvert("<p data-key='a'>Hello</p>", out var second);
        HtmlVdomConverter.TryConvert("<p data-key='a'>World</p>", out var mutated);

        VdomComparer.AreEqual(first, second).ShouldBeTrue();
        VdomComparer.AreEqual(first, mutated).ShouldBeFalse();
    }

    [Fact]
    public void VdomDiffService_ReturnsTextMutation_ForTextChanges()
    {
        var service = new VdomDiffService();
        HtmlVdomConverter.TryConvert("<p>Hi</p>", out var previous);
        HtmlVdomConverter.TryConvert("<p>Bye</p>", out var current);

        var diff = service.Diff(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.Mutations.ShouldHaveSingleItem();
        diff.Mutations[0].Kind.ShouldBe(VdomMutationKind.UpdateText);
        diff.Mutations[0].Text.ShouldBe("Bye");
    }
}
