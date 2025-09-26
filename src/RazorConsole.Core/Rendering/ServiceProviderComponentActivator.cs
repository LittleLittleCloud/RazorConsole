using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace RazorConsole.Core.Rendering;

public sealed class ServiceProviderComponentActivator : IComponentActivator
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderComponentActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IComponent CreateInstance(Type componentType)
    {
        return (IComponent)ActivatorUtilities.CreateInstance(_serviceProvider, componentType);
    }
}
