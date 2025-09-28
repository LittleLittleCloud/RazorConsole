using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Core;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests;

public sealed class ConsoleAppTests
{
    [Fact]
    public async Task Renderer_ReturnsConsoleViewResult()
    {
        await using var app = AppHost.Create<TestComponent>();

        var renderer = app.Services.GetRequiredService<IRazorComponentRenderer>();

        var view = await renderer.RenderAsync<TestComponent>(new { Message = "Hello" });

        Assert.NotNull(view);
        Assert.IsType<ConsoleViewResult>(view);
        Assert.Contains("Hello", view.Html, StringComparison.Ordinal);
        Assert.NotNull(view.Renderable);
    }

    [Fact]
    public async Task RunAsync_InvokesCustomAfterRenderCallback()
    {
        ConsoleViewResult? observed = null;
        var tcs = new TaskCompletionSource<ConsoleViewResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var cts = new CancellationTokenSource();

        var runTask = AppHost.RunAsync<TestComponent>(new { Message = "Callback" }, builder =>
        {
            builder.Configure(options =>
            {
                options.AutoClearConsole = false;
                options.AfterRenderAsync = (context, view, _) =>
                {
                    observed = view;
                    tcs.TrySetResult(view);
                    return Task.CompletedTask;
                };
            });
        }, cts.Token);

    var result = await tcs.Task;

        cts.Cancel();
        await runTask;

        Assert.NotNull(observed);
        Assert.Same(observed, result);
        Assert.Contains("Callback", result.Html, StringComparison.Ordinal);
    }

    private sealed class TestComponent : ComponentBase
    {
        [Parameter]
        public string? Message { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "data-text", "true");
            builder.AddContent(2, Message ?? string.Empty);
            builder.CloseElement();
        }
    }
}
