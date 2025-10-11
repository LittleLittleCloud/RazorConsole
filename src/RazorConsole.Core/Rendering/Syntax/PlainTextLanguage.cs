using System;
using System.Collections.Generic;
using ColorCode;
using ColorCode.Compilation;

namespace RazorConsole.Core.Rendering.Syntax;

internal sealed class PlainTextLanguage : ILanguage
{
    public string Id => nameof(PlainTextLanguage).ToLowerInvariant();

    public string FirstLinePattern => string.Empty;

    public string Name => "Plain Text";

    public IList<LanguageRule> Rules { get; } = new List<LanguageRule>();

    public string CssClassName => "plain-text";

    public bool HasAlias(string lang) => string.Equals(lang, "text", StringComparison.OrdinalIgnoreCase) || string.Equals(lang, "plain", StringComparison.OrdinalIgnoreCase);
}
