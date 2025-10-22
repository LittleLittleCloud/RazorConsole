using System.Collections.Generic;

namespace RazorConsole.Core.Rendering.Markdown;

public sealed record MarkdownRenderModel(
    List<MarkdownElement> Elements);
