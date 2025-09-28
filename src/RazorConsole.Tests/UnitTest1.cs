using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests;

public class HtmlRenderingTests
{
    [Fact]
    public void TryCreateRenderable_ReturnsPanel_ForBorderMarkup()
    {
        const string html = "<div data-border=\"panel\" data-header=\"Header\">Hello</div>";

    var success = TryCreateRenderable(html, out var renderable);

        Assert.True(success);
        var panel = Assert.IsType<Panel>(renderable);
        Assert.Equal("Header", panel.Header?.Text);
    }

    [Fact]
    public void TryCreateRenderable_CapturesAnimatedSpinnerMetadata()
    {
        const string html = "<div data-spinner=\"true\" data-message=\"Working\" data-style=\"yellow\" data-spinner-type=\"Dots\"></div>";

    var success = TryCreateRenderable(html, out var renderable, out var animations);

        Assert.True(success);
        Assert.NotNull(renderable);
        var animatedSpinner = Assert.Single(animations);
        Assert.IsType<AnimatedSpinnerRenderable>(animatedSpinner);
        Assert.Equal(Spinner.Known.Dots.Interval, animatedSpinner.RefreshInterval);
    }

    [Fact]
    public void TryCreateRenderable_ReturnsPadderWithConfiguredPadding()
    {
        const string html = "<div data-padder=\"true\" data-padding=\"2,1,2,1\"><span data-text=\"true\">Content</span></div>";

    var success = TryCreateRenderable(html, out var renderable);

        Assert.True(success);
        var padder = Assert.IsType<Padder>(renderable);
        var paddingProperty = padder.GetType().GetProperty("Padding");
        Assert.NotNull(paddingProperty);
        var value = paddingProperty!.GetValue(padder);
        var padding = Assert.IsType<Padding>(value);
        Assert.Equal(new Padding(2, 1, 2, 1), padding);
    }

    [Fact]
    public void TryCreateRenderable_ReturnsAlignWithAlignmentAndSize()
    {
        const string html = "<div data-align=\"true\" data-horizontal=\"center\" data-vertical=\"middle\" data-width=\"40\" data-height=\"5\"><span data-text=\"true\">Inner</span></div>";

    var success = TryCreateRenderable(html, out var renderable);

        Assert.True(success);
        var align = Assert.IsType<Align>(renderable);

    var horizontalProperty = align.GetType().GetProperty("Horizontal");
        Assert.NotNull(horizontalProperty);
        var horizontal = (HorizontalAlignment?)horizontalProperty!.GetValue(align);
        Assert.Equal(HorizontalAlignment.Center, horizontal);

    var verticalProperty = align.GetType().GetProperty("Vertical");
        Assert.NotNull(verticalProperty);
        var vertical = (VerticalAlignment?)verticalProperty!.GetValue(align);
        Assert.Equal(VerticalAlignment.Middle, vertical);

        var widthProperty = align.GetType().GetProperty("Width");
        Assert.NotNull(widthProperty);
        var width = (int?)widthProperty!.GetValue(align);
        Assert.Equal(40, width);

        var heightProperty = align.GetType().GetProperty("Height");
        Assert.NotNull(heightProperty);
        var height = (int?)heightProperty!.GetValue(align);
        Assert.Equal(5, height);
    }

    [Fact]
    public void TryCreateRenderable_HonorsPanelExpandAttribute()
    {
        const string expandedHtml = "<div data-panel=\"true\" data-panel-expand=\"true\">Content</div>";
        const string collapsedHtml = "<div data-panel=\"true\">Content</div>";

    var expandedSuccess = TryCreateRenderable(expandedHtml, out var expandedRenderable);
    var collapsedSuccess = TryCreateRenderable(collapsedHtml, out var collapsedRenderable);

        Assert.True(expandedSuccess);
        Assert.True(collapsedSuccess);

        var expandedPanel = Assert.IsType<Panel>(expandedRenderable);
        var collapsedPanel = Assert.IsType<Panel>(collapsedRenderable);

        var expandProperty = expandedPanel.GetType().GetProperty("Expand");
        Assert.NotNull(expandProperty);

        var expandedValue = expandProperty!.GetValue(expandedPanel) as bool?;
        var collapsedValue = expandProperty.GetValue(collapsedPanel) as bool?;

        Assert.True(expandedValue);
        Assert.False(collapsedValue);
    }

