using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Renderables;
using RazorConsole.Core.Rendering;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core;

internal sealed class LiveDisplayCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
{
    private DiffRenderable? _current;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public void UpdateTarget(IRenderable? renderable)
    {
        if (_current is null && renderable is null)
        {
            return;
        }

        if (_current is not null && renderable is not null && ReferenceEquals(_current, renderable))
        {
            return;
        }

        if (!_semaphore.Wait(100))
        {
            return;
        }

        if (_current is null && renderable is not null)
        {
            _current = new DiffRenderable(AnsiConsole.Console, renderable);
            AnsiConsole.Write(_current);
        }
        else if (_current is not null && renderable is not null)
        {
            _current.UpdateRenderable(renderable);
            AnsiConsole.Write(_current);
        }

        _semaphore.Release();
    }

    public void Refresh()
    {
        if (_current is not null)
        {
            AnsiConsole.Write(new ControlCode(string.Empty));
            AnsiConsole.Write(_current);
        }
    }

    public bool TryReplaceNode(IReadOnlyList<int> path, IRenderable renderable)
        => false;

    public bool TryUpdateText(IReadOnlyList<int> path, string? text)
        => false;

    public bool TryUpdateAttributes(IReadOnlyList<int> path, IReadOnlyDictionary<string, string?> attributes)
        => false;
}
