// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Vdom;

namespace RazorConsole.Tests.Vdom;

public class VdomDiffServiceTests
{
    private readonly VdomDiffService _service = new();

    [Fact]
    public void Diff_InsertsNode_WhenNewChildAppears()
    {
        HtmlVdomConverter.TryConvert("<div><span>Hi</span></div>", out var previous);
        HtmlVdomConverter.TryConvert("<div><span>Hi</span><span>There</span></div>", out var current);

        var diff = _service.Diff(previous, current);

        diff.HasChanges.ShouldBeTrue();
        diff.Mutations.ShouldContain(mutation => mutation.Kind == VdomMutationKind.InsertNode);
        var insert = diff.Mutations.Single(mutation => mutation.Kind == VdomMutationKind.InsertNode);
        insert.Path.ShouldBe(new[] { 1 });
    }

    [Fact]
    public void Diff_UpdatesAttributes_WhenValuesChange()
    {
        HtmlVdomConverter.TryConvert("<panel header=\"Old\"></panel>", out var previous);
        HtmlVdomConverter.TryConvert("<panel header=\"New\"></panel>", out var current);

        var diff = _service.Diff(previous, current);

        diff.Mutations.ShouldHaveSingleItem();
        var mutation = diff.Mutations.Single();
        mutation.Kind.ShouldBe(VdomMutationKind.UpdateAttributes);
        mutation.Attributes!["header"].ShouldBe("New");
    }

    [Fact]
    public void Diff_RemovesNode_WhenChildDisappears()
    {
        HtmlVdomConverter.TryConvert("<div><span>Hi</span></div>", out var previous);
        HtmlVdomConverter.TryConvert("<div></div>", out var current);

        var diff = _service.Diff(previous, current);

        diff.Mutations.ShouldHaveSingleItem();
        var mutation = diff.Mutations.Single();
        mutation.Kind.ShouldBe(VdomMutationKind.RemoveNode);
        mutation.Path.ShouldBe(new[] { 0 });
    }
}
