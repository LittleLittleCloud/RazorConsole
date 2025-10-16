using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVdomTranslator<TTranslator>(
        this IServiceCollection services)
        where TTranslator : class, IVdomElementTranslator
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<IVdomElementTranslator, TTranslator>();

        return services;
    }

    /// <summary>
    /// Adds a custom VDOM element translator instance to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="translator">The translator instance to register.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVdomTranslator(
        this IServiceCollection services,
        IVdomElementTranslator translator)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (translator is null)
        {
            throw new ArgumentNullException(nameof(translator));
        }

        services.AddSingleton<IVdomElementTranslator>(translator);

        return services;
    }

    /// <summary>
    /// Adds a custom VDOM element translator factory to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="factory">A factory function to create the translator.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVdomTranslator(
        this IServiceCollection services,
        Func<IServiceProvider, IVdomElementTranslator> factory)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        services.AddSingleton<IVdomElementTranslator>(factory);

        return services;
    }

    internal static void AddDefaultVdomTranslators(this IServiceCollection services)
    {
        // Register default translators - they will be ordered by their Priority property
        var defaultTranslators = VdomSpectreTranslator.CreateDefaultTranslators();
        foreach (var translator in defaultTranslators)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IVdomElementTranslator>(translator));
        }
    }
}
