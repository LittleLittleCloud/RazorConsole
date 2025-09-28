using System;
using System.Threading;
using System.Threading.Tasks;

namespace RazorConsole.Core.Rendering;

/// <summary>
/// Provides access to the active <see cref="ConsoleLiveDisplayContext"/> so components can request re-renders.
/// </summary>
public sealed class LiveDisplayContextAccessor
{
    private readonly object _sync = new();
    private ConsoleLiveDisplayContext? _context;

    /// <summary>
    /// Gets the current live display context, if any.
    /// </summary>
    public ConsoleLiveDisplayContext? Current
    {
        get
        {
            lock (_sync)
            {
                return _context;
            }
        }
    }

    /// <summary>
    /// Attaches the supplied live display context.
    /// </summary>
    /// <param name="context">The context to expose.</param>
    public void Attach(ConsoleLiveDisplayContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        lock (_sync)
        {
            _context = context;
        }
    }

    /// <summary>
    /// Detaches the live display context if it matches the currently tracked instance.
    /// </summary>
    /// <param name="context">The context that is being disposed.</param>
    public void Detach(ConsoleLiveDisplayContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        lock (_sync)
        {
            if (ReferenceEquals(_context, context))
            {
                _context = null;
            }
        }
    }

    /// <summary>
    /// Requests a re-render using the supplied parameters.
    /// </summary>
    /// <param name="parameters">Parameters to pass to the component. Pass <see langword="null"/> to reuse the previous parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when an update was applied; otherwise <see langword="false"/>.</returns>
    public Task<bool> UpdateModelAsync(object? parameters, CancellationToken cancellationToken = default)
    {
        var context = Current;
        return context is not null
            ? context.UpdateModelAsync(parameters, cancellationToken)
            : Task.FromResult(false);
    }
}
