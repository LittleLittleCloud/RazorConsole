using System.Collections.Generic;

namespace RazorConsole.Core.Rendering.Markdown;

public abstract record MarkdownElement;

public sealed record HeadingElement(int Level, string Content) : MarkdownElement;

public sealed record ParagraphElement(string Content) : MarkdownElement;

public sealed record CodeBlockElement(string Code, string? Language, bool ShowLineNumbers) : MarkdownElement;

public sealed record ListElement(bool IsOrdered, List<string> Items) : MarkdownElement;

public sealed record QuoteElement(string Content) : MarkdownElement;

public sealed record ThematicBreakElement() : MarkdownElement;
