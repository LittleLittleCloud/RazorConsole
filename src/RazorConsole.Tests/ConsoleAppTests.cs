// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;
using RazorConsole.Core.Controllers;

namespace RazorConsole.Tests;

public sealed class ConsoleAppTests
{
    [Fact]
    public async Task RunAsync_InvokesCustomAfterRenderCallback()
    {
        ConsoleViewResult? observed = null;
        var tcs = new TaskCompletionSource<ConsoleViewResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var cts = new CancellationTokenSource();

        Func<Core.Rendering.ConsoleLiveDisplayContext, ConsoleViewResult, CancellationToken, Task>? afterRenderCallback = (context, view, _) =>
        {
            observed = view;
            tcs.TrySetResult(view);
            return Task.CompletedTask;
        };

        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.UseRazorConsole<TestComponent>();
        hostBuilder.Services.Configure<ConsoleAppOptions>(options =>
        {
            options.AfterRenderAsync = afterRenderCallback;
        });

        var host = hostBuilder.Build();

        var runTask = host.RunAsync(cts.Token);

        var result = await tcs.Task;

        cts.Cancel();
        await runTask;

        Assert.NotNull(observed);
        Assert.Same(observed, result);
        Assert.Contains("Callback", result.Html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_DefaultConsoleAppOptionsResolves()
    {
        using var cts = new CancellationTokenSource();

        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.UseRazorConsole<TestComponent>();

        var host = hostBuilder.Build();

        var runTask = host.RunAsync(cts.Token);

        ConsoleAppOptions options = host.Services.GetRequiredService<ConsoleAppOptions>();

        cts.Cancel();
        await runTask;

        Assert.NotNull(options);
        Assert.Equivalent(new ConsoleAppOptions(), options);
    }

    private sealed class TestComponent : ComponentBase
    {
        [Parameter]
        public string Message { get; set; } = "Callback Test";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "data-text", "true");
            builder.AddContent(2, Message ?? string.Empty);
            builder.CloseElement();
        }
    }
}
