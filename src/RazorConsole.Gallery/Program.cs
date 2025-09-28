using RazorConsole.Components;
using RazorConsole.Core;
using RazorConsole.Gallery.Models;

var model = new GreetingModel
{
    Timestamp = DateTime.Now,
};

await ConsoleApp.RunAsync<HelloComponent>(
    new { Model = model },
    builder =>
    {
        builder.Options.AfterRenderAsync = async (context, _, token) =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                model.Timestamp = DateTime.Now;

                await context.UpdateModelAsync(() => new { Model = model }, token).ConfigureAwait(false);
            }
        };
    });
