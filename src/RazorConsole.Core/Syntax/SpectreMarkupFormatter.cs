// Copyright (c) RazorConsole. All rights reserved.

using System.Text;
using ColorCode;
using ColorCode.Parsing;
using ColorCode.Styling;
using Spectre.Console;
using SpectreStyle = Spectre.Console.Style;

namespace RazorConsole.Core.Rendering.Syntax;

public sealed class SpectreMarkupFormatter : CodeColorizerBase
{
    private readonly Dictionary<SpectreStyle, string> _styleMarkupCache = new();
    private SyntaxTheme _currentTheme = null!;
    private SyntaxOptions _options = null!;
    private StringBuilder _builder = null!;

    public SpectreMarkupFormatter()
        : base(StyleDictionary.DefaultLight, languageParser: null)
    {
    }

    public string Format(string sourceCode, ILanguage language, SyntaxTheme theme, SyntaxOptions options)
    {
        if (sourceCode is null)
        {
            throw new ArgumentNullException(nameof(sourceCode));
        }

        if (language is null)
        {
            throw new ArgumentNullException(nameof(language));
        }

        if (theme is null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _builder = new StringBuilder(sourceCode.Length * 2);
        _currentTheme = theme;
        _options = options;
        _styleMarkupCache.Clear();

        languageParser.Parse(sourceCode, language, Write);

        return _builder.ToString();
    }

    protected override void Write(string parsedSourceCode, IList<Scope> scopes)
    {
        if (parsedSourceCode.Length == 0)
        {
            return;
        }

        var span = parsedSourceCode.AsSpan();
        AppendSegment(span, scopes, _currentTheme.DefaultStyle);
    }

    private void AppendSegment(ReadOnlySpan<char> content, IList<Scope> scopes, SpectreStyle parentStyle)
    {
        if (scopes is null || scopes.Count == 0)
        {
            AppendMarkup(content, parentStyle);
            return;
        }

        var ordered = scopes.OrderBy(static scope => scope.Index).ToArray();
        var position = 0;
        foreach (var scope in ordered)
        {
            var relativeIndex = Math.Clamp(scope.Index, 0, content.Length);
            if (relativeIndex > position)
            {
                AppendMarkup(content.Slice(position, relativeIndex - position), parentStyle);
            }

            var scopeLength = Math.Clamp(scope.Length, 0, content.Length - relativeIndex);
            if (scopeLength <= 0)
            {
                continue;
            }

            var segment = content.Slice(relativeIndex, scopeLength);
            var scopeStyle = _currentTheme.GetStyle(scope.Name);
            if (scope.Children.Count > 0)
            {
                AppendSegment(segment, scope.Children, scopeStyle);
            }
            else
            {
                AppendMarkup(segment, scopeStyle);
            }

            position = relativeIndex + scopeLength;
        }

        if (position < content.Length)
        {
            AppendMarkup(content[position..], parentStyle);
        }
    }

    private void AppendMarkup(ReadOnlySpan<char> content, SpectreStyle style)
    {
        if (content.Length == 0)
        {
            return;
        }

        var text = ExpandTabs(content);
        var segments = text.Split('\n');
        for (var i = 0; i < segments.Length; i++)
        {
            AppendStyledToken(segments[i], style);
            if (i < segments.Length - 1)
            {
                _builder.Append('\n');
            }
        }
    }

    private void AppendStyledToken(string text, SpectreStyle style)
    {
        var escaped = Markup.Escape(text);
        if (string.IsNullOrEmpty(escaped))
        {
            return;
        }

        if (style == SpectreStyle.Plain)
        {
            _builder.Append(escaped);
            return;
        }

        var token = GetStyleMarkup(style);
        _builder.Append('[').Append(token).Append(']').Append(escaped).Append("[/]");
    }

    private string GetStyleMarkup(SpectreStyle style)
    {
        if (_styleMarkupCache.TryGetValue(style, out var cached))
        {
            return cached;
        }

        var markup = style.ToMarkup();
        _styleMarkupCache[style] = markup;
        return markup;
    }

    private string ExpandTabs(ReadOnlySpan<char> content)
    {
        if (_options.TabWidth <= 0)
        {
            return content.ToString();
        }

        var tabReplacement = new string(' ', _options.TabWidth);
        return content.ToString().Replace("\r\n", "\n", StringComparison.Ordinal)
                                 .Replace("\r", "\n", StringComparison.Ordinal)
                                 .Replace("\t", tabReplacement, StringComparison.Ordinal);
    }
}
