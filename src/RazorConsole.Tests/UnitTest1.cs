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
}
