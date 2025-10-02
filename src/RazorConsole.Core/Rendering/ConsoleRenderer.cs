#nullable enable
#pragma warning disable BL0006 // RenderTree types are "internal-ish"; acceptable for console renderer.
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;
using System.Globalization;
using System.Text;

namespace RazorConsole.Core.Rendering;

internal sealed class ConsoleRenderer : Renderer, IObservable<ConsoleRenderer.RenderSnapshot>
{
    private sealed class ImmediateDispatcher : Dispatcher
    {
        public override bool CheckAccess() => true;

        public override Task InvokeAsync(Action workItem)
        {
            workItem?.Invoke();
            return Task.CompletedTask;
        }

        public override Task InvokeAsync(Func<Task> workItem)
            => workItem?.Invoke() ?? Task.CompletedTask;

        public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            return Task.FromResult(workItem());
        }

        public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            return workItem();
        }
    }

    private static readonly ImmediateDispatcher DispatcherInstance = new();

    private readonly Dictionary<int, VNode> _componentRoots = new();
    private readonly Stack<VNode> _cursor = new();
    private readonly VdomSpectreTranslator _translator = new();
    private readonly object _observersSync = new();
    private readonly List<IObserver<RenderSnapshot>> _observers = new();

    private TaskCompletionSource<RenderSnapshot>? _pendingRender;
    private int _rootComponentId = -1;
    private RenderSnapshot _lastSnapshot = RenderSnapshot.Empty;
    private bool _disposed;

    public ConsoleRenderer(IServiceProvider services, ILoggerFactory loggerFactory)
        : base(services, loggerFactory)
    {
        // _logger = loggerFactory.CreateLogger<ConsoleRenderer>();
    }

    public override Dispatcher Dispatcher => DispatcherInstance;

    public async Task<RenderSnapshot> MountComponentAsync<TComponent>(ParameterView parameters, CancellationToken cancellationToken)
        where TComponent : IComponent
    {
        var component = InstantiateComponent(typeof(TComponent));
        var componentId = AssignRootComponentId(component);
        _rootComponentId = componentId;

        var tcs = new TaskCompletionSource<RenderSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        _pendingRender = tcs;

        await RenderRootComponentAsync(componentId, parameters).ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }

    public IDisposable Subscribe(IObserver<RenderSnapshot> observer)
    {
        if (observer is null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        RenderSnapshot snapshot;
        IDisposable subscription;
        lock (_observersSync)
        {
            if (_disposed)
            {
                snapshot = _lastSnapshot;
                subscription = NoopDisposable.Instance;
            }
            else
            {
                _observers.Add(observer);
                snapshot = _lastSnapshot;
                subscription = new Subscription(this, observer);
            }
        }

        observer.OnNext(snapshot);

        if (_disposed)
        {
            observer.OnCompleted();
        }

        return subscription;
    }

    protected override Task UpdateDisplayAsync(in RenderBatch batch)
    {
        for (var i = 0; i < batch.UpdatedComponents.Count; i++)
        {
            var diff = batch.UpdatedComponents.Array[i];
            ApplyComponentEdits(batch, diff);
        }

        for (var i = 0; i < batch.DisposedComponentIDs.Count; i++)
        {
            var componentId = batch.DisposedComponentIDs.Array[i];
            _componentRoots.Remove(componentId);
        }

        var snapshot = CreateSnapshot();
        _lastSnapshot = snapshot;

        _pendingRender?.TrySetResult(snapshot);
        _pendingRender = null;

        NotifyObservers(snapshot);

        return Task.CompletedTask;
    }

    protected override void HandleException(Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        NotifyError(exception);
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
    }

    internal RenderSnapshot GetCurrentSnapshot() => _lastSnapshot;

    private void ApplyComponentEdits(in RenderBatch batch, RenderTreeDiff diff)
    {
        var slot = GetOrCreateComponentSlot(diff.ComponentId);
        _cursor.Clear();
        _cursor.Push(slot);
        var edits = diff.Edits;
        var offset = edits.Offset;
        for (var i = 0; i < edits.Count; i++)
        {
            var edit = edits.Array[i + offset];
            switch (edit.Type)
            {
                case RenderTreeEditType.PrependFrame:
                    {
                        var parent = _cursor.Peek();
                        var referenceFrame = batch.ReferenceFrames.Array[edit.ReferenceFrameIndex];
                        if (referenceFrame.FrameType == RenderTreeFrameType.Attribute)
                        {
                            ApplyAttributeFrame(parent, referenceFrame);
                            break;
                        }

                        var (child, _) = BuildSubtree(batch.ReferenceFrames, edit.ReferenceFrameIndex);
                        parent.InsertChild(edit.SiblingIndex, child);
                        break;
                    }

                case RenderTreeEditType.RemoveFrame:
                    {
                        var parent = _cursor.Peek();
                        parent.RemoveChildAt(edit.SiblingIndex);
                        break;
                    }

                case RenderTreeEditType.SetAttribute:
                    {
                        var parent = _cursor.Peek();
                        var frame = batch.ReferenceFrames.Array[edit.ReferenceFrameIndex];
                        if (frame.FrameType == RenderTreeFrameType.Attribute)
                        {
                            ApplyAttributeFrame(parent, frame);
                        }

                        break;
                    }

                case RenderTreeEditType.RemoveAttribute:
                    {
                        var parent = _cursor.Peek();
                        var frame = batch.ReferenceFrames.Array[edit.ReferenceFrameIndex];
                        if (frame.FrameType == RenderTreeFrameType.Attribute)
                        {
                            if (frame.AttributeEventHandlerId != 0)
                            {
                                parent.RemoveEvent(frame.AttributeName!);
                            }
                            else
                            {
                                parent.RemoveAttribute(frame.AttributeName!);
                                if (IsKeyAttribute(frame.AttributeName!))
                                {
                                    parent.SetKey(null);
                                }
                            }
                        }

                        break;
                    }

                case RenderTreeEditType.UpdateText:
                    {
                        var parent = _cursor.Peek();
                        if ((uint)edit.SiblingIndex < (uint)parent.Children.Count)
                        {
                            var child = parent.Children[edit.SiblingIndex];
                            var frame = batch.ReferenceFrames.Array[edit.ReferenceFrameIndex];
                            var textContent = frame.FrameType == RenderTreeFrameType.Text
                                ? frame.TextContent
                                : frame.MarkupContent;
                            child.SetText(textContent);
                        }

                        break;
                    }

                case RenderTreeEditType.StepIn:
                    {
                        var parent = _cursor.Peek();
                        if ((uint)edit.SiblingIndex < (uint)parent.Children.Count)
                        {
                            _cursor.Push(parent.Children[edit.SiblingIndex]);
                        }

                        break;
                    }

                case RenderTreeEditType.StepOut:
                    {
                        if (_cursor.Count > 0)
                        {
                            _cursor.Pop();
                        }

                        break;
                    }
            }
        }
    }

    private (VNode Node, int NextIndex) BuildSubtree(ArrayRange<RenderTreeFrame> frames, int index)
    {
        var frame = frames.Array[index];
        switch (frame.FrameType)
        {
            case RenderTreeFrameType.Element:
                {
                    var element = VNode.CreateElement(frame.ElementName!);
                    var end = index + frame.ElementSubtreeLength;
                    index++;

                    while (index < end && frames.Array[index].FrameType == RenderTreeFrameType.Attribute)
                    {
                        var attribute = frames.Array[index];
                        if (attribute.AttributeEventHandlerId != 0)
                        {
                            element.SetEvent(attribute.AttributeName!, attribute.AttributeEventHandlerId);
                        }
                        else
                        {
                            var value = FormatAttributeValue(attribute.AttributeValue);
                            element.SetAttribute(attribute.AttributeName!, value);
                            if (IsKeyAttribute(attribute.AttributeName!))
                            {
                                element.SetKey(string.IsNullOrWhiteSpace(value) ? null : value);
                            }
                        }

                        index++;
                    }

                    while (index < end)
                    {
                        var (child, next) = BuildSubtree(frames, index);
                        element.AddChild(child);
                        index = next;
                    }

                    return (element, index);
                }

            case RenderTreeFrameType.Text:
                return (VNode.CreateText(frame.TextContent), index + 1);

            case RenderTreeFrameType.Markup:
                if (HtmlVdomConverter.TryConvert(frame.MarkupContent, out var vnode) && vnode is not null)
                {
                    return (vnode, index + 1);
                }
                else
                {
                    return (VNode.CreateText(frame.MarkupContent), index + 1);
                }

            case RenderTreeFrameType.Region:
                {
                    var region = VNode.CreateRegion();
                    var end = index + frame.RegionSubtreeLength;
                    index++;

                    while (index < end)
                    {
                        var (child, next) = BuildSubtree(frames, index);
                        region.AddChild(child);
                        index = next;
                    }

                    return (region, index);
                }

            case RenderTreeFrameType.Component:
                {
                    var componentId = frame.ComponentId;
                    var component = VNode.CreateComponent();
                    component.SetAttribute("component-id", componentId.ToString(CultureInfo.InvariantCulture));
                    if (frame.ComponentType is not null)
                    {
                        component.SetAttribute("component-type", frame.ComponentType.FullName);
                    }

                    var end = index + frame.ComponentSubtreeLength;
                    index++;

                    while (index < end && frames.Array[index].FrameType == RenderTreeFrameType.Attribute)
                    {
                        var attribute = frames.Array[index];
                        if (attribute.AttributeEventHandlerId == 0)
                        {
                            var value = FormatAttributeValue(attribute.AttributeValue);
                            component.SetAttribute(attribute.AttributeName!, value);
                        }

                        index++;
                    }

                    index = end;
                    return (component, index);
                }

            default:
                return (VNode.CreateRegion(), index + 1);
        }
    }

    private VNode GetOrCreateComponentSlot(int componentId)
    {
        if (_componentRoots.TryGetValue(componentId, out var node))
        {
            return node;
        }

        var slot = VNode.CreateComponent();
        slot.SetAttribute("component-id", componentId.ToString(CultureInfo.InvariantCulture));
        _componentRoots[componentId] = slot;
        return slot;
    }

    private RenderSnapshot CreateSnapshot()
    {
        // LogComponentRoots();

        if (_rootComponentId == -1 || !_componentRoots.TryGetValue(_rootComponentId, out var componentNode))
        {
            return RenderSnapshot.Empty;
        }

        var vnode = CreateRenderableRoot(componentNode);
        if (vnode is null)
        {
            return RenderSnapshot.Empty;
        }

        if (!_translator.TryTranslate(vnode, out var renderable, out var animatedRenderables) || renderable is null)
        {
            return RenderSnapshot.Empty;
        }

        return new RenderSnapshot(vnode, renderable, animatedRenderables);
    }

    private VNode? CreateRenderableRoot(VNode node)
    {
        var visitedComponents = new HashSet<int>();
        var nodeId = TryGetComponentId(node);
        if (nodeId.HasValue)
        {
            visitedComponents.Add(nodeId.Value);
        }

        var collected = CollectRenderableChildren(node, visitedComponents);
        return collected.Count switch
        {
            0 => null,
            1 => collected[0],
            _ => CreateRowsWrapper(collected),
        };
    }
    private void LogComponentRoots()
    {
        if (_componentRoots.Count == 0)
        {
            return;
        }

        var builder = new StringBuilder();
        foreach (var entry in _componentRoots.OrderBy(static pair => pair.Key))
        {
            builder.Append("Component ");
            builder.Append(entry.Key);
            builder.AppendLine(":");
            AppendNode(entry.Value, builder, string.Empty, true);
            builder.AppendLine();
        }

        var dump = builder.ToString();
        if (!string.IsNullOrWhiteSpace(dump))
        {
            Console.WriteLine(dump.TrimEnd());
        }
    }

    private static void AppendNode(VNode node, StringBuilder builder, string indent, bool isLast)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (indent.Length > 0)
        {
            builder.Append(indent);
            builder.Append(isLast ? "└── " : "├── ");
        }
        else
        {
            builder.Append("• ");
        }

        builder.Append(DescribeNode(node));
        builder.AppendLine();

        if (node.Children.Count == 0)
        {
            return;
        }

        var nextIndent = indent + (indent.Length > 0 ? (isLast ? "    " : "│   ") : "   ");
        for (var i = 0; i < node.Children.Count; i++)
        {
            AppendNode(node.Children[i], builder, nextIndent, i == node.Children.Count - 1);
        }
    }

    private static string DescribeNode(VNode node)
    {
        var summary = new StringBuilder();
        summary.Append(node.Kind);

        switch (node.Kind)
        {
            case VNodeKind.Element:
                summary.Append(' ');
                summary.Append(node.TagName ?? "<unknown>");
                if (!string.IsNullOrWhiteSpace(node.Key))
                {
                    summary.Append(" key=");
                    summary.Append(node.Key);
                }

                AppendAttributes(summary, node.Attributes);
                AppendEvents(summary, node.Events);
                break;
            case VNodeKind.Text:
                var text = node.Text ?? string.Empty;
                text = text.Replace("\r", "\\r", StringComparison.Ordinal)
                           .Replace("\n", "\\n", StringComparison.Ordinal);
                const int maxLength = 60;
                if (text.Length > maxLength)
                {
                    text = text[..maxLength] + "…";
                }

                summary.Append(" \"");
                summary.Append(text);
                summary.Append('\"');
                break;
            case VNodeKind.Component:
                if (!string.IsNullOrWhiteSpace(node.Key))
                {
                    summary.Append(" key=");
                    summary.Append(node.Key);
                }

                AppendAttributes(summary, node.Attributes);
                AppendEvents(summary, node.Events);
                // TODO
                // include children
                break;
            case VNodeKind.Region:
                break;
        }

        return summary.ToString();
    }

    private static void AppendAttributes(StringBuilder builder, IReadOnlyDictionary<string, string?> attributes)
    {
        if (attributes.Count == 0)
        {
            return;
        }

        builder.Append(" attrs[");
        var first = true;
        foreach (var attribute in attributes.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            if (!first)
            {
                builder.Append(", ");
            }

            first = false;
            builder.Append(attribute.Key);
            builder.Append('=');
            builder.Append(attribute.Value ?? "<null>");
        }

        builder.Append(']');
    }

    private static void AppendEvents(StringBuilder builder, IReadOnlyCollection<VNodeEvent> events)
    {
        if (events.Count == 0)
        {
            return;
        }

        builder.Append(" events[");
        var first = true;
        foreach (var @event in events.OrderBy(static evt => evt.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (!first)
            {
                builder.Append(", ");
            }

            first = false;
            builder.Append(@event.Name);
        }

        builder.Append(']');
    }

    private List<VNode> CollectRenderableChildren(VNode node, HashSet<int> visitedComponents)
    {
        var result = new List<VNode>();
        foreach (var child in node.Children)
        {
            switch (child.Kind)
            {
                case VNodeKind.Element:
                    result.Add(child.Clone());
                    break;
                case VNodeKind.Text:
                    result.Add(VNode.CreateText(child.Text));
                    break;
                case VNodeKind.Component:
                    {
                        var childId = TryGetComponentId(child);
                        if (childId.HasValue && !visitedComponents.Contains(childId.Value) && _componentRoots.TryGetValue(childId.Value, out var componentRoot))
                        {
                            visitedComponents.Add(childId.Value);
                            result.AddRange(CollectRenderableChildren(componentRoot, visitedComponents));
                            visitedComponents.Remove(childId.Value);
                        }
                        else
                        {
                            result.AddRange(CollectRenderableChildren(child, visitedComponents));
                        }

                        break;
                    }
                default:
                    result.AddRange(CollectRenderableChildren(child, visitedComponents));
                    break;
            }
        }

        return result;
    }

    private static VNode CreateRowsWrapper(List<VNode> children)
    {
        var wrapper = VNode.CreateElement("div");
        wrapper.SetAttribute("data-rows", "true");
        foreach (var child in children)
        {
            wrapper.AddChild(child);
        }

        return wrapper;
    }

    private static int? TryGetComponentId(VNode node)
    {
        if (node.Attributes.TryGetValue("component-id", out var value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var componentId))
        {
            return componentId;
        }

        return null;
    }

    private static void ApplyAttributeFrame(VNode parent, RenderTreeFrame frame)
    {
        if (frame.AttributeEventHandlerId != 0)
        {
            parent.SetEvent(frame.AttributeName!, frame.AttributeEventHandlerId);
            return;
        }

        var value = FormatAttributeValue(frame.AttributeValue);
        parent.SetAttribute(frame.AttributeName!, value);
        if (IsKeyAttribute(frame.AttributeName!))
        {
            parent.SetKey(string.IsNullOrWhiteSpace(value) ? null : value);
        }
    }

    private static bool IsKeyAttribute(string name)
        => string.Equals(name, "key", StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "data-key", StringComparison.OrdinalIgnoreCase);

    private static string? FormatAttributeValue(object? value)
        => value switch
        {
            null => null,
            string s => s,
            bool b => b ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

    public readonly record struct RenderSnapshot(VNode? Root, IRenderable? Renderable, IReadOnlyCollection<IAnimatedConsoleRenderable> AnimatedRenderables)
    {
        public static RenderSnapshot Empty { get; } = new(null, null, Array.Empty<IAnimatedConsoleRenderable>());
    }

    private void NotifyObservers(RenderSnapshot snapshot)
    {
        List<IObserver<RenderSnapshot>> observers;
        lock (_observersSync)
        {
            if (_observers.Count == 0)
            {
                return;
            }

            observers = new List<IObserver<RenderSnapshot>>(_observers);
        }

        foreach (var observer in observers)
        {
            observer.OnNext(snapshot);
        }
    }

    private void NotifyError(Exception exception)
    {
        List<IObserver<RenderSnapshot>> observers;
        lock (_observersSync)
        {
            if (_observers.Count == 0)
            {
                return;
            }

            observers = new List<IObserver<RenderSnapshot>>(_observers);
        }

        foreach (var observer in observers)
        {
            observer.OnError(exception);
        }
    }

    internal Task DispatchEventAsync(ulong handlerId, EventArgs eventArgs)
    {
        if (handlerId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(handlerId));
        }

        if (eventArgs is null)
        {
            throw new ArgumentNullException(nameof(eventArgs));
        }

    return base.DispatchEventAsync(handlerId, default, eventArgs);
    }

    private void CompleteObservers()
    {
        List<IObserver<RenderSnapshot>> observers;
        lock (_observersSync)
        {
            if (_observers.Count == 0)
            {
                return;
            }

            observers = new List<IObserver<RenderSnapshot>>(_observers);
            _observers.Clear();
        }

        foreach (var observer in observers)
        {
            observer.OnCompleted();
        }
    }

    private void Unsubscribe(IObserver<RenderSnapshot> observer)
    {
        lock (_observersSync)
        {
            _observers.Remove(observer);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_observersSync)
            {
                if (_disposed)
                {
                    base.Dispose(disposing);
                    return;
                }

                _disposed = true;
            }

            CompleteObservers();
        }

        base.Dispose(disposing);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly ConsoleRenderer _owner;
        private IObserver<RenderSnapshot>? _observer;

        public Subscription(ConsoleRenderer owner, IObserver<RenderSnapshot> observer)
        {
            _owner = owner;
            _observer = observer;
        }

        public void Dispose()
        {
            var observer = Interlocked.Exchange(ref _observer, null);
            if (observer is not null)
            {
                _owner.Unsubscribe(observer);
            }
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        private NoopDisposable()
        {
        }

        public void Dispose()
        {
        }
    }
}
#pragma warning restore BL0006
