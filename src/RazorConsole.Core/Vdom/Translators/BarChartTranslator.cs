// Copyright (c) RazorConsole. All rights reserved.

using System.Globalization;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Vdom.Translators;

public class BarChartTranslator : IVdomElementTranslator
{
    public int Priority => 150;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (!IsBarChart(node))
        {
            return false;
        }

        if (!node.Attrs.TryGetValue(nameof(Components.BarChart.BarChartItems), out var itemsObj) ||
            itemsObj is not List<IBarChartItem> barChartItems)
        {
            return false;
        }

        var barChart = new Spectre.Console.BarChart();
        try
        {
            AddBarChartItems(barChart, barChartItems);
        }
        catch
        {
            return false;
        }

        if (node.Attrs.TryGetValue(nameof(Components.BarChart.Width), out var widthObj) && widthObj is int width)
        {
            barChart.Width = width;
        }

        if (node.Attrs.TryGetValue(nameof(Components.BarChart.Label), out var labelObj) && labelObj is string label && !string.IsNullOrEmpty(label))
        {
            barChart.Label = label;

            if (node.Attrs.TryGetValue(nameof(Components.BarChart.LabelForeground), out var fgObj) &&
                node.Attrs.TryGetValue(nameof(Components.BarChart.LabelBackground), out var bgObj) &&
                node.Attrs.TryGetValue(nameof(Components.BarChart.LabelDecoration), out var decObj) &&
                fgObj is Color fg && bgObj is Color bg && decObj is Decoration dec)
            {
                var labelStyle = new Style(fg, bg, dec);
                barChart.Label = $"[{labelStyle.ToMarkup()}]{label}[/]";
            }
        }

        if (node.Attrs.TryGetValue(nameof(Components.BarChart.LabelAlignment), out var labelAlignmentObj) &&
            labelAlignmentObj is Justify labelAlignment)
        {
            barChart.LabelAlignment = labelAlignment;
        }

        if (node.Attrs.TryGetValue(nameof(Components.BarChart.MaxValue), out var maxValueObj) && maxValueObj is double maxValue)
        {
            barChart.MaxValue = maxValue;
        }

        if (node.Attrs.TryGetValue(nameof(Components.BarChart.ShowValues), out var showValuesObj) && showValuesObj is bool showValues)
        {
            barChart.ShowValues = showValues;
        }

        if (node.Attrs.TryGetValue(nameof(Components.BarChart.Culture), out var cultureObj) && cultureObj is CultureInfo cultureInfo)
        {
            barChart.Culture = cultureInfo;
        }
        else
        {
            barChart.Culture = CultureInfo.CurrentCulture;
        }

        renderable = barChart;

        return true;
    }

    private static bool IsBarChart(VNode node)
    {
        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(Components.BarChart))
        {
            return false;
        }

        return true;
    }

    private void AddBarChartItems(Spectre.Console.BarChart barChart, List<IBarChartItem> items)
    {
        foreach (var item in items)
        {
            barChart.AddItem(item.Label, item.Value, item.Color);
        }
    }
}
