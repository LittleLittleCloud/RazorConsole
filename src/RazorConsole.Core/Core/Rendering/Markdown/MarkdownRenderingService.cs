// Copyright (c) RazorConsole. All rights reserved.

using Markdig;

namespace RazorConsole.Core.Rendering.Markdown;

public sealed class MarkdownRenderingService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownRenderingService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string ConvertToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var html = Markdig.Markdown.ToHtml(markdown, _pipeline);
        // Wrap in a root element for XML parsing
        return $"<div>{html}</div>";
    }
}