    [Fact]
    public void TryCreateRenderable_AppliesPanelDimensionsAndStyling()
    {
        const string html = "<div data-panel=\"true\" data-panel-border=\"rounded\" data-panel-padding=\"1,2,3,4\" data-panel-height=\"10\" data-panel-width=\"40\">Content</div>";

    var success = TryCreateRenderable(html, out var renderable);

        Assert.True(success);

        var panel = Assert.IsType<Panel>(renderable);
        Assert.Equal(BoxBorder.Rounded, panel.Border);
        Assert.Equal(new Padding(1, 2, 3, 4), panel.Padding);
        Assert.Equal(10, panel.Height);
        Assert.Equal(40, panel.Width);
    }

    [Fact]
    public void TryCreateRenderable_ConfiguresColumnsExpandAndPadding()
    {
        const string html = "<div data-columns=\"true\" data-columns-expand=\"true\" data-columns-padding=\"1,2,3,4\"><span data-text=\"true\">One</span></div>";

    var success = TryCreateRenderable(html, out var renderable);

        Assert.True(success);

        var padder = Assert.IsType<Padder>(renderable);
        var paddingProperty = padder.GetType().GetProperty("Padding");
        Assert.NotNull(paddingProperty);
        var paddingValue = paddingProperty!.GetValue(padder);
        var padding = Assert.IsType<Padding>(paddingValue);
        Assert.Equal(new Padding(1, 2, 3, 4), padding);

        static object? GetChildRenderable(object source)
        {
            var type = source.GetType();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (typeof(IRenderable).IsAssignableFrom(property.PropertyType))
                {
                    return property.GetValue(source);
                }
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (typeof(IRenderable).IsAssignableFrom(field.FieldType))
                {
                    return field.GetValue(source);
                }
            }

            return null;
        }

        var child = GetChildRenderable(padder);
        Assert.NotNull(child);
        var columns = Assert.IsType<Columns>(child);

    var expandProperty = columns.GetType().GetProperty("Expand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(expandProperty);
        var expandValue = expandProperty!.GetValue(columns) as bool?;
        Assert.True(expandValue);
    }

    [Fact]
    public void TryCreateRenderable_ConfiguresRowsExpand()
    {
        const string html = "<div data-rows=\"true\" data-expand=\"true\"><span data-text=\"true\">Row1</span><span data-text=\"true\">Row2</span></div>";

    var success = TryCreateRenderable(html, out var renderable);

        Assert.True(success);

    var rows = Assert.IsType<Rows>(renderable);
    var expandProperty = rows.GetType().GetProperty("Expand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(expandProperty);
        var expandValue = expandProperty!.GetValue(rows) as bool?;
        Assert.True(expandValue);
    }

    [Fact]
    public void TryCreateRenderable_ConfiguresGridExpandAndWidth()
    {
        const string html = "<div data-grid=\"true\" data-columns=\"2\" data-grid-expand=\"true\" data-grid-width=\"80\"><span data-text=\"true\">Cell1</span><span data-text=\"true\">Cell2</span></div>";

    var success = TryCreateRenderable(html, out var renderable);

        Assert.True(success);

        var grid = Assert.IsType<Grid>(renderable);
    var expandProperty = grid.GetType().GetProperty("Expand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(expandProperty);
        var expandValue = expandProperty!.GetValue(grid) as bool?;
        Assert.True(expandValue);
        Assert.Equal(80, grid.Width);
    }

    private static bool TryCreateRenderable(string html, out IRenderable? renderable)
        => TryCreateRenderable(html, out renderable, out _);

    private static bool TryCreateRenderable(string html, out IRenderable? renderable, out IReadOnlyCollection<IAnimatedConsoleRenderable> animations)
    {
        var parsed = HtmlVdomConverter.TryConvert(html, out var root);
        Assert.True(parsed, "Expected HTML to parse to VDOM.");
        Assert.NotNull(root);
        return SpectreRenderableFactory.TryCreateRenderable(root, out renderable, out animations);
    }
}
