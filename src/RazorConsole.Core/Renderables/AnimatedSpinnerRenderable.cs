// Copyright (c) RazorConsole. All rights reserved.

using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal sealed class AnimatedSpinnerRenderable : IRenderable, IAnimatedConsoleRenderable
{
    private readonly Spinner _spinner;
    private readonly string? _message;
    private readonly string? _style;
    private readonly TimeSpan _interval;

    public AnimatedSpinnerRenderable(Spinner spinner, string? message, string? style, bool autoDismiss)
    {
        _spinner = spinner ?? throw new ArgumentNullException(nameof(spinner));
        _message = message;
        _style = style;
        _ = autoDismiss; // Placeholder for future support when auto-dismiss semantics are defined.
        _interval = spinner.Interval <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(100) : spinner.Interval;
    }

    public TimeSpan RefreshInterval => _interval;

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        var markup = new Markup(BuildMarkup());
        return ((IRenderable)markup).Measure(options, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var markup = new Markup(BuildMarkup());
        return ((IRenderable)markup).Render(options, maxWidth);
    }

    private string BuildMarkup()
    {
        var frame = ResolveFrame();
        var content = string.IsNullOrWhiteSpace(_message) ? frame : string.Concat(frame, " ", _message);
        return ComponentMarkupUtilities.CreateStyledMarkup(_style, content, requiresEscape: true);
    }

    private string ResolveFrame()
    {
        var frames = _spinner.Frames;
        if (frames.Count == 0)
        {
            return "";
        }

        // Use global time so new instances continue the animation instead of restarting.
        var intervalTicks = _interval.Ticks;

        // Use DateTime.UtcNow.Ticks as continuous clock.
        var tick = DateTime.UtcNow.Ticks / intervalTicks;
        var index = (int)(tick % frames.Count);
        return frames[index];
    }
}
