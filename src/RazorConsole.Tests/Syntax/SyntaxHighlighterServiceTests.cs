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

        result.Lines.ShouldNotBeEmpty();
        result.Lines[0].ShouldContain("public");
        result.Lines[0].ShouldContain('[');
    }

    [Fact]
    public void Highlight_WithEmptyCode_ReturnsPlaceholder()
    {
        var request = new SyntaxHighlightRequest(string.Empty, "csharp", null, true, SyntaxOptions.Default);

        var result = _service.Highlight(request);

        result.Lines.ShouldBeEmpty();
        result.ShowLineNumbers.ShouldBeTrue();
        result.PlaceholderMarkup.ShouldBe(SyntaxOptions.Default.PlaceholderMarkup);
    }

    [Fact]
    public void Highlight_WithTabWidth_ReplacesTabs()
    {
        var options = SyntaxOptions.Default with { TabWidth = 2 };
        var request = new SyntaxHighlightRequest("\tConsole.WriteLine();", "csharp", null, false, options);

        var result = _service.Highlight(request);

        result.Lines.ShouldNotBeEmpty();
        result.Lines[0].ShouldNotContain('\t');
    }

    [Fact]
    public void EncodeDecodePayload_RoundTripsModel()
    {
        var model = new SyntaxHighlightRenderModel(new[] { "[blue]line[/]" }, true, "blue", "placeholder");

        var encoded = SyntaxHighlightingService.EncodePayload(model);
        var decoded = SyntaxHighlightingService.DecodePayload(encoded);

        decoded.ShowLineNumbers.ShouldBe(model.ShowLineNumbers);
        decoded.LineNumberStyleMarkup.ShouldBe(model.LineNumberStyleMarkup);
        decoded.PlaceholderMarkup.ShouldBe(model.PlaceholderMarkup);
        decoded.Lines.ShouldBe(model.Lines);
    }
}
