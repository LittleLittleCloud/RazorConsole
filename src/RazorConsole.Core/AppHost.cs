using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Input;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.Syntax;
using RazorConsole.Core.Utilities;
using RazorConsole.Core.Vdom;
using Spectre.Console;

namespace RazorConsole.Core;

/// <summary>
/// Convenience helpers for rendering Razor components directly to the console.
/// </summary>
public static class AppHost
{
    /// <summary>
    /// Creates a <see cref="ConsoleApp{TComponent}"/> instance for the specified component type.
    /// </summary>
    /// <typeparam name="TComponent">Component type to render.</typeparam>
    /// <param name="configure">Optional callback to customize services and options.</param>
    public static ConsoleApp<TComponent> Create<TComponent>(Action<ConsoleAppBuilder>? configure = null)
        where TComponent : IComponent
    {
        var builder = ConsoleAppBuilder.Create();
        configure?.Invoke(builder);
        return new ConsoleApp<TComponent>(builder);
    }

    /// <summary>
    /// Renders the specified component type to the console using default settings.
    /// </summary>
    /// <typeparam name="TComponent">Component type to render.</typeparam>
    /// <param name="parameters">Optional parameters passed to the component.</param>
    /// <param name="configure">Optional callback to customize services and options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task RunAsync<TComponent>(object? parameters = null, Action<ConsoleAppBuilder>? configure = null, CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        await using var app = Create<TComponent>(configure);
        await app.RunAsync(parameters, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Builds a service provider and options for running console components.
/// </summary>
public sealed class ConsoleAppBuilder
{
    private ConsoleAppBuilder()
    {
        Services = new ServiceCollection();
        Options = new ConsoleAppOptions();
        RegisterDefaults(Services);
    }

    /// <summary>
    /// Gets the service collection used to construct the application.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the configuration options for the console application.
    /// </summary>
    public ConsoleAppOptions Options { get; }

    internal static ConsoleAppBuilder Create() => new();

    /// <summary>
    /// Configures additional services for the console app.
    /// </summary>
    public ConsoleAppBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        configure(Services);
        return this;
    }

    /// <summary>
    /// Configures options for the console app.
    /// </summary>
    public ConsoleAppBuilder Configure(Action<ConsoleAppOptions> configure)
    {
        if (configure is not null)
        {
            configure(Options);
        }

        return this;
    }

    internal ServiceProvider BuildServiceProvider()
    {
        return Services.BuildServiceProvider();
    }

    private static void RegisterDefaults(IServiceCollection services)
    {
        services.TryAddSingleton<IComponentActivator, ComponentActivator>();
        services.TryAddSingleton<ConsoleNavigationManager>();
        services.TryAddSingleton<NavigationManager>(sp => sp.GetRequiredService<ConsoleNavigationManager>());
        services.TryAddSingleton<ILoggerFactory>(_ => NullLoggerFactory.Instance);
        services.TryAddSingleton<ConsoleRenderer>();
        services.TryAddSingleton<VdomDiffService>();
        services.TryAddSingleton<RendererKeyboardEventDispatcher>();
        services.TryAddSingleton<IKeyboardEventDispatcher>(sp => sp.GetRequiredService<RendererKeyboardEventDispatcher>());
        services.TryAddSingleton<IFocusEventDispatcher>(sp => sp.GetRequiredService<RendererKeyboardEventDispatcher>());
        services.TryAddSingleton<FocusManager>(sp => new FocusManager(sp.GetService<IFocusEventDispatcher>()));
        services.TryAddSingleton<KeyboardEventManager>();
        services.TryAddSingleton<ISyntaxLanguageRegistry, ColorCodeLanguageRegistry>();
        services.TryAddSingleton<ISyntaxThemeRegistry, SyntaxThemeRegistry>();
        services.TryAddSingleton<SpectreMarkupFormatter>();
        services.TryAddSingleton<SyntaxHighlightingService>();
    }
}

/// <summary>
/// Options that control how console applications render output.
/// </summary>
public sealed class ConsoleAppOptions
{
    /// <summary>
    /// Gets or sets whether the console should be cleared before writing output.
    /// </summary>
    public bool AutoClearConsole { get; set; } = true;

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

/// <summary>
/// Runs Razor components in a console context.
/// </summary>
/// <typeparam name="TComponent">Component type to render.</typeparam>
public sealed partial class ConsoleApp<TComponent> : IAsyncDisposable, IDisposable
    where TComponent : IComponent
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ConsoleAppOptions _options;
    private readonly VdomDiffService _diffService;
    private readonly ConsoleRenderer _consoleRenderer;
    private readonly SemaphoreSlim _renderLock = new(1, 1);
    private bool _disposed;

