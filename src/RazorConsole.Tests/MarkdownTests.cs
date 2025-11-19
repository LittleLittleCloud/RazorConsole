// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Components;
using RazorConsole.Core.Rendering.Markdown;

namespace RazorConsole.Tests;

public sealed class MarkdownTests
{
    [Fact]
    public void MarkdownRenderingService_ConvertsSimpleMarkdownToHtml()
    {
        var service = new MarkdownRenderingService();
        var markdown = "# Hello World";

        var html = service.ConvertToHtml(markdown);

        Assert.NotNull(html);
        Assert.Contains("<h1", html);
        Assert.Contains("Hello World", html);
        Assert.Contains("</h1>", html);
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsBoldAndItalic()
    {
        var service = new MarkdownRenderingService();
        var markdown = "This is **bold** and *italic*";

        var html = service.ConvertToHtml(markdown);

        Assert.NotNull(html);
        Assert.Contains("<strong>", html);
        Assert.Contains("<em>", html);
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsCodeBlocks()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"```csharp
Console.WriteLine(""test"");
```";

        var html = service.ConvertToHtml(markdown);

        Assert.NotNull(html);
        Assert.Contains("<pre>", html);
        Assert.Contains("<code", html);
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsLists()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"- Item 1
- Item 2
- Item 3";

        var html = service.ConvertToHtml(markdown);

        Assert.NotNull(html);
        Assert.Contains("<ul>", html);
        Assert.Contains("<li>", html);
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsOrderedLists()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"1. First
2. Second
3. Third";

        var html = service.ConvertToHtml(markdown);

        Assert.NotNull(html);
        Assert.Contains("<ol>", html);
        Assert.Contains("<li>", html);
    }

    [Fact]
    public async Task Markdown_ComponentRendersWithoutError()
    {
        var services = new ServiceCollection();
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
