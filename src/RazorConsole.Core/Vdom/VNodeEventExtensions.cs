// Copyright (c) RazorConsole. All rights reserved.

namespace RazorConsole.Core.Vdom;

/// <summary>
/// Extension methods for working with VNode events.
/// </summary>
internal static class VNodeEventCollectionExtensions
{
    /// <summary>
    /// Attempts to find an event with the specified name in the events collection.
    /// </summary>
    /// <param name="events">The collection of events to search.</param>
    /// <param name="name">The name of the event to find (case-insensitive).</param>
    /// <param name="nodeEvent">When this method returns, contains the event if found; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if the event was found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetEvent(this IReadOnlyCollection<VNodeEvent> events, string name, out VNodeEvent nodeEvent)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(name);

        foreach (var candidate in events)
        {
            if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                nodeEvent = candidate;
                return true;
            }
        }

        nodeEvent = default;
        return false;
    }
}

