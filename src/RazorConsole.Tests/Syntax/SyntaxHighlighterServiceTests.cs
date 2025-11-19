// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Rendering.Syntax;

namespace RazorConsole.Tests.Syntax;

public sealed class SyntaxHighlighterServiceTests
{
    private readonly SyntaxHighlightingService _service = new(new ColorCodeLanguageRegistry(), new SyntaxThemeRegistry(), new SpectreMarkupFormatter());

    [Fact]
    public void Highlight_WithCSharpCode_ProducesSpectreMarkup()
    {
        var request = new SyntaxHighlightRequest("public class Foo { }", "csharp", null, false, SyntaxOptions.Default);

        var result = _service.Highlight(request);

        Assert.NotEmpty(result.Lines);
        Assert.Contains("public", result.Lines[0], StringComparison.Ordinal);
        Assert.Contains('[', result.Lines[0]);
    }

    [Fact]
    public void Highlight_WithEmptyCode_ReturnsPlaceholder()
    {
        var request = new SyntaxHighlightRequest(string.Empty, "csharp", null, true, SyntaxOptions.Default);

        var result = _service.Highlight(request);

        Assert.Empty(result.Lines);
        Assert.True(result.ShowLineNumbers);
        Assert.Equal(SyntaxOptions.Default.PlaceholderMarkup, result.PlaceholderMarkup);
    }

    [Fact]
    public void Highlight_WithTabWidth_ReplacesTabs()
    {
        var options = SyntaxOptions.Default with { TabWidth = 2 };
        var request = new SyntaxHighlightRequest("\tConsole.WriteLine();", "csharp", null, false, options);

        var result = _service.Highlight(request);

        Assert.NotEmpty(result.Lines);
        Assert.DoesNotContain('\t', result.Lines[0]);
    }

    [Fact]
    public void EncodeDecodePayload_RoundTripsModel()
    {
        var model = new SyntaxHighlightRenderModel(new[] { "[blue]line[/]" }, true, "blue", "placeholder");

        var encoded = SyntaxHighlightingService.EncodePayload(model);
        var decoded = SyntaxHighlightingService.DecodePayload(encoded);

        Assert.Equal(model.ShowLineNumbers, decoded.ShowLineNumbers);
        Assert.Equal(model.LineNumberStyleMarkup, decoded.LineNumberStyleMarkup);
        Assert.Equal(model.PlaceholderMarkup, decoded.PlaceholderMarkup);
        Assert.Equal(model.Lines, decoded.Lines);
    }
}
