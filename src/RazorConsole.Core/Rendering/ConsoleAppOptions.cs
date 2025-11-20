using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;

/// <summary>
/// Options that control how console applications render output.
/// </summary>
public sealed class ConsoleAppOptions
{
    /// <summary>
    /// Gets or sets whether the console should be cleared before writing output.
    /// </summary>
    public bool AutoClearConsole { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the alternate screen buffer is enabled.
    /// </summary>
    /// <remarks>When enabled, output is directed to the alternate screen buffer, which allows temporary
    /// display changes without affecting the main screen. This is commonly used in terminal applications to present
    /// full-screen interfaces that restore the original content upon exit.</remarks>
    public bool AlternateScreen { get; set; } = true;

    public ConsoleLiveDisplayOptions ConsoleLiveDisplayOptions { get; } = ConsoleLiveDisplayOptions.Default;

    /// <summary>
    /// Callback invoked after a component has been rendered.
    /// </summary>
    public Func<ConsoleLiveDisplayContext, ConsoleViewResult, CancellationToken, Task>? AfterRenderAsync { get; set; } = DefaultAfterRenderAsync;

    internal static Task DefaultAfterRenderAsync(ConsoleLiveDisplayContext context, ConsoleViewResult view, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
