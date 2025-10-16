using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RazorConsole.Core.Rendering.Vdom;

namespace RazorConsole.Core.Vdom;

/// <summary>
/// Extension methods for registering VDOM element translators.
/// </summary>
public static class VdomTranslatorServiceCollectionExtensions
{
    /// <summary>
    /// Adds a custom VDOM element translator to the service collection.
    /// </summary>
    /// <typeparam name="TTranslator">The translator type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="priority">The priority of the translator. Lower values are tried first. Default is 1000.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVdomTranslator<TTranslator>(
        this IServiceCollection services,
        int priority = 1000)
        where TTranslator : class, VdomSpectreTranslator.IVdomElementTranslator
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<VdomSpectreTranslator.IVdomElementTranslator>(sp =>
            new PrioritizedTranslator(ActivatorUtilities.CreateInstance<TTranslator>(sp), priority));

        return services;
    }

    /// <summary>
    /// Adds a custom VDOM element translator instance to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="translator">The translator instance to register.</param>
    /// <param name="priority">The priority of the translator. Lower values are tried first. Default is 1000.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVdomTranslator(
        this IServiceCollection services,
        VdomSpectreTranslator.IVdomElementTranslator translator,
        int priority = 1000)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (translator is null)
        {
            throw new ArgumentNullException(nameof(translator));
        }

        services.AddSingleton<VdomSpectreTranslator.IVdomElementTranslator>(
            new PrioritizedTranslator(translator, priority));

        return services;
    }

    /// <summary>
    /// Adds a custom VDOM element translator factory to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="factory">A factory function to create the translator.</param>
    /// <param name="priority">The priority of the translator. Lower values are tried first. Default is 1000.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVdomTranslator(
        this IServiceCollection services,
        Func<IServiceProvider, VdomSpectreTranslator.IVdomElementTranslator> factory,
        int priority = 1000)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        services.AddSingleton<VdomSpectreTranslator.IVdomElementTranslator>(sp =>
            new PrioritizedTranslator(factory(sp), priority));

        return services;
    }

    internal static void AddDefaultVdomTranslators(this IServiceCollection services)
    {
        // Register translators in priority order (lower priority numbers are tried first)
        services.AddVdomTranslator(new VdomSpectreTranslator.TextElementTranslator(), priority: 10);
        services.AddVdomTranslator(new VdomSpectreTranslator.HtmlInlineTextElementTranslator(), priority: 20);
        services.AddVdomTranslator(new VdomSpectreTranslator.ParagraphElementTranslator(), priority: 30);
        services.AddVdomTranslator(new VdomSpectreTranslator.SpacerElementTranslator(), priority: 40);
        services.AddVdomTranslator(new VdomSpectreTranslator.NewlineElementTranslator(), priority: 50);
        services.AddVdomTranslator(new VdomSpectreTranslator.SpinnerElementTranslator(), priority: 60);
        services.AddVdomTranslator(new VdomSpectreTranslator.ButtonElementTranslator(), priority: 70);
        services.AddVdomTranslator(new VdomSpectreTranslator.HtmlButtonElementTranslator(), priority: 80);
        services.AddVdomTranslator(new VdomSpectreTranslator.SyntaxHighlighterElementTranslator(), priority: 90);
        services.AddVdomTranslator(new VdomSpectreTranslator.PanelElementTranslator(), priority: 100);
        services.AddVdomTranslator(new VdomSpectreTranslator.RowsElementTranslator(), priority: 110);
        services.AddVdomTranslator(new VdomSpectreTranslator.ColumnsElementTranslator(), priority: 120);
        services.AddVdomTranslator(new VdomSpectreTranslator.GridElementTranslator(), priority: 130);
        services.AddVdomTranslator(new VdomSpectreTranslator.PadderElementTranslator(), priority: 140);
        services.AddVdomTranslator(new VdomSpectreTranslator.AlignElementTranslator(), priority: 150);
        services.AddVdomTranslator(new VdomSpectreTranslator.FigletElementTranslator(), priority: 160);
        services.AddVdomTranslator(new VdomSpectreTranslator.TableElementTranslator(), priority: 170);
        services.AddVdomTranslator(new VdomSpectreTranslator.HtmlListElementTranslator(), priority: 180);
        services.AddVdomTranslator(new VdomSpectreTranslator.HtmlDivElementTranslator(), priority: 190);
        services.AddVdomTranslator(new VdomSpectreTranslator.FailToRenderElementTranslator(), priority: 1000); // Last resort
    }

    /// <summary>
    /// Wrapper class that associates a translator with a priority.
    /// </summary>
    private sealed class PrioritizedTranslator : VdomSpectreTranslator.IVdomElementTranslator, IPrioritizedTranslator
    {
        private readonly VdomSpectreTranslator.IVdomElementTranslator _translator;

        public PrioritizedTranslator(VdomSpectreTranslator.IVdomElementTranslator translator, int priority)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            Priority = priority;
        }

        public int Priority { get; }

        public bool TryTranslate(VNode node, VdomSpectreTranslator.TranslationContext context, out Spectre.Console.Rendering.IRenderable? renderable)
            => _translator.TryTranslate(node, context, out renderable);
    }

    /// <summary>
    /// Internal interface for prioritized translators.
    /// </summary>
    private interface IPrioritizedTranslator
    {
        int Priority { get; }
    }
}
