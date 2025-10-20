using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
    /// Creates a <see cref="IHost"/> instance for the specified component type.
    /// </summary>
    /// <typeparam name="TComponent">Component type to render.</typeparam>
    /// <param name="configure">Optional callback to customize services and options.</param>
    public static IHost Create<TComponent>(Action<HostApplicationBuilder>? configure = null)
        where TComponent : IComponent
    {
        var builder = ConsoleAppBuilder.Create<TComponent>();
        configure?.Invoke(builder);
        return builder.Build();
    }

    /// <summary>
    /// Renders the specified component type to the console using default settings.
    /// </summary>
    /// <typeparam name="TComponent">Component type to render.</typeparam>
    /// <param name="parameters">Optional parameters passed to the component.</param>
    /// <param name="configure">Optional callback to customize services and options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task RunAsync<TComponent>(object? parameters = null, Action<HostApplicationBuilder>? configure = null, CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        var app = Create<TComponent>(builder =>
        {
            configure?.Invoke(builder);
            builder.AddParameters(parameters);
        });
        await app.RunAsync(cancellationToken);
    }
}

/// <summary>
/// Builds a service provider and options for running console components.
/// </summary>
public sealed class ConsoleAppBuilder
{
    internal static HostApplicationBuilder Create<TComponent>() where TComponent : IComponent
    {
        var b = Host.CreateApplicationBuilder();
        RegisterDefaults<TComponent>(b.Services);

        b.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            //plumb in alternate
        });
        return b;
    }

    private static void RegisterDefaults<TComponent>(IServiceCollection services) where TComponent : IComponent
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
        services.AddDefaultVdomTranslators();
        services.TryAddSingleton(sp =>
        {
            var translators = sp.GetServices<Rendering.Vdom.IVdomElementTranslator>()
                .OrderBy(t => t.Priority)
                .ToList();
            return new Rendering.Vdom.VdomSpectreTranslator(translators);
        });

        services.AddSingleton<ConsoleAppOptions>();
        services.AddSingleton<ParamContainer>();
        services.AddHostedService<ComponentService<TComponent>>();
    }
}

public static class ConsoleAppBuilderExtensions
{
    public static void AddParameters(this HostApplicationBuilder builder, object? p)
    {
        var container = new ParamContainer
        {
            Parameters = p
        };

        builder.Services.AddSingleton(container);
    }
}

public class ParamContainer
{
    public object? Parameters { get; set; }
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

internal class ComponentService<TComponent>(
    ConsoleAppOptions options,
    ConsoleRenderer consoleRenderer,
    FocusManager focusManager,
    KeyboardEventManager keyboardEventManager,
    ParamContainer? paramContainer) : BackgroundService where TComponent : IComponent
{
    private readonly SemaphoreSlim _renderLock = new(1, 1);

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        // Bubble exceptions up into Host.StopAsync, invoked when Host self-stops when a BackgroundService throws
        if (ExecuteTask?.Exception is not null)
        {
            var flattened = ExecuteTask.Exception.Flatten();
            if (flattened.InnerException is not null)
            {
                throw flattened.InnerException;
            }
            else
            {
                throw flattened;
            }
        }

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var initialView = await RenderComponentInternalAsync(paramContainer?.Parameters, token).ConfigureAwait(false);

        var callback = options.AfterRenderAsync ?? ConsoleAppOptions.DefaultAfterRenderAsync;

        if (options.AutoClearConsole)
        {
            AnsiConsole.Clear();
        }

        using var liveContext = new ConsoleLiveDisplayContext(new LiveDisplayCanvas(), consoleRenderer, null);
        using var _ = consoleRenderer.Subscribe(focusManager);
        using var focusSession = focusManager.BeginSession(liveContext, initialView, token);
        await focusSession.InitializationTask.ConfigureAwait(false);
        var keyListenerTask = keyboardEventManager.RunAsync(token);

        await callback(liveContext, initialView, token).ConfigureAwait(false);

        await Task.Delay(Timeout.InfiniteTimeSpan, token).ConfigureAwait(false);
    }

    private async Task<ConsoleViewResult> RenderComponentInternalAsync(object? parameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _renderLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var parameterView = CreateParameterView(parameters);
            var snapshot = await consoleRenderer.MountComponentAsync<TComponent>(parameterView, cancellationToken).ConfigureAwait(false);

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