    internal ConsoleApp(ConsoleAppBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        _serviceProvider = builder.BuildServiceProvider();
        _options = builder.Options;
        _diffService = _serviceProvider.GetRequiredService<VdomDiffService>();
        _consoleRenderer = _serviceProvider.GetRequiredService<ConsoleRenderer>();
    }

    /// <summary>
    /// Gets the root service provider for advanced scenarios.
    /// </summary>
    public IServiceProvider Services
    {
        get
        {
            ThrowIfDisposed();
            return _serviceProvider;
        }
    }

    /// <summary>
    /// Renders the component and writes it to the console.
    /// This method blocks until the user cancels the operation (e.g., by pressing Ctrl+C).
    /// </summary>
    /// <param name="parameters">Optional component parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RunAsync(object? parameters = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        using var shutdown = CreateShutdownSource(cancellationToken, out var shutdownToken, out var registerCtrlC);
        ConsoleCancelEventHandler? cancelHandler = null;

        if (registerCtrlC)
        {
            cancelHandler = RegisterCtrlCHandler(shutdown);
        }

        try
        {
            var initialView = await RenderComponentInternalAsync(parameters, shutdownToken).ConfigureAwait(false);
            var currentParameters = parameters;
            var focusManager = _serviceProvider.GetRequiredService<FocusManager>();
            var keyboardManager = _serviceProvider.GetRequiredService<KeyboardEventManager>();
            var callback = _options.AfterRenderAsync ?? ConsoleAppOptions.DefaultAfterRenderAsync;

            if (_options.AutoClearConsole)
            {
                AnsiConsole.Clear();
            }

            using var liveContext = new ConsoleLiveDisplayContext(new LiveDisplayCanvas(), _consoleRenderer, null);
            using var _ = _consoleRenderer.Subscribe(focusManager);
            using var focusSession = focusManager.BeginSession(liveContext, initialView, shutdownToken);
            await focusSession.InitializationTask.ConfigureAwait(false);
            var keyListenerTask = keyboardManager.RunAsync(shutdownToken);

            await callback(liveContext, initialView, shutdownToken).ConfigureAwait(false);
            await WaitForExitAsync(shutdownToken).ConfigureAwait(false);

            try
            {
                await keyListenerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }

            return;
        }
        finally
        {
            if (cancelHandler is not null)
            {
                Console.CancelKeyPress -= cancelHandler;
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _serviceProvider.Dispose();
        _renderLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _serviceProvider.DisposeAsync().ConfigureAwait(false);
        _renderLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ConsoleApp<TComponent>));
        }
    }

    private static CancellationTokenSource CreateShutdownSource(CancellationToken cancellationToken, out CancellationToken shutdownToken, out bool registerCtrlC)
    {
        if (cancellationToken.CanBeCanceled)
        {
            var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            shutdownToken = linked.Token;
            registerCtrlC = false;
            return linked;
        }

        var source = new CancellationTokenSource();
        shutdownToken = source.Token;
        registerCtrlC = true;
        return source;
    }

    private static ConsoleCancelEventHandler? RegisterCtrlCHandler(CancellationTokenSource shutdown)
    {
        try
        {
            ConsoleCancelEventHandler handler = (_, args) =>
            {
                args.Cancel = true;
                if (!shutdown.IsCancellationRequested)
                {
                    shutdown.Cancel();
                }
            };

            Console.CancelKeyPress += handler;
            return handler;
        }
        catch
        {
            return null;
        }
    }

    private static async Task WaitForExitAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<ConsoleViewResult> RenderComponentInternalAsync(object? parameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _renderLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var parameterView = CreateParameterView(parameters);
            var snapshot = await _consoleRenderer.MountComponentAsync<TComponent>(parameterView, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return ConsoleViewResult.FromSnapshot(snapshot);
        }
        finally
        {
            _renderLock.Release();
        }
    }

    private static ParameterView CreateParameterView(object? parameters)
    {
        if (parameters is null)
        {
            return ParameterView.Empty;
        }

        if (parameters is ParameterView parameterView)
        {
            return parameterView;
        }

        if (parameters is IDictionary<string, object?> dictionary)
        {
            return ParameterView.FromDictionary(new Dictionary<string, object?>(dictionary));
        }

        if (parameters is IReadOnlyDictionary<string, object?> readOnlyDictionary)
        {
            return ParameterView.FromDictionary(readOnlyDictionary.ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        var props = parameters
            .GetType()
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Where(property => property.GetMethod is not null)
            .ToDictionary(property => property.Name, property => property.GetValue(parameters));

        return ParameterView.FromDictionary(props);
    }
}
