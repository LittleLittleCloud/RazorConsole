using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core.Rendering.Markdown;
using RazorConsole.Core.Rendering.Syntax;

namespace RazorConsole.Tests;

public sealed class MarkdownTests
{
    [Fact]
    public void MarkdownRenderingService_ParsesSimpleMarkdown()
    {
        var service = new MarkdownRenderingService();
        var request = new MarkdownRenderRequest("# Hello World", ShowCodeLineNumbers: true);

        var result = service.Render(request);

        Assert.NotNull(result);
        Assert.Single(result.Elements);
        var heading = Assert.IsType<HeadingElement>(result.Elements[0]);
        Assert.Equal(1, heading.Level);
        Assert.Equal("Hello World", heading.Content);
    }

    [Fact]
    public void MarkdownRenderingService_ParsesCodeBlocks()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"```csharp
Console.WriteLine(""test"");
```";
        var request = new MarkdownRenderRequest(markdown, ShowCodeLineNumbers: true);

        var result = service.Render(request);

        Assert.NotNull(result);
        Assert.Single(result.Elements);
        var codeBlock = Assert.IsType<CodeBlockElement>(result.Elements[0]);
        Assert.Equal("csharp", codeBlock.Language);
        Assert.Contains("Console.WriteLine", codeBlock.Code);
    }

    [Fact]
    public void MarkdownRenderingService_ParsesLists()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"- Item 1
- Item 2
- Item 3";
        var request = new MarkdownRenderRequest(markdown, ShowCodeLineNumbers: true);

        var result = service.Render(request);

        Assert.NotNull(result);
        Assert.Single(result.Elements);
        var list = Assert.IsType<ListElement>(result.Elements[0]);
        Assert.False(list.IsOrdered);
        Assert.Equal(3, list.Items.Count);
        Assert.Equal("Item 1", list.Items[0]);
        Assert.Equal("Item 2", list.Items[1]);
        Assert.Equal("Item 3", list.Items[2]);
    }

    [Fact]
    public void MarkdownRenderingService_ParsesOrderedLists()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"1. First
2. Second
3. Third";
        var request = new MarkdownRenderRequest(markdown, ShowCodeLineNumbers: true);

        var result = service.Render(request);

        Assert.NotNull(result);
        Assert.Single(result.Elements);
        var list = Assert.IsType<ListElement>(result.Elements[0]);
        Assert.True(list.IsOrdered);
        Assert.Equal(3, list.Items.Count);
    }

    [Fact]
    public void MarkdownRenderingService_EncodesAndDecodesPayload()
    {
        var service = new MarkdownRenderingService();
        var request = new MarkdownRenderRequest("# Test", ShowCodeLineNumbers: true);
        var model = service.Render(request);

        var encoded = MarkdownRenderingService.EncodePayload(model);
        var decoded = MarkdownRenderingService.DecodePayload(encoded);

        Assert.NotNull(decoded);
        Assert.Single(decoded.Elements);
        var heading = Assert.IsType<HeadingElement>(decoded.Elements[0]);
        Assert.Equal(1, heading.Level);
        Assert.Equal("Test", heading.Content);
    }

    [Fact]
    public async Task Markdown_ComponentRendersWithoutError()
    {
        // Create a service collection with required dependencies
        var services = new ServiceCollection();
        services.AddSingleton<ISyntaxLanguageRegistry, ColorCodeLanguageRegistry>();
        services.AddSingleton<ISyntaxThemeRegistry, SyntaxThemeRegistry>();
        services.AddSingleton<SpectreMarkupFormatter>();
        services.AddSingleton<SyntaxHighlightingService>();
        services.AddSingleton<MarkdownRenderingService>();

        var serviceProvider = services.BuildServiceProvider();

        using var renderer = TestHelpers.CreateTestRenderer(serviceProvider);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { "Content", "# Hello World" }
        });

        var snapshot = await renderer.MountComponentAsync<Markdown>(parameters, CancellationToken.None);

        Assert.NotNull(snapshot.Root);
    }
}
