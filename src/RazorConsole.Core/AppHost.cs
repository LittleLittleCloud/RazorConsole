using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.Focus;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

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
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        configure(Options);
        return this;
    }

    internal ConsoleAppOptions BuildOptions() => Options.Clone();

    internal ServiceProvider BuildServiceProvider()
    {
        return Services.BuildServiceProvider();
    }

    private static void RegisterDefaults(IServiceCollection services)
    {
        services.TryAddSingleton<IComponentActivator, ServiceProviderComponentActivator>();
        services.TryAddSingleton<ConsoleNavigationManager>();
        services.TryAddSingleton<NavigationManager>(sp => sp.GetRequiredService<ConsoleNavigationManager>());
        services.TryAddSingleton<ILoggerFactory>(_ => NullLoggerFactory.Instance);
        services.TryAddScoped(sp => new HtmlRenderer(sp, sp.GetRequiredService<ILoggerFactory>()));
        services.TryAddSingleton<RazorComponentRenderer>();
        services.TryAddSingleton<IRazorComponentRenderer>(sp => sp.GetRequiredService<RazorComponentRenderer>());
        services.TryAddSingleton<VdomDiffService>();
        services.TryAddSingleton<FocusManager>();
        services.TryAddSingleton<LiveDisplayContextAccessor>();
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

    /// <summary>
    /// Callback invoked after a component has been rendered.
    /// </summary>
    public Func<ConsoleLiveDisplayContext, ConsoleViewResult, CancellationToken, Task>? AfterRenderAsync { get; set; } = DefaultAfterRenderAsync;

    internal ConsoleAppOptions Clone() => new()
    {
        AutoClearConsole = AutoClearConsole,
        AfterRenderAsync = AfterRenderAsync,
    };

    internal static Task DefaultAfterRenderAsync(ConsoleLiveDisplayContext context, ConsoleViewResult view, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// Runs Razor components in a console context.
/// </summary>
/// <typeparam name="TComponent">Component type to render.</typeparam>
public sealed class ConsoleApp<TComponent> : IAsyncDisposable, IDisposable
    where TComponent : IComponent
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ConsoleAppOptions _options;
    private readonly VdomDiffService _diffService;
    private readonly LiveDisplayContextAccessor? _liveContextAccessor;
    private readonly IRazorComponentRenderer _renderer;
    private bool _disposed;

    internal ConsoleApp(ConsoleAppBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        _serviceProvider = builder.BuildServiceProvider();
        _options = builder.BuildOptions();
        _diffService = _serviceProvider.GetRequiredService<VdomDiffService>();
        _liveContextAccessor = _serviceProvider.GetService<LiveDisplayContextAccessor>();
        _renderer = _serviceProvider.GetRequiredService<IRazorComponentRenderer>();
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
            var view = await _renderer.RenderAsync<TComponent>(parameters, shutdownToken).ConfigureAwait(false);
            var currentParameters = parameters;
            var focusManager = _serviceProvider.GetService<FocusManager>();

            var callback = _options.AfterRenderAsync ?? ConsoleAppOptions.DefaultAfterRenderAsync;

            if (SupportsLiveDisplay())
            {
                var liveDisplay = AnsiConsole.Live(view.Renderable);
                liveDisplay.AutoClear = true;
                await liveDisplay.StartAsync(async liveContext =>
                {
                    shutdownToken.ThrowIfCancellationRequested();

                    using var context = ConsoleLiveDisplayContext.Create<TComponent>(liveContext, view, _diffService, _renderer, currentParameters);
                    _liveContextAccessor?.Attach(context);
                    FocusManager.FocusSession? session = null;
                    Task? keyListener = null;

                    try
                    {
                        if (focusManager is not null)
                        {
                            session = focusManager.BeginSession(context, view, shutdownToken);
                            await session.InitializationTask.ConfigureAwait(false);

                            if (!Console.IsInputRedirected)
                            {
                                keyListener = ListenForFocusKeysAsync(focusManager, session.Token);
                            }
                        }

                        await callback(context, view, shutdownToken).ConfigureAwait(false);
                        await WaitForExitAsync(shutdownToken).ConfigureAwait(false);

                        if (keyListener is not null)
                        {
                            try
                            {
                                await keyListener.ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                            }
                        }
                    }
                    finally
                    {
                        session?.Dispose();
                        _liveContextAccessor?.Detach(context);
                    }
                }).ConfigureAwait(false);

                return;
            }

            if (_options.AutoClearConsole)
            {
                AnsiConsole.Clear();
            }

            using var fallbackContext = ConsoleLiveDisplayContext.CreateForTesting<TComponent>(new FallbackLiveDisplayCanvas(), view, _diffService, _renderer, currentParameters);
            _liveContextAccessor?.Attach(fallbackContext);
            fallbackContext.UpdateRenderable(view.Renderable);

            FocusManager.FocusSession? fallbackSession = null;
            Task? fallbackKeyListener = null;

            try
            {
                if (focusManager is not null)
                {
                    fallbackSession = focusManager.BeginSession(fallbackContext, view, shutdownToken);
                    await fallbackSession.InitializationTask.ConfigureAwait(false);

                    if (!Console.IsInputRedirected)
                    {
                        fallbackKeyListener = ListenForFocusKeysAsync(focusManager, fallbackSession.Token);
                    }
                }

                await callback(fallbackContext, view, shutdownToken).ConfigureAwait(false);
                await WaitForExitAsync(shutdownToken).ConfigureAwait(false);

                if (fallbackKeyListener is not null)
                {
                    try
                    {
                        await fallbackKeyListener.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
            finally
            {
                fallbackSession?.Dispose();
                _liveContextAccessor?.Detach(fallbackContext);
            }
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

    private static bool SupportsLiveDisplay()
    {
        try
        {
            return !Console.IsOutputRedirected && !Console.IsErrorRedirected;
        }
        catch
        {
            return false;
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

    private static async Task ListenForFocusKeysAsync(FocusManager focusManager, CancellationToken token)
    {
        if (focusManager is null)
        {
            throw new ArgumentNullException(nameof(focusManager));
        }

        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!Console.KeyAvailable)
                {
                    await Task.Delay(50, token).ConfigureAwait(false);
                    continue;
                }

                var keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key != ConsoleKey.Tab)
                {
                    continue;
                }

                if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
                {
                    await focusManager.FocusPreviousAsync(token).ConfigureAwait(false);
                }
                else
                {
                    await focusManager.FocusNextAsync(token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (InvalidOperationException)
            {
                // Console input is not available (e.g., redirected). Stop listening.
                break;
            }
            catch (IOException)
            {
                // Console input is not available. Stop listening.
                break;
            }
            catch
            {
                // Minor glitches retrieving input should not terminate the loop immediately.
                try
                {
                    await Task.Delay(200, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
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

    private sealed class FallbackLiveDisplayCanvas : ConsoleLiveDisplayContext.ILiveDisplayCanvas
    {
        private IRenderable? _current;

        public void UpdateTarget(IRenderable? renderable)
        {
            _current = renderable;

            if (renderable is not null)
            {
                AnsiConsole.Write(renderable);
            }
        }

        public void Refresh()
        {
            if (_current is not null)
            {
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
}
