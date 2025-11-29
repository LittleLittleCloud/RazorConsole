// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Core.Rendering.Markdown;

namespace RazorConsole.Tests.Rendering.Markdown;

public sealed class MarkdownTests
{
    [Fact]
    public void MarkdownRenderingService_ConvertsSimpleMarkdownToHtml()
    {
        var service = new MarkdownRenderingService();
        var markdown = "# Hello World";

        var html = service.ConvertToHtml(markdown);

        html.ShouldNotBeNull();
        html.ShouldContain("<h1");
        html.ShouldContain("Hello World");
        html.ShouldContain("</h1>");
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsBoldAndItalic()
    {
        var service = new MarkdownRenderingService();
        var markdown = "This is **bold** and *italic*";

        var html = service.ConvertToHtml(markdown);

        html.ShouldNotBeNull();
        html.ShouldContain("<strong>");
        html.ShouldContain("<em>");
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsCodeBlocks()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"```csharp
Console.WriteLine(""test"");
```";

        var html = service.ConvertToHtml(markdown);

        html.ShouldNotBeNull();
        html.ShouldContain("<pre>");
        html.ShouldContain("<code");
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsLists()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"- Item 1
- Item 2
- Item 3";

        var html = service.ConvertToHtml(markdown);

        html.ShouldNotBeNull();
        html.ShouldContain("<ul>");
        html.ShouldContain("<li>");
    }

    [Fact]
    public void MarkdownRenderingService_ConvertsOrderedLists()
    {
        var service = new MarkdownRenderingService();
        var markdown = @"1. First
2. Second
3. Third";

        var html = service.ConvertToHtml(markdown);

        html.ShouldNotBeNull();
        html.ShouldContain("<ol>");
        html.ShouldContain("<li>");
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

        var snapshot = await renderer.MountComponentAsync<RazorConsole.Components.Markdown>(parameters, CancellationToken.None);

        snapshot.Root.ShouldNotBeNull();
    }
}

