using System;
using System.Collections.Generic;
using System.Linq;
using Facebook.Yoga;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Renderables;

/// <summary>
/// A flexible box layout renderable using Yoga layout engine.
/// </summary>
public sealed class BoxRenderable : IRenderable
{
    private readonly List<IRenderable> _children;
    private readonly YogaFlexDirection _flexDirection;
    private readonly YogaJustify _justifyContent;
    private readonly YogaAlign _alignItems;
    private readonly YogaWrap _flexWrap;
    private readonly int? _width;
    private readonly int? _height;
    private readonly Padding _padding;
    private readonly int _gap;

    public BoxRenderable(
        IEnumerable<IRenderable> children,
        YogaFlexDirection flexDirection = YogaFlexDirection.Row,
        YogaJustify justifyContent = YogaJustify.FlexStart,
        YogaAlign alignItems = YogaAlign.Stretch,
        YogaWrap flexWrap = YogaWrap.NoWrap,
        int? width = null,
        int? height = null,
        Padding? padding = null,
        int gap = 0)
    {
        _children = children?.ToList() ?? new List<IRenderable>();
        _flexDirection = flexDirection;
        _justifyContent = justifyContent;
        _alignItems = alignItems;
        _flexWrap = flexWrap;
        _width = width;
        _height = height;
        _padding = padding ?? new Padding(0, 0, 0, 0);
        _gap = gap;
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        if (_children.Count == 0)
        {
            return new Measurement(_width ?? 0, _height ?? 0);
        }

        try
        {
            // Create the root Yoga node
            var rootNode = new YogaNode
            {
                FlexDirection = _flexDirection,
                JustifyContent = _justifyContent,
                AlignItems = _alignItems,
                Wrap = _flexWrap,
            };

            // Apply width and height if specified
            if (_width.HasValue)
            {
                rootNode.Width = _width.Value;
            }
            else
            {
                rootNode.Width = maxWidth;
            }

            if (_height.HasValue)
            {
                rootNode.Height = _height.Value;
            }

            // Apply padding
            rootNode.PaddingLeft = _padding.Left;
            rootNode.PaddingTop = _padding.Top;
            rootNode.PaddingRight = _padding.Right;
            rootNode.PaddingBottom = _padding.Bottom;

            // Create child nodes
            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                var measurement = child.Measure(options, maxWidth);

                var childNode = new YogaNode
                {
                    Width = measurement.Max,
                    Height = measurement.Max, // Use height from measurement if available
                };

                // Add gap between items (except for the last item)
                if (i > 0 && _gap > 0)
                {
                    if (_flexDirection == YogaFlexDirection.Row || _flexDirection == YogaFlexDirection.RowReverse)
                    {
                        childNode.MarginLeft = _gap;
                    }
                    else
                    {
                        childNode.MarginTop = _gap;
                    }
                }

                rootNode.Insert(i, childNode);
            }

            // Calculate layout
            rootNode.CalculateLayout();

            var calculatedWidth = (int)Math.Ceiling(rootNode.LayoutWidth);
            var calculatedHeight = (int)Math.Ceiling(rootNode.LayoutHeight);

            return new Measurement(calculatedWidth, calculatedHeight);
        }
        catch (Exception)
        {
            // If Yoga layout fails, fall back to simple measurement
            var totalWidth = _children.Sum(c => c.Measure(options, maxWidth).Max);
            var maxHeight = _children.Any() ? _children.Max(c => c.Measure(options, maxWidth).Max) : 0;
            return new Measurement(_width ?? totalWidth, _height ?? maxHeight);
        }
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (_children.Count == 0)
        {
            return Enumerable.Empty<Segment>();
        }

        try
        {
            // Create the root Yoga node
            var rootNode = new YogaNode
            {
                FlexDirection = _flexDirection,
                JustifyContent = _justifyContent,
                AlignItems = _alignItems,
                Wrap = _flexWrap,
            };

            // Apply width and height if specified
            if (_width.HasValue)
            {
                rootNode.Width = _width.Value;
            }
            else
            {
                rootNode.Width = maxWidth;
            }

            if (_height.HasValue)
            {
                rootNode.Height = _height.Value;
            }

            // Apply padding
            rootNode.PaddingLeft = _padding.Left;
            rootNode.PaddingTop = _padding.Top;
            rootNode.PaddingRight = _padding.Right;
            rootNode.PaddingBottom = _padding.Bottom;

            // Create child nodes and store renderables
            var childNodes = new List<(YogaNode node, IRenderable renderable)>();
            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                var measurement = child.Measure(options, maxWidth);

                var childNode = new YogaNode
                {
                    Width = measurement.Max,
                    Height = measurement.Max,
                };

                // Add gap between items (except for the last item)
                if (i > 0 && _gap > 0)
                {
                    if (_flexDirection == YogaFlexDirection.Row || _flexDirection == YogaFlexDirection.RowReverse)
                    {
                        childNode.MarginLeft = _gap;
                    }
                    else
                    {
                        childNode.MarginTop = _gap;
                    }
                }

                rootNode.Insert(i, childNode);
                childNodes.Add((childNode, child));
            }

            // Calculate layout
            rootNode.CalculateLayout();

            // Render based on calculated layout
            // For terminal rendering, we'll use a simple approach:
            // render as rows or columns based on flex direction
            IRenderable layout;

            if (_flexDirection == YogaFlexDirection.Row || _flexDirection == YogaFlexDirection.RowReverse)
            {
                // Render as columns
                layout = new Columns(_children);
            }
            else
            {
                // Render as rows
                layout = new Rows(_children);
            }

            return layout.Render(options, maxWidth);
        }
        catch (Exception)
        {
            // If Yoga layout fails, fall back to simple row layout
            var rows = new Rows(_children);
            return ((IRenderable)rows).Render(options, maxWidth);
        }
    }
}
