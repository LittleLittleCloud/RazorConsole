using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ColorCode.Common;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.Syntax;

/// <summary>
/// Represents a mapping between ColorCode scopes and Spectre.Console styles.
/// </summary>
public class SyntaxTheme
{
    private readonly IReadOnlyDictionary<string, Style> _scopedStyles;

    protected SyntaxTheme(string name, Style defaultStyle, IReadOnlyDictionary<string, Style>? scopedStyles)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DefaultStyle = defaultStyle;
        _scopedStyles = scopedStyles ?? new ReadOnlyDictionary<string, Style>(new Dictionary<string, Style>(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the name of the theme.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the default style when a scope is not matched.
    /// </summary>
    public Style DefaultStyle { get; }

    /// <summary>
    /// Gets the style for the specified ColorCode scope name.
    /// </summary>
    public virtual Style GetStyle(string? scopeName)
    {
        if (string.IsNullOrEmpty(scopeName))
        {
            return DefaultStyle;
        }

        if (_scopedStyles.TryGetValue(scopeName, out var style))
        {
            return style;
        }

        // Allow lookups using canonical scope names even if the stored keys differ in casing.
        if (_scopedStyles.TryGetValue(scopeName.Trim(), out style))
        {
            return style;
        }

        return DefaultStyle;
    }

    /// <summary>
    /// Creates the default syntax theme.
    /// </summary>
    public static SyntaxTheme CreateDefault()
    {
        var defaultStyle = Style.Plain;
        var styles = new Dictionary<string, Style>(StringComparer.OrdinalIgnoreCase)
        {
            [ScopeName.Comment] = new Style(Color.FromHex("#6A9955"), decoration: Decoration.Italic),
            [ScopeName.Keyword] = new Style(Color.FromHex("#569CD6")),
            [ScopeName.ControlKeyword] = new Style(Color.FromHex("#569CD6"), decoration: Decoration.Bold),
            [ScopeName.PreprocessorKeyword] = new Style(Color.FromHex("#9CDCFE")),
            [ScopeName.String] = new Style(Color.FromHex("#CE9178")),
            [ScopeName.StringCSharpVerbatim] = new Style(Color.FromHex("#CE9178")),
            [ScopeName.Number] = new Style(Color.FromHex("#B5CEA8")),
            [ScopeName.Operator] = new Style(Color.FromHex("#D4D4D4")),
            [ScopeName.Delimiter] = new Style(Color.FromHex("#D4D4D4")),
            [ScopeName.ClassName] = new Style(Color.FromHex("#4EC9B0")),
            [ScopeName.Type] = new Style(Color.FromHex("#4EC9B0")),
            [ScopeName.TypeVariable] = new Style(Color.FromHex("#4EC9B0")),
            [ScopeName.NameSpace] = new Style(Color.FromHex("#4EC9B0")),
            [ScopeName.Constructor] = new Style(Color.FromHex("#4EC9B0")),
            [ScopeName.Attribute] = new Style(Color.FromHex("#C586C0")),
            [ScopeName.BuiltinFunction] = new Style(Color.FromHex("#DCDCAA")),
            [ScopeName.BuiltinValue] = new Style(Color.FromHex("#DCDCAA")),
            [ScopeName.SpecialCharacter] = new Style(Color.FromHex("#D7BA7D")),
            [ScopeName.JsonKey] = new Style(Color.FromHex("#9CDCFE")),
            [ScopeName.JsonString] = new Style(Color.FromHex("#CE9178")),
            [ScopeName.JsonNumber] = new Style(Color.FromHex("#B5CEA8")),
            [ScopeName.JsonConst] = new Style(Color.FromHex("#DCDCAA")),
            [ScopeName.HtmlElementName] = new Style(Color.FromHex("#569CD6")),
            [ScopeName.HtmlAttributeName] = new Style(Color.FromHex("#9CDCFE")),
            [ScopeName.HtmlAttributeValue] = new Style(Color.FromHex("#CE9178")),
            [ScopeName.HtmlTagDelimiter] = new Style(Color.FromHex("#808080")),
            [ScopeName.HtmlOperator] = new Style(Color.FromHex("#808080")),
            [ScopeName.HtmlComment] = new Style(Color.FromHex("#6A9955"), decoration: Decoration.Italic),
            [ScopeName.MarkdownHeader] = new Style(Color.FromHex("#569CD6"), decoration: Decoration.Bold),
            [ScopeName.MarkdownCode] = new Style(Color.FromHex("#CE9178")),
            [ScopeName.MarkdownListItem] = new Style(Color.FromHex("#9CDCFE")),
            [ScopeName.MarkdownEmph] = new Style(Color.FromHex("#9CDCFE"), decoration: Decoration.Italic),
            [ScopeName.MarkdownBold] = new Style(Color.FromHex("#9CDCFE"), decoration: Decoration.Bold),
            [ScopeName.PowerShellCommand] = new Style(Color.FromHex("#4EC9B0")),
            [ScopeName.PowerShellParameter] = new Style(Color.FromHex("#9CDCFE")),
            [ScopeName.PowerShellVariable] = new Style(Color.FromHex("#CE9178")),
        };

        return new SyntaxTheme("default", defaultStyle, new ReadOnlyDictionary<string, Style>(styles));
    }
}
