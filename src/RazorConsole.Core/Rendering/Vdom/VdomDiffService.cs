using System;
using System.Collections.Generic;

namespace RazorConsole.Core.Rendering.Vdom;

public sealed class VdomDiffService
{
    public VdomDiffResult Diff(VNode? previous, VNode? current)
    {
        var mutations = new List<VdomMutation>();
        var path = new List<int>();

        DiffInternal(previous, current, path, mutations);

        return new VdomDiffResult(current, mutations);
    }

    private static void DiffInternal(
        VNode? previous,
        VNode? current,
        List<int> path,
        List<VdomMutation> mutations)
    {
        if (previous is null && current is null)
        {
            return;
        }

        if (previous is null)
        {
            mutations.Add(CreateMutation(VdomMutationKind.InsertNode, path, current));
            return;
        }

        if (current is null)
        {
            mutations.Add(CreateMutation(VdomMutationKind.RemoveNode, path, null));
            return;
        }

        if (ReferenceEquals(previous, current) || VdomComparer.AreEqual(previous, current))
        {
            return;
        }

        if (previous.Kind != current.Kind)
        {
            mutations.Add(CreateMutation(VdomMutationKind.ReplaceNode, path, current));
            return;
        }

        switch (previous)
        {
            case VTextNode prevText when current is VTextNode newText:
                if (!string.Equals(prevText.Text, newText.Text, StringComparison.Ordinal))
                {
                    mutations.Add(CreateMutation(VdomMutationKind.UpdateText, path, current, newText.Text, null));
                }

                return;

            case VElementNode prevElement when current is VElementNode newElement:
                if (!string.Equals(prevElement.TagName, newElement.TagName, StringComparison.Ordinal) ||
                    !string.Equals(prevElement.Key, newElement.Key, StringComparison.Ordinal))
                {
                    mutations.Add(CreateMutation(VdomMutationKind.ReplaceNode, path, current));
                    return;
                }

                if (!AreAttributesEqual(prevElement.Attributes, newElement.Attributes))
                {
                    mutations.Add(CreateMutation(VdomMutationKind.UpdateAttributes, path, current, null, newElement.Attributes));
                }

                DiffChildren(prevElement.Children, newElement.Children, path, mutations);
                return;

            default:
                mutations.Add(CreateMutation(VdomMutationKind.ReplaceNode, path, current));
                return;
        }
    }

    private static void DiffChildren(
        IReadOnlyList<VNode> previous,
        IReadOnlyList<VNode> current,
        List<int> path,
        List<VdomMutation> mutations)
    {
        var previousCount = previous.Count;
        var currentCount = current.Count;
        var max = Math.Max(previousCount, currentCount);

        for (var i = 0; i < max; i++)
        {
            path.Add(i);

            if (i >= previousCount)
            {
                mutations.Add(CreateMutation(VdomMutationKind.InsertNode, path, current[i]));
                path.RemoveAt(path.Count - 1);
                continue;
            }

            if (i >= currentCount)
            {
                mutations.Add(CreateMutation(VdomMutationKind.RemoveNode, path, null));
                path.RemoveAt(path.Count - 1);
                continue;
            }

            DiffInternal(previous[i], current[i], path, mutations);
            path.RemoveAt(path.Count - 1);
        }
    }

    private static bool AreAttributesEqual(
        IReadOnlyDictionary<string, string?> left,
        IReadOnlyDictionary<string, string?> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var candidate) || !string.Equals(pair.Value, candidate, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static VdomMutation CreateMutation(
        VdomMutationKind kind,
        List<int> path,
        VNode? node,
        string? text = null,
        IReadOnlyDictionary<string, string?>? attributes = null)
    {
        var pathCopy = path.Count == 0 ? Array.Empty<int>() : path.ToArray();
        var resolvedText = text ?? (node is VTextNode textNode ? textNode.Text : null);
        var resolvedAttributes = attributes ?? (node is VElementNode element ? element.Attributes : null);

        return new VdomMutation(kind, pathCopy, node, resolvedText, resolvedAttributes);
    }
}

public sealed record VdomDiffResult(VNode? Current, IReadOnlyList<VdomMutation> Mutations)
{
    public bool HasChanges => Mutations.Count != 0;

    public static VdomDiffResult NoChanges(VNode? current)
        => new(current, Array.Empty<VdomMutation>());
}

public sealed record VdomMutation(
    VdomMutationKind Kind,
    IReadOnlyList<int> Path,
    VNode? Node,
    string? Text,
    IReadOnlyDictionary<string, string?>? Attributes);

public enum VdomMutationKind
{
    ReplaceNode,
    InsertNode,
    RemoveNode,
    UpdateAttributes,
    UpdateText,
}
