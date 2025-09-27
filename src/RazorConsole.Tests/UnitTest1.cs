using System;
using RazorConsole.Core.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;
using Xunit;

namespace RazorConsole.Tests;

public class HtmlRenderingTests
{
    [Fact]
    public void Convert_ReturnsEmpty_WhenHtmlIsNullOrWhitespace()
    {
        var empty = HtmlToSpectreRenderableConverter.Convert("   ");
        Assert.Equal(string.Empty, empty);
    }

    [Fact]
    public void TryCreateRenderable_ReturnsPanel_ForBorderMarkup()
    {
        const string html = "<div data-border=\"panel\" data-header=\"Header\">Hello</div>";

        var success = SpectreRenderableFactory.TryCreateRenderable(html, out var renderable);

        Assert.True(success);
        var panel = Assert.IsType<Panel>(renderable);
        Assert.Equal("Header", panel.Header?.Text);
    }

    [Fact]
    public void Convert_RendersStyledTextComponent()
    {
        const string html = "<span data-text=\"true\" data-style=\"green\">Hello &amp; Welcome</span>";

        var markup = HtmlToSpectreRenderableConverter.Convert(html);

        Assert.Equal("[green]Hello & Welcome[/]", markup);
    }

    [Fact]
    public void Convert_RendersMarkupTextComponentWithoutEscaping()
    {
        const string html = "<span data-text=\"true\" data-ismarkup=\"true\">[bold]Hi[/]</span>";

        var markup = HtmlToSpectreRenderableConverter.Convert(html);

        Assert.Equal("[bold]Hi[/]", markup);
    }

    [Fact]
    public void Convert_AddsLineBreaksForNewlineComponent()
    {
        const string html = "<p>Start</p><div data-newline=\"true\" data-count=\"2\"></div><p>End</p>";

        var markup = HtmlToSpectreRenderableConverter.Convert(html);

        Assert.Contains(string.Concat(Environment.NewLine, Environment.NewLine), markup);
    }

    [Fact]
    public void Convert_RendersSpacerWithFillCharacters()
    {
        const string html = "<div data-spacer=\"true\" data-lines=\"2\" data-fill=\"#\"></div>";

        var markup = HtmlToSpectreRenderableConverter.Convert(html);

        Assert.Equal("#" + Environment.NewLine + "#", markup);
    }

    [Fact]
    public void Convert_RendersSpinnerWithStyle()
    {
        const string html = "<div data-spinner=\"true\" data-message=\"Working\" data-style=\"yellow\" data-spinner-type=\"Dots\"></div>";

        var markup = HtmlToSpectreRenderableConverter.Convert(html);

        Assert.Equal("[yellow]⠋ Working[/]", markup);
    }

    [Fact]
    public void Convert_ReturnsInnerMarkupForBorderComponent()
    {
        const string html = "<div data-border=\"panel\" data-header=\"Header\" data-border-color=\"steelblue\"><span data-text=\"true\">Inner</span></div>";

        var markup = HtmlToSpectreRenderableConverter.Convert(html);

        Assert.Equal("Inner", markup);
    }
}
