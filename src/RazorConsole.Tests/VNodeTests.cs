using RazorConsole.Core.Vdom;
using Xunit;

namespace RazorConsole.Tests;

public sealed class VNodeTests
{
    [Fact]
    public void Equals_ReturnsTrue_ForIdenticalTrees()
    {
        var left = BuildSampleNode();
        var right = BuildSampleNode();

        Assert.True(left == right);
        Assert.True(left.Equals(right));
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenAttributesDiffer()
    {
        var left = BuildSampleNode();
        var right = BuildSampleNode();
        right.SetAttribute("data-id", "other");

        Assert.True(left != right);
        Assert.False(left.Equals(right));
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenChildStructureDiffers()
    {
        var left = BuildSampleNode();
        var right = BuildSampleNode();
        right.Children[0].AddChild(VNode.CreateText("extra"));

        Assert.True(left != right);
    }

    [Fact]
    public void Equals_HandlesNullComparisons()
    {
        var node = BuildSampleNode();

        Assert.False(node == null);
        Assert.True(node != null);
        Assert.True((VNode?)null == (VNode?)null);
    }

    private static VNode BuildSampleNode()
    {
        var root = VNode.CreateElement("div", "root");
        root.SetAttribute("class", "container");
        root.SetAttribute("data-id", "root");
        root.SetEvent("onclick", 1, new VNodeEventOptions(true, false));

        var child = VNode.CreateElement("span", "child");
        child.SetAttribute("role", "text");
        child.AddChild(VNode.CreateText("Hello"));

        root.AddChild(child);
        return root;
    }
}
