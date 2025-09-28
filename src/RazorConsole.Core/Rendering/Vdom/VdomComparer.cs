using System;

namespace RazorConsole.Core.Rendering.Vdom;

internal static class VdomComparer
{
    public static bool AreEqual(VNode? left, VNode? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left.Kind != right.Kind)
        {
            return false;
        }

        return left switch
        {
            VTextNode leftText when right is VTextNode rightText => string.Equals(leftText.Text, rightText.Text, StringComparison.Ordinal),
            VElementNode leftElement when right is VElementNode rightElement => AreElementsEqual(leftElement, rightElement),
            _ => false,
        };
    }

    private static bool AreElementsEqual(VElementNode left, VElementNode right)
    {
        if (!string.Equals(left.TagName, right.TagName, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.Equals(left.Key, right.Key, StringComparison.Ordinal))
        {
            return false;
        }

        if (left.Attributes.Count != right.Attributes.Count)
        {
            return false;
        }

        foreach (var pair in left.Attributes)
        {
            if (!right.Attributes.TryGetValue(pair.Key, out var rightValue))
            {
                return false;
            }

            if (!string.Equals(pair.Value, rightValue, StringComparison.Ordinal))
            {
                return false;
            }
        }

        if (left.Children.Count != right.Children.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Children.Count; i++)
        {
            if (!AreEqual(left.Children[i], right.Children[i]))
            {
                return false;
            }
        }

        return true;
    }
}
