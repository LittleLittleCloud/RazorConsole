// Copyright (c) RazorConsole. All rights reserved.

using System.Collections.Concurrent;
using ColorCode;
using ColorCode.Common;

namespace RazorConsole.Core.Rendering.Syntax;

public interface ISyntaxLanguageRegistry
{
    ILanguage GetLanguage(string? key);
    void Register(string key, ILanguage language);
    IReadOnlyCollection<string> RegisteredLanguageKeys { get; }
}

public sealed class ColorCodeLanguageRegistry : ISyntaxLanguageRegistry
{
    private readonly ConcurrentDictionary<string, ILanguage> _languages = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILanguage _plainText = new PlainTextLanguage();

    public ColorCodeLanguageRegistry()
    {
        RegisterDefaults();
    }

    public ILanguage GetLanguage(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return _plainText;
        }

        if (_languages.TryGetValue(key, out var language))
        {
            return language;
        }

        return _plainText;
    }

    public void Register(string key, ILanguage language)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Language key cannot be null or whitespace.", nameof(key));
        }

        if (language is null)
        {
            throw new ArgumentNullException(nameof(language));
        }

        _languages[key] = language;
    }

    public IReadOnlyCollection<string> RegisteredLanguageKeys => _languages.Keys.ToArray();

    private void RegisterDefaults()
    {
        Register("text", _plainText);
        Register("plaintext", _plainText);
        Register("plain", _plainText);
        Register("csharp", Languages.FindById(LanguageId.CSharp));
        Register("cs", Languages.FindById(LanguageId.CSharp));
        Register("razor", Languages.Html); // Razor currently tokenizes as HTML/C# hybrid
        Register("html", Languages.Html);
        Register("json", Languages.FindById(LanguageId.Json));
        Register("xml", Languages.Xml);
        Register("sql", Languages.Sql);
        Register("js", Languages.JavaScript);
        Register("javascript", Languages.JavaScript);
        Register("ts", Languages.FindById(LanguageId.TypeScript));
        Register("typescript", Languages.FindById(LanguageId.TypeScript));
        Register("css", Languages.Css);
        Register("powershell", Languages.PowerShell);
        Register("ps", Languages.PowerShell);
        Register("python", Languages.Python);
        Register("md", Languages.Markdown);
        Register("markdown", Languages.Markdown);
    }
}
