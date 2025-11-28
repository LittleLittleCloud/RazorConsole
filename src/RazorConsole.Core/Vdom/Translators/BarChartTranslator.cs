// Copyright (c) RazorConsole. All rights reserved.

using System.Globalization;
using RazorConsole.Core.Extensions;
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

        if (!node.TryGetAttributeValue<List<IBarChartItem>>(nameof(Components.BarChart.BarChartItems), out var barChartItems)
            || barChartItems is null)
        {
            return false;
        }

        var barChart = new BarChart();
        try
        {
            AddBarChartItems(barChart, barChartItems);
        }
        catch
        {
            return false;
        }

        barChart.Width = node.GetAttributeValue<int?>(nameof(Components.BarChart.Width));


        if (node.TryGetAttributeValue<string>(nameof(Components.BarChart.Label), out var label) && !string.IsNullOrWhiteSpace(label))
        {
            barChart.Label = label;

            var fg = node.GetAttributeValue(nameof(Components.BarChart.LabelForeground), Style.Plain.Foreground);
            var bg = node.GetAttributeValue(nameof(Components.BarChart.LabelBackground), Style.Plain.Background);
            var dec = node.GetAttributeValue(nameof(Components.BarChart.LabelDecoration), Decoration.None);
            var labelStyle = new Style(fg, bg, dec);
            barChart.Label = $"[{labelStyle.ToMarkup()}]{label}[/]";
        }

        barChart.LabelAlignment = node.GetAttributeValue<Justify?>(nameof(Components.BarChart.LabelAlignment));
        barChart.MaxValue = node.GetAttributeValue<double?>(nameof(Components.BarChart.MaxValue));
        barChart.ShowValues = node.GetAttributeValue(nameof(Components.BarChart.ShowValues), false);
        barChart.Culture = node.GetAttributeValue(nameof(Components.BarChart.Culture), CultureInfo.CurrentCulture);

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
