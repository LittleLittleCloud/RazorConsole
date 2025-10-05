using System;
using System.Collections.Generic;

namespace RazorConsole.Core.Vdom;

public enum VNodeKind
{
    Component,
    Region,
    Element,
    Text,
}

public sealed class VNode
{
    private readonly List<VNode> _children;
    private readonly Dictionary<string, string?> _attributes;
    private readonly Dictionary<string, VNodeEvent> _events;

    private VNode(VNodeKind kind, string? tagName = null, string? text = null, string? key = null)
    {
        Kind = kind;
        TagName = tagName;
        Text = text ?? string.Empty;
        Key = key;
        _children = new List<VNode>();
        _attributes = new Dictionary<string, string?>(StringComparer.Ordinal);
        _events = new Dictionary<string, VNodeEvent>(StringComparer.OrdinalIgnoreCase);
    }

    public VNodeKind Kind { get; }

    public string? TagName { get; private set; }

    public string Text { get; private set; }

    public string? Key { get; private set; }

    public IReadOnlyList<VNode> Children => _children;

    public IReadOnlyDictionary<string, string?> Attributes => _attributes;

    public IReadOnlyCollection<VNodeEvent> Events => _events.Values;

    public static VNode CreateComponent() => new(VNodeKind.Component);

    public static VNode CreateRegion() => new(VNodeKind.Region);

    public static VNode CreateElement(string tagName, string? key = null)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name is required.", nameof(tagName));
        }

        return new VNode(VNodeKind.Element, tagName, key: key);
    }

    public static VNode CreateText(string? text)
        => new(VNodeKind.Text, text: text);

    public void AddChild(VNode child)
    {
        if (child is null)
        {
            throw new ArgumentNullException(nameof(child));
        }

        _children.Add(child);
    }

    public void InsertChild(int index, VNode child)
    {
        if (child is null)
        {
            throw new ArgumentNullException(nameof(child));
        }

        if ((uint)index > (uint)_children.Count)
        {
            index = _children.Count;
        }

        _children.Insert(index, child);
    }

    public void RemoveChildAt(int index)
    {
        if ((uint)index >= (uint)_children.Count)
        {
            return;
        }

        _children.RemoveAt(index);
    }

    public void SetAttribute(string name, string? value)
    {
        if ((Kind != VNodeKind.Element && Kind != VNodeKind.Component) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        _attributes[name] = value;
    }

    public void RemoveAttribute(string name)
    {
        if ((Kind != VNodeKind.Element && Kind != VNodeKind.Component) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        _attributes.Remove(name);
    }

    public void SetEvent(string name, ulong handlerId, VNodeEventOptions? options = null)
    {
        if (Kind != VNodeKind.Element || string.IsNullOrWhiteSpace(name) || handlerId == 0)
        {
            return;
        }

        var resolvedOptions = options ?? new VNodeEventOptions(false, false);
        _events[name] = new VNodeEvent(name, handlerId, resolvedOptions);
    }

    public void RemoveEvent(string name)
    {
        if (Kind != VNodeKind.Element || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        _events.Remove(name);
    }

    public void SetText(string? text)
    {
        if (Kind == VNodeKind.Text)
        {
            Text = text ?? string.Empty;
        }
    }

    public void SetKey(string? key)
    {
        if (Kind == VNodeKind.Element || Kind == VNodeKind.Component)
        {
            Key = string.IsNullOrWhiteSpace(key) ? null : key;
        }
    }

    public void SetTagName(string tagName)
    {
        if (Kind == VNodeKind.Element && !string.IsNullOrWhiteSpace(tagName))
        {
            TagName = tagName;
        }
    }

    public VNode Clone()
    {
        var clone = new VNode(Kind, TagName, Text, Key);
        foreach (var attribute in _attributes)
        {
            clone._attributes[attribute.Key] = attribute.Value;
        }

        foreach (var @event in _events)
        {
            clone._events[@event.Key] = @event.Value;
        }

        foreach (var child in _children)
        {
            clone._children.Add(child.Clone());
        }

        return clone;
    }

    public override string ToString()
        => Kind switch
        {
            VNodeKind.Component => "[Component]",
            VNodeKind.Region => "[Region]",
            VNodeKind.Element => $"[Element: {TagName}, Key={Key}, Attrs={_attributes.Count}, Events={_events.Count}, Children={_children.Count}]",
            VNodeKind.Text => $"[Text: \"{Text}\"]",
            _ => "[Unknown]",
        };
}

public readonly record struct VNodeEvent(string Name, ulong HandlerId, VNodeEventOptions Options);

public readonly record struct VNodeEventOptions(bool PreventDefault, bool StopPropagation);
