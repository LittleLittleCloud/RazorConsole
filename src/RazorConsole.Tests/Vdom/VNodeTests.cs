// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Vdom;

namespace RazorConsole.Tests.Vdom;

public sealed class VNodeTests
{
    [Fact]
    public void Equals_ReturnsTrue_ForIdenticalTrees()
    {
        var left = BuildSampleNode();
        var right = BuildSampleNode();

        (left == right).ShouldBeTrue();
        left.Equals(right).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenAttributesDiffer()
    {
        var left = BuildSampleNode();
        var right = BuildSampleNode();
        right.SetAttribute("data-id", "other");

        (left != right).ShouldBeTrue();
        left.Equals(right).ShouldBeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenChildStructureDiffers()
    {
        var left = BuildSampleNode();
        var right = BuildSampleNode();
        right.Children[0].AddChild(VNode.CreateText("extra"));

        (left != right).ShouldBeTrue();
    }

    [Fact]
    public void Equals_HandlesNullComparisons()
    {
        var node = BuildSampleNode();

        (node == null).ShouldBeFalse();
        (node != null).ShouldBeTrue();
        ((VNode?)null == (VNode?)null).ShouldBeTrue();
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

