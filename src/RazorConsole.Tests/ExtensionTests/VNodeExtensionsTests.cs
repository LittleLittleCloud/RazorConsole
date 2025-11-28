// Copyright (c) RazorConsole. All rights reserved.


using RazorConsole.Core.Extensions;
using RazorConsole.Core.Vdom;

using Shouldly;

namespace RazorConsole.Tests.ExtensionTests;

public sealed class VNodeExtensionsTests
{

    private sealed record Mock;

    [Theory]
    [InlineData([42])]
    [InlineData(["string"])]
    public void TryGetAttributeValue_HasValueType_ReturnsValue<TValue>(TValue? expectedValue)
    {
        // Arrange
        const string key = "key";
        var node = VNode.CreateComponent();
        node.Attrs[key] = expectedValue;

        // Act
        var result = node.TryGetAttributeValue<TValue>(key, out var actualValue);

        // Assert
        result.ShouldBe(true);
        actualValue.ShouldBe(expectedValue);
    }



    [Fact]
    public void TryGetAttributeValue_NodeIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        static void act() => ((VNode?)null)!.TryGetAttributeValue<string>("key", out var _);

        // Act
        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Theory]
    [InlineData([(string?)null])]
    [InlineData([""])]
    [InlineData(["   "])]
    public void TryGetAttributeValue_InvalidKey_ThrowsArgumentException(string? key)
    {
        // Arrange
        var node = VNode.CreateComponent();
        void act() => node.TryGetAttributeValue<string>(key!, out var _);

        // Act
        // Assert
        Should.Throw<ArgumentException>(act);
    }

}
