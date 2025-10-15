using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace RazorConsole.Core.Utilities;

internal sealed class ComponentActivator : IComponentActivator
{
    private readonly IServiceProvider _serviceProvider;

    public ComponentActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IComponent CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type componentType)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentType))
        {
            throw new ArgumentException($"The type {componentType.FullName} does not implement {nameof(IComponent)}.", nameof(componentType));
        }

        return (IComponent)ActivatorUtilities.CreateInstance(_serviceProvider, componentType)!;
    }
}
