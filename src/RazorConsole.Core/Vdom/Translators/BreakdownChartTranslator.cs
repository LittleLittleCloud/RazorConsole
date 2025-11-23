using System.Globalization;
using System.Text.Json;
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

        var itemsAttribute = VdomSpectreTranslator.GetAttribute(node, "data-items");
        if (string.IsNullOrEmpty(itemsAttribute))
        {
            return false;
        }

        List<JsonElement> rawItems;
        try
        {
            rawItems = JsonSerializer.Deserialize<List<JsonElement>>(itemsAttribute) ??
                       throw new InvalidOperationException();
        }
        catch
        {
            return false;
        }

        var breakdownChart = new BreakdownChart();
        try
        {
            AddBreakdownChartItems(breakdownChart, rawItems);
        }
        catch
        {
            return false;
        }

        if (VdomSpectreTranslator.TryGetBoolAttribute(node, "data-compact", out var compact))
        {
            breakdownChart.Compact = compact;
        }


        var cultureInfoAttribute = VdomSpectreTranslator.GetAttribute(node, "data-culture");
        CultureInfo cultureInfo;
        if (cultureInfoAttribute != null)
        {
            try
            {
                cultureInfo = CultureInfo.GetCultureInfo(cultureInfoAttribute);
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        breakdownChart.Culture = cultureInfo;

        if (VdomSpectreTranslator.TryGetBoolAttribute(node, "data-expand", out var expand))
        {
            breakdownChart.Expand = expand;
        }

        if (VdomSpectreTranslator.TryParsePositiveInt(VdomSpectreTranslator.GetAttribute(node, "data-width"),
                out var width))
        {
            breakdownChart.Width = width;
        }

        if (VdomSpectreTranslator.TryGetBoolAttribute(node, "data-show-tags", out var showTags))
        {
            breakdownChart.ShowTags = showTags;
        }

        if (VdomSpectreTranslator.TryGetBoolAttribute(node, "data-show-tag-values", out var showTagValues))
        {
            breakdownChart.ShowTagValues = showTagValues;
        }

        if (VdomSpectreTranslator.TryGetBoolAttribute(node, "data-show-tag-values-percentage", out var showTagValuesPercentage) && showTagValuesPercentage)
        {
            breakdownChart.ShowPercentage();
        }

        var valueColorAttribute = VdomSpectreTranslator.GetAttribute(node, "data-value-color");
        if (!string.IsNullOrEmpty(valueColorAttribute) && Color.TryFromHex(valueColorAttribute, out var valueColor))
        {
            breakdownChart.ValueColor = valueColor;
        }

        renderable = breakdownChart;

        return true;
    }

    private static bool IsBreakdownChart(VNode node)
    {
        if (node.Kind != VNodeKind.Element)
        {
            return false;
        }

        if (!string.Equals(node.TagName, "breakdownchart", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private void AddBreakdownChartItems(BreakdownChart breakdownChart, List<JsonElement> rawItems)
    {
        foreach (var el in rawItems)
        {
            if (el.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidCastException("Only objects are supported");
            }

            string itemLabel;
            if (el.TryGetProperty(nameof(IBreakdownChartItem.Label), out var l))
            {
                var labelValue = l.GetString();
                itemLabel = labelValue ?? throw new InvalidOperationException("Label cannot be null");
            }
            else
            {
                throw new InvalidCastException("Not found label property");
            }

            double itemValue;
            if (el.TryGetProperty(nameof(IBreakdownChartItem.Value), out var v))
            {
                itemValue = v.TryGetDouble(out var value)
                    ? value
                    : throw new InvalidOperationException($"Unable to parse `{itemLabel}` value as a double");
            }
            else
            {
                throw new InvalidCastException("Not found value property");
            }

            Color color;
            if (el.TryGetProperty(nameof(IBreakdownChartItem.Color), out var cProp) &&
                !string.IsNullOrWhiteSpace(cProp.GetString()))
            {
                var hex = cProp.GetString();
                if (hex != null && Color.TryFromHex(hex, out var parsed))
                {
                    color = parsed;
                }
                else
                {
                    throw new InvalidOperationException($"Unable to parse `{itemLabel}` as a color");
                }
            }
            else
            {
                throw new InvalidCastException("Not found color property");
            }

            breakdownChart.AddItem(itemLabel, itemValue, color);
        }
    }
}
