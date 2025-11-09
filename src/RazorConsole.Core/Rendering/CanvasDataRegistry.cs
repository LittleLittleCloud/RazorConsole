using System.Collections.Concurrent;
using Spectre.Console;


namespace RazorConsole.Core.Rendering;

public static class CanvasDataRegistry
{
    private static readonly ConcurrentDictionary<Guid, (int, int, Color)[]> Data = new();

    public static void Register(Guid id, (int, int, Color)[] data) => Data.AddOrUpdate(id, data, (_, _) => data);

    public static void Unregister(Guid id) => Data.TryRemove(id, out _);

    public static bool TryGetData(Guid id, out (int, int, Color)[]? data) => Data.TryGetValue(id, out data);
}
