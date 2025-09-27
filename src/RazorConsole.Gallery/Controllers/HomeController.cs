using System;
using System.Threading;
using System.Threading.Tasks;
using RazorConsole.Components;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Gallery.Models;
using RazorConsole.Gallery.Services;

namespace RazorConsole.Gallery.Controllers;

public sealed class HomeController : ConsoleController
{
    private readonly GreetingService _greetingService;

    public HomeController(RazorComponentRenderer renderer, GreetingService greetingService)
        : base(renderer)
    {
        _greetingService = greetingService ?? throw new ArgumentNullException(nameof(greetingService));
    }

    public override async Task<NavigationIntent> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var initialModel = _greetingService.GetSnapshot();
        initialModel.Timestamp = DateTime.Now;

        var initialView = await RenderViewAsync<HelloComponent>(new { Model = initialModel }).ConfigureAwait(false);

        await RunLiveDisplayAsync(initialView, async (displayContext, token) =>
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var tickerTask = RunTimestampTickerAsync(displayContext, linkedCts.Token);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await UpdateDisplayAsync(displayContext, token);
                }
            }
            finally
            {
                linkedCts.Cancel();
                try
                {
                    await tickerTask;
                }
                catch (OperationCanceledException)
                {
                }
            }
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        ClearOutput();
        return NavigationIntent.Exit;

        async Task RunTimestampTickerAsync(ConsoleLiveDisplayContext context, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!token.IsCancellationRequested)
                {
                    await UpdateDisplayAsync(context, token).ConfigureAwait(false);
                }
            }
        }

        async Task UpdateDisplayAsync(ConsoleLiveDisplayContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var snapshot = _greetingService.GetSnapshot();
            snapshot.Timestamp = DateTime.Now;

            var view = await RenderViewAsync<HelloComponent>(new { Model = snapshot }).ConfigureAwait(false);
            context.UpdateView(view);
        }
    }
}
