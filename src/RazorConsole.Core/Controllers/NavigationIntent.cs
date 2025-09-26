using System;

namespace RazorConsole.Core.Controllers;

/// <summary>
/// Represents the intent produced by a controller after it finishes executing.
/// </summary>
public readonly struct NavigationIntent : IEquatable<NavigationIntent>
{
    private NavigationIntent(NavigationIntentType type, string? target)
    {
        Type = type;
        Target = target;
    }

    public NavigationIntentType Type { get; }

    public string? Target { get; }

    public static NavigationIntent Stay { get; } = new(NavigationIntentType.Stay, null);

    public static NavigationIntent Exit { get; } = new(NavigationIntentType.Exit, null);

    public static NavigationIntent Navigate(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Route target cannot be null or whitespace.", nameof(target));
        }

        return new NavigationIntent(NavigationIntentType.Navigate, target);
    }

    public bool Equals(NavigationIntent other)
        => Type == other.Type && string.Equals(Target, other.Target, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is NavigationIntent other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine((int)Type, Target);

    public override string ToString()
        => Type switch
        {
            NavigationIntentType.Stay => "Stay",
            NavigationIntentType.Exit => "Exit",
            NavigationIntentType.Navigate => $"Navigate({Target})",
            _ => Type.ToString()
        };
}

public enum NavigationIntentType
{
    Stay = 0,
    Navigate = 1,
    Exit = 2
}
