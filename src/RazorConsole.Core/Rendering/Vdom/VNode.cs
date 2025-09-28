using System.Collections.Generic;

namespace RazorConsole.Core.Rendering.Vdom;

public enum VNodeKind
{
    Element,
    Text,
}

public abstract record VNode(string? Key)
{
    public abstract VNodeKind Kind { get; }
}

public sealed record VElementNode(
    string TagName,
    IReadOnlyDictionary<string, string?> Attributes,
    IReadOnlyList<VNode> Children,
    string? Key = null) : VNode(Key)
{
    public override VNodeKind Kind => VNodeKind.Element;
}

public sealed record VTextNode(string Text) : VNode(Key: null)
{
    public override VNodeKind Kind => VNodeKind.Text;
}
