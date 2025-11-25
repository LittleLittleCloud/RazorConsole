// Copyright (c) RazorConsole. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RazorConsole.Core;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Focus;
using RazorConsole.Core.Input;
using RazorConsole.Core.Rendering;
using RazorConsole.Core.Rendering.Markdown;
using RazorConsole.Core.Rendering.Syntax;
using RazorConsole.Core.Rendering.Vdom;
using RazorConsole.Core.Utilities;
using RazorConsole.Core.Vdom;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace RazorConsole.Website;

internal interface IRazorConsoleRenderer
{
    Task HandleKeyboardEventAsync(string xtermKey, string domKey, bool ctrlKey, bool altKey, bool shiftKey);
    event Action<string>? SnapshotRendered;
}

internal class RazorConsoleRenderer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent> : IObserver<ConsoleRenderer.RenderSnapshot>, IRazorConsoleRenderer
    where TComponent : IComponent
{
    private readonly string _componentId;
    private IServiceProvider? _serviceProvider;
    private ConsoleRenderer? _consoleRenderer;
    private IAnsiConsole? _ansiConsole;
    private readonly StringWriter _sw = new StringWriter();
    private KeyboardEventManager? _keyboardEventManager;
    private Task? _initializationTask;
    public event Action<string>? SnapshotRendered;

    public RazorConsoleRenderer(string componentId)
    {
        _componentId = componentId;
        _initializationTask = InitializeAsync();
    }

    private Task EnsureInitializedAsync()
    {
        if (_initializationTask is null)
        {
            _initializationTask = InitializeAsync();
        }
        return _initializationTask;
    }

    /// <summary>
    /// Initializes the rendering pipeline so components can be materialized outside the console host.
    /// </summary>
    private async Task InitializeAsync()
    {
        if (_serviceProvider is not null)
        {
            throw new InvalidOperationException("The renderer has already been initialized.");
        }

        var services = new ServiceCollection();
        services.Configure<ConsoleAppOptions>(_ => { });

        services.TryAddSingleton<IComponentActivator>(sp => new ComponentActivator(sp));
        services.TryAddSingleton<ConsoleNavigationManager>();
        services.TryAddSingleton<NavigationManager>(sp => sp.GetRequiredService<ConsoleNavigationManager>());
        services.AddSingleton<INavigationInterception, NoopNavigationInterception>();
        services.AddSingleton<IScrollToLocationHash, NoopScrollToLocationHash>();

        services.TryAddSingleton<ILoggerFactory>(_ => NullLoggerFactory.Instance);
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
        services.AddSingleton<IVdomElementTranslator>(sp =>
            new HtmlCodeBlockElementTranslator(sp.GetRequiredService<SyntaxHighlightingService>()));
        services.TryAddSingleton(sp =>
        {
            var translators = sp.GetServices<IVdomElementTranslator>()
                .OrderBy(t => t.Priority)
                .ToList();
            return new VdomSpectreTranslator(translators);
        });

        services.TryAddSingleton<ConsoleRenderer>();
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ConsoleAppOptions>>().Value);

        _serviceProvider = services.BuildServiceProvider();
        _consoleRenderer = _serviceProvider.GetRequiredService<ConsoleRenderer>();
        _keyboardEventManager = _serviceProvider.GetRequiredService<KeyboardEventManager>();
        var focusManager = _serviceProvider.GetRequiredService<FocusManager>();

        _ansiConsole = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.Standard,
            Out = new AnsiConsoleOutput(_sw)
        });

        _ansiConsole.Profile.Capabilities.Unicode = true;

        _ansiConsole.Profile.Width = 80;
        _ansiConsole.Profile.Height = 150;
        var snapshot = await _consoleRenderer.MountComponentAsync<TComponent>(ParameterView.Empty, default).ConfigureAwait(false);
        _consoleRenderer.Subscribe(this);
        _consoleRenderer.Subscribe(focusManager);

        var initialView = ConsoleViewResult.FromSnapshot(snapshot);
        var canvas = new LiveDisplayCanvas(_ansiConsole);
        var consoleLiveDisplayContext = new ConsoleLiveDisplayContext(canvas, _consoleRenderer, initialView);
        var focusSession = focusManager.BeginSession(consoleLiveDisplayContext, initialView, CancellationToken.None);
        await focusSession.InitializationTask.ConfigureAwait(false);
        canvas.Refreshed += () =>
        {
            var output = _sw.ToString();
            SnapshotRendered?.Invoke(output);
            XTermInterop.WriteToTerminal(_componentId, output);
            _sw.GetStringBuilder().Clear();
        };
    }

    /// <summary>
    /// Render Razor component to ANSI string that can be sent to xterm.js.
    /// </summary>
    /// <returns></returns>
    public async Task<string> RenderAsync()
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        if (_consoleRenderer is null)
        {
            throw new InvalidOperationException("The renderer has not been initialized.");
        }
        var sw = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.Standard,
            Out = new AnsiConsoleOutput(sw)
        });

        console.Profile.Width = 80;
        console.Profile.Height = 150;

        var consoleOption = new RenderOptions(console.Profile.Capabilities, new Size(80, 150));

        var snapshot = await _consoleRenderer.MountComponentAsync<TComponent>(ParameterView.Empty, CancellationToken.None);

        try
        {
            console.Write(snapshot.Renderable!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering component: {ex.Message} {ex.StackTrace}");

            throw;
        }

        return sw.ToString();
    }

    /// <summary>
    /// Processes a keyboard event from the browser.
    /// </summary>
    public async Task HandleKeyboardEventAsync(string xtermKey, string domKey, bool ctrlKey, bool altKey, bool shiftKey)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        if (_keyboardEventManager is null)
        {
            return;
        }

        var keyInfo = ParseKeyFromBrowser(xtermKey, domKey, ctrlKey, altKey, shiftKey);
        Console.WriteLine($"Parsed ConsoleKeyInfo: KeyChar='{keyInfo.KeyChar}', Key={keyInfo.Key}, Modifiers={keyInfo.Modifiers}");
        await _keyboardEventManager.HandleKeyAsync(keyInfo, CancellationToken.None).ConfigureAwait(false);
    }

    private static ConsoleKeyInfo ParseKeyFromBrowser(string xtermKey, string domKey, bool ctrlKey, bool altKey, bool shiftKey)
    {
        var modifiers = ConsoleModifiers.None;
        if (ctrlKey)
        {
            modifiers |= ConsoleModifiers.Control;
        }
        if (altKey)
        {
            modifiers |= ConsoleModifiers.Alt;
        }
        if (shiftKey)
        {
            modifiers |= ConsoleModifiers.Shift;
        }

        // Use domKey for key identification (cleaner names like "Enter", "Tab", etc.)
        var consoleKey = domKey switch
        {
            "Enter" => ConsoleKey.Enter,
            "Tab" => ConsoleKey.Tab,
            "Backspace" => ConsoleKey.Backspace,
            "Escape" => ConsoleKey.Escape,
            "ArrowUp" => ConsoleKey.UpArrow,
            "ArrowDown" => ConsoleKey.DownArrow,
            "ArrowLeft" => ConsoleKey.LeftArrow,
            "ArrowRight" => ConsoleKey.RightArrow,
            _ when domKey.Length == 1 => ParseSingleChar(domKey[0]),
            _ => ConsoleKey.None
        };

        // Use xtermKey for the actual character (better for printable characters)
        // For special keys, xtermKey contains escape sequences, so use domKey mapping
        var keyChar = domKey switch
        {
            "Enter" => '\r',
            "Tab" => '\t',
            "Backspace" => '\b',
            _ when xtermKey.Length == 1 => xtermKey[0],
            _ => '\0'
        };

        return new ConsoleKeyInfo(keyChar, consoleKey, shiftKey, altKey, ctrlKey);
    }

    private static ConsoleKey ParseSingleChar(char c)
    {
        return c switch
        {
            >= 'a' and <= 'z' => ConsoleKey.A + (c - 'a'),
            >= 'A' and <= 'Z' => ConsoleKey.A + (c - 'A'),
            >= '0' and <= '9' => ConsoleKey.D0 + (c - '0'),
            ' ' => ConsoleKey.Spacebar,
            _ => ConsoleKey.None
        };
    }

    public void OnCompleted()
    {
        return;
    }
    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(ConsoleRenderer.RenderSnapshot value)
    {
        try
        {
            if (value.Renderable is null)
            {
                return;
            }

            var output = _sw.ToString();
            SnapshotRendered?.Invoke(output);
            XTermInterop.WriteToTerminal(_componentId, output);
            _sw.GetStringBuilder().Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering component: {ex.Message} {ex.StackTrace}");
            throw;
        }
    }
}
