// Copyright (c) RazorConsole. All rights reserved.

using System.Globalization;
using RazorConsole.Core.Extensions;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Vdom.Translators;

public class BreakdownChartTranslator : IVdomElementTranslator
{
    public int Priority => 150;

    public bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable)
    {
        renderable = null;

        if (!IsBreakdownChart(node))
        {
            return false;
        }

        if (!node.TryGetAttributeValue<List<IBreakdownChartItem>>(nameof(Components.BreakdownChart.BreakdownChartItems), out var breakdownChartItems)
            || breakdownChartItems is null)
        {
            return false;
        }

        var breakdownChart = new Spectre.Console.BreakdownChart();
        try
        {
            AddBreakdownChartItems(breakdownChart, breakdownChartItems);
        }
        catch
        {
            return false;
        }

        breakdownChart.Compact = node.GetAttributeValue(nameof(Components.BreakdownChart.Compact), false);
        breakdownChart.Culture = node.GetAttributeValue(nameof(Components.BreakdownChart.Culture), CultureInfo.CurrentCulture);
        breakdownChart.Expand = node.GetAttributeValue(nameof(Components.BreakdownChart.Expand), false);
        breakdownChart.Width = node.GetAttributeValue<int?>(nameof(Components.BreakdownChart.Width));
        breakdownChart.ShowTags = node.GetAttributeValue(nameof(Components.BreakdownChart.ShowTags), false);
        breakdownChart.ShowTagValues = node.GetAttributeValue(nameof(Components.BreakdownChart.ShowTagValues), false);

        if (node.GetAttributeValue(nameof(Components.BreakdownChart.ShowTagValuesPercentage), false))
        {
            breakdownChart.ShowPercentage();
        }

        if (node.TryGetAttributeValue<Color>(nameof(Components.BreakdownChart.ValueColor), out var colorValue))
        {
            breakdownChart.ValueColor = colorValue;
        }

        renderable = breakdownChart;

        return true;
    }

    private static bool IsBreakdownChart(VNode node)
    {
        if (node.Kind != VNodeKind.Component)
        {
            return false;
        }

        if (node.ComponentType != typeof(RazorConsole.Components.BreakdownChart))
        {
            return false;
        }

        return true;
    }

    private void AddBreakdownChartItems(Spectre.Console.BreakdownChart breakdownChart, List<IBreakdownChartItem> items)
    {
        foreach (var item in items)
        {
            breakdownChart.AddItem(item.Label, item.Value, item.Color);
        }
    }
}
