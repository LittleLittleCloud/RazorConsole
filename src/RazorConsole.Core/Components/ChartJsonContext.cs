using System.Text.Json;
using System.Text.Json.Serialization;

namespace RazorConsole.Components;

/// <summary>
/// JSON serialization context for chart data to support trimming.
/// </summary>
[JsonSerializable(typeof(ChartItemData))]
[JsonSerializable(typeof(List<ChartItemData>))]
[JsonSerializable(typeof(List<JsonElement>))]
internal partial class ChartJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Represents chart item data for JSON serialization.
/// </summary>
internal record ChartItemData(string Label, double Value, string? Color);
