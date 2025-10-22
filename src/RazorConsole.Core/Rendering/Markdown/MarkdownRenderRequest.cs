namespace RazorConsole.Core.Rendering.Markdown;

public sealed record MarkdownRenderRequest(
    string Content,
    bool ShowCodeLineNumbers);
