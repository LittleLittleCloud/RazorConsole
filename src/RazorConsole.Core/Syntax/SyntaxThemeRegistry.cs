// Copyright (c) RazorConsole. All rights reserved.

using System.Collections.Concurrent;

namespace RazorConsole.Core.Rendering.Syntax;

public interface ISyntaxThemeRegistry
{
    SyntaxTheme GetTheme(string? key);
    void Register(string key, SyntaxTheme theme);
    IReadOnlyCollection<string> RegisteredThemeKeys { get; }
}

public sealed class SyntaxThemeRegistry : ISyntaxThemeRegistry
{
    public const string DefaultThemeKey = "default";

    private readonly ConcurrentDictionary<string, SyntaxTheme> _themes = new(StringComparer.OrdinalIgnoreCase);

    public SyntaxThemeRegistry()
    {
        Register(DefaultThemeKey, SyntaxTheme.CreateDefault());
    }

    public SyntaxTheme GetTheme(string? key)
    {
        key ??= DefaultThemeKey;
        if (_themes.TryGetValue(key, out var theme))
        {
            return theme;
        }

        return _themes[DefaultThemeKey];
    }

    public void Register(string key, SyntaxTheme theme)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Theme key cannot be null or whitespace.", nameof(key));
        }

        if (theme is null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        _themes[key] = theme;
    }

    public IReadOnlyCollection<string> RegisteredThemeKeys => _themes.Keys.ToArray();
}
