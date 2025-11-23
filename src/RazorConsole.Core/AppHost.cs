// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using RazorConsole.Core.Controllers;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Input;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.Markdown;
using RazorConsole.Core.Rendering.Syntax;
using RazorConsole.Core.Utilities;
using RazorConsole.Core.Vdom;

using Spectre.Console;

namespace RazorConsole.Core;

/// <summary>
/// Extension methods for wiring Razor Console into generic host builders.
/// </summary>
public static class HostBuilderExtension
{
    /// <summary>
    /// Adds Razor Console services to the specified <see cref="IHostBuilder"/> using the provided root component.
    /// </summary>
    /// <typeparam name="TComponent">The Razor component that acts as the application's root component.</typeparam>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="configure">An optional callback to perform additional configuration.</param>
    /// <returns>The configured <see cref="IHostBuilder"/> instance.</returns>
    public static IHostBuilder UseRazorConsole<TComponent>(
        this IHostBuilder hostBuilder,
        Action<IHostBuilder>? configure = null)
        where TComponent : IComponent
    {
        hostBuilder.ConfigureServices(RegisterDefaults<TComponent>);
        configure?.Invoke(hostBuilder);

        return hostBuilder;
    }

    /// <summary>
    /// Adds Razor Console services to the specified <see cref="IHostApplicationBuilder"/> using the provided root component.
    /// </summary>
    /// <typeparam name="TComponent">The Razor component that acts as the application's root component.</typeparam>
    /// <param name="hostBuilder">The host application builder to configure.</param>
    /// <param name="configure">An optional callback to perform additional configuration.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/> instance.</returns>
    public static IHostApplicationBuilder UseRazorConsole<TComponent>(
        this IHostApplicationBuilder hostBuilder,
        Action<IHostApplicationBuilder>? configure = null)
        where TComponent : IComponent
    {
        RuntimeEncoding.EnsureUtf8();
        RegisterDefaults<TComponent>(hostBuilder.Services);

        configure?.Invoke(hostBuilder);

        return hostBuilder;
    }

    private static void RegisterDefaults<TComponent>(IServiceCollection services) where TComponent : IComponent
    {
        services.TryAddSingleton<IComponentActivator, ComponentActivator>();
        services.TryAddSingleton<ConsoleNavigationManager>();
        services.TryAddSingleton<NavigationManager>(sp => sp.GetRequiredService<ConsoleNavigationManager>());
        services.AddSingleton<INavigationInterception, NoopNavigationInterception>();
        services.AddSingleton<IScrollToLocationHash, NoopScrollToLocationHash>();
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
        services.TryAddSingleton<MarkdownRenderingService>();
        services.AddDefaultVdomTranslators();
        // Register HtmlCodeBlockElementTranslator with dependency injection
        services.AddSingleton<Rendering.Vdom.IVdomElementTranslator>(sp =>
            new Rendering.Vdom.HtmlCodeBlockElementTranslator(sp.GetRequiredService<SyntaxHighlightingService>()));
        services.TryAddSingleton(sp =>
        {
            var translators = sp.GetServices<Rendering.Vdom.IVdomElementTranslator>()
                .OrderBy(t => t.Priority)
                .ToList();
            return new Rendering.Vdom.VdomSpectreTranslator(translators);
        });

        // Add ConsoleAppOptions as a singleton by resolving the IOptions value in a factory to avoid IOptions dependency in injecting components.
        services.AddSingleton<ConsoleAppOptions>(resolver => resolver.GetRequiredService<IOptions<ConsoleAppOptions>>().Value);
        services.AddHostedService<ComponentService<TComponent>>();

        // clear all log providers because it would interfere with console rendering
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
        });
    }
}

internal class ComponentService<TComponent>(
    ConsoleAppOptions options,
    ConsoleRenderer consoleRenderer,
    FocusManager focusManager,
    KeyboardEventManager keyboardEventManager) : BackgroundService where TComponent : IComponent
{
    private readonly SemaphoreSlim _renderLock = new(1, 1);

    /// <summary>
    /// Ensures exceptions that occur during component execution are surfaced when the host stops.
    /// </summary>
    /// <param name="cancellationToken">A token that requests the stop operation to cancel.</param>
    /// <returns>A task that completes when background processing has stopped.</returns>
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
        var initialView = await RenderComponentInternalAsync(null, token).ConfigureAwait(false);

        var callback = options.AfterRenderAsync ?? ConsoleAppOptions.DefaultAfterRenderAsync;

        if (options.AutoClearConsole)
        {
            AnsiConsole.Clear();
        }

        using var liveContext = new ConsoleLiveDisplayContext(new LiveDisplayCanvas(AnsiConsole.Console), consoleRenderer, null);
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
