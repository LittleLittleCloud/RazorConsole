using System.Linq;
using RazorConsole.Core.Vdom;
using Xunit;

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

        Assert.True(diff.HasChanges);
        var insert = Assert.Single(diff.Mutations, mutation => mutation.Kind == VdomMutationKind.InsertNode);
        Assert.Equal(new[] { 1 }, insert.Path);
    }

    [Fact]
    public void Diff_UpdatesAttributes_WhenValuesChange()
    {
        HtmlVdomConverter.TryConvert("<panel header=\"Old\"></panel>", out var previous);
        HtmlVdomConverter.TryConvert("<panel header=\"New\"></panel>", out var current);

        var diff = _service.Diff(previous, current);

        var mutation = Assert.Single(diff.Mutations);
        Assert.Equal(VdomMutationKind.UpdateAttributes, mutation.Kind);
        Assert.Equal("New", mutation.Attributes!["header"]);
    }

    [Fact]
    public void Diff_RemovesNode_WhenChildDisappears()
    {
        HtmlVdomConverter.TryConvert("<div><span>Hi</span></div>", out var previous);
        HtmlVdomConverter.TryConvert("<div></div>", out var current);

        var diff = _service.Diff(previous, current);

        var mutation = Assert.Single(diff.Mutations);
        Assert.Equal(VdomMutationKind.RemoveNode, mutation.Kind);
        Assert.Equal(new[] { 0 }, mutation.Path);
    }
}
