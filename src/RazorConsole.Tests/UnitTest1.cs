using System;
using RazorConsole.Core.Rendering;
using Spectre.Console;
using Xunit;

namespace RazorConsole.Tests;

public class HtmlRenderingTests
{
    [Fact]
    public void Convert_ReturnsEmpty_WhenHtmlIsNullOrWhitespace()
    {
        var empty = HtmlToSpectreMarkupConverter.Convert("   ");
        Assert.Equal(string.Empty, empty);
    }

    [Fact]
    public void TryCreatePanel_ReturnsTrue_ForBorderMarkup()
    {
        const string html = "<div data-border=\"panel\" data-header=\"Header\">Hello</div>";

        var success = SpectrePanelFactory.TryCreatePanel(html, out var panel);

        Assert.True(success);
        Assert.NotNull(panel);
        Assert.Equal("Header", panel!.Header?.Text);
    }

    [Fact]
    public void Convert_RendersStyledTextComponent()
    {
        const string html = "<span data-text=\"true\" data-style=\"green\">Hello &amp; Welcome</span>";

        var markup = HtmlToSpectreMarkupConverter.Convert(html);

        Assert.Equal("[green]Hello & Welcome[/]", markup);
    }

    [Fact]
    public void Convert_RendersMarkupTextComponentWithoutEscaping()
    {
        const string html = "<span data-text=\"true\" data-ismarkup=\"true\">[bold]Hi[/]</span>";

        var markup = HtmlToSpectreMarkupConverter.Convert(html);

        Assert.Equal("[bold]Hi[/]", markup);
    }

    [Fact]
    public void Convert_AddsLineBreaksForNewlineComponent()
    {
        const string html = "<p>Start</p><div data-newline=\"true\" data-count=\"2\"></div><p>End</p>";

        var markup = HtmlToSpectreMarkupConverter.Convert(html);

        Assert.Contains(string.Concat(Environment.NewLine, Environment.NewLine), markup);
    }

    [Fact]
    public void Convert_RendersSpacerWithFillCharacters()
    {
        const string html = "<div data-spacer=\"true\" data-lines=\"2\" data-fill=\"#\"></div>";

        var markup = HtmlToSpectreMarkupConverter.Convert(html);

        Assert.Equal("#" + Environment.NewLine + "#", markup);
    }

    [Fact]
    public void Convert_RendersSpinnerWithStyle()
    {
        const string html = "<div data-spinner=\"true\" data-message=\"Working\" data-style=\"yellow\" data-spinner-type=\"Dots\"></div>";

        var markup = HtmlToSpectreMarkupConverter.Convert(html);

        Assert.Equal("[yellow]⠋ Working[/]", markup);
    }
}
