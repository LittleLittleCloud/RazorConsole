using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.Syntax;

public sealed record SyntaxHighlightRequest(
    string Code,
    string? Language,
    string? Theme,
    bool ShowLineNumbers,
    SyntaxOptions Options);

public sealed record SyntaxHighlightRenderModel(
    IReadOnlyList<string> Lines,
    bool ShowLineNumbers,
    string LineNumberStyleMarkup,
    string PlaceholderMarkup);

public sealed class SyntaxHighlightingService
{
    private readonly ISyntaxLanguageRegistry _languageRegistry;
    private readonly ISyntaxThemeRegistry _themeRegistry;
    private readonly SpectreMarkupFormatter _formatter;

    public SyntaxHighlightingService(
        ISyntaxLanguageRegistry languageRegistry,
        ISyntaxThemeRegistry themeRegistry,
        SpectreMarkupFormatter formatter)
    {
        _languageRegistry = languageRegistry ?? throw new ArgumentNullException(nameof(languageRegistry));
        _themeRegistry = themeRegistry ?? throw new ArgumentNullException(nameof(themeRegistry));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    public SyntaxHighlightRenderModel Highlight(SyntaxHighlightRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var options = request.Options ?? SyntaxOptions.Default;
        if (string.IsNullOrEmpty(request.Code))
        {
            return new SyntaxHighlightRenderModel(Array.Empty<string>(), request.ShowLineNumbers, options.LineNumberStyle.ToMarkup(), options.PlaceholderMarkup);
        }

        var language = _languageRegistry.GetLanguage(request.Language);
        var theme = _themeRegistry.GetTheme(request.Theme);
        var markup = _formatter.Format(request.Code, language, theme, options);
        var normalized = NormalizeLines(markup);

        return new SyntaxHighlightRenderModel(normalized, request.ShowLineNumbers, options.LineNumberStyle.ToMarkup(), options.PlaceholderMarkup);
    }

    public static string EncodePayload(SyntaxHighlightRenderModel model)
    {
        var payload = new SyntaxHighlightPayload(model.ShowLineNumbers, model.LineNumberStyleMarkup, model.PlaceholderMarkup, model.Lines.ToArray());
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        return Convert.ToBase64String(bytes);
    }

    public static SyntaxHighlightRenderModel DecodePayload(string encoded)
    {
        if (string.IsNullOrEmpty(encoded))
        {
            return new SyntaxHighlightRenderModel(Array.Empty<string>(), false, Style.Plain.ToMarkup(), SyntaxOptions.Default.PlaceholderMarkup);
        }

        var bytes = Convert.FromBase64String(encoded);
        var payload = JsonSerializer.Deserialize<SyntaxHighlightPayload>(bytes) ?? new SyntaxHighlightPayload(false, Style.Plain.ToMarkup(), SyntaxOptions.Default.PlaceholderMarkup, Array.Empty<string>());
        return new SyntaxHighlightRenderModel(payload.Lines, payload.ShowLineNumbers, payload.LineNumberStyleMarkup, payload.PlaceholderMarkup);
    }

    private static IReadOnlyList<string> NormalizeLines(string markup)
    {
        if (string.IsNullOrEmpty(markup))
        {
            return Array.Empty<string>();
        }

        return markup.Replace("\r\n", "\n", StringComparison.Ordinal)
                     .Replace("\r", "\n", StringComparison.Ordinal)
                     .Split('\n');
    }

    private sealed record SyntaxHighlightPayload(bool ShowLineNumbers, string LineNumberStyleMarkup, string PlaceholderMarkup, string[] Lines);
}
