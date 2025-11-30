// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Core.Utilities;

namespace RazorConsole.Tests.Utilities;

public sealed class ComponentActivatorTests
{
    [Fact]
    public void CreateInstance_WithDependency_ResolvesConstructorInjection()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddTransient<IMyService, MyService>();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ComponentActivator activator = new(serviceProvider);

        // Act
        IComponent component = activator.CreateInstance(typeof(MyComponent));

        // Assert
        component.ShouldNotBeNull();
        var myComponent = component.ShouldBeOfType<MyComponent>();
        myComponent.Service.ShouldNotBeNull();
        myComponent.Service.ShouldBeOfType<MyService>();
        myComponent.GetMessage().ShouldBe(MyService.message);
    }

    [Fact]
    public void CreateInstance_WithKeyedDependencies_ResolvesConstructorInjection()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddKeyedTransient<IMyService, MyService>(nameof(MyKeyedComponent.FirstService));
        services.AddKeyedTransient<IMyService, MyService>(nameof(MyKeyedComponent.SecondService));
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ComponentActivator activator = new(serviceProvider);

        // Act
        IComponent component = activator.CreateInstance(typeof(MyKeyedComponent));

        // Assert
        component.ShouldNotBeNull();
        var myComponent = component.ShouldBeOfType<MyKeyedComponent>();
        myComponent.FirstService.ShouldNotBeNull();
        myComponent.FirstService.ShouldBeOfType<MyService>();
        myComponent.SecondService.ShouldNotBeNull();
        myComponent.SecondService.ShouldBeOfType<MyService>();
        myComponent.FirstGetMessage().ShouldBe(MyService.message);
        myComponent.SecondGetMessage().ShouldBe(MyService.message);
    }

    [Fact]
    public void CreateInstance_ForNonComponentType_ThrowsArgumentException()
    {
        // Arrange
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ComponentActivator activator = new(serviceProvider);

        // Act & Assert
        Should.Throw<ArgumentException>(() => activator.CreateInstance(typeof(string)));
    }

    [Fact]
    public void CreateInstance_WithMissingDependency_ThrowsInvalidOperationException()
    {
        // Arrange
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ComponentActivator activator = new(serviceProvider);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => activator.CreateInstance(typeof(MyComponent)));
    }

    private interface IMyService
    {
        string GetMessage();
    }

    private sealed class MyService : IMyService
    {
        public static string message = "Hello from MyService!";
        public string GetMessage() => message;
    }

    private sealed class MyComponent : IComponent
    {
        public IMyService Service { get; init; }

        public MyComponent(IMyService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Attach(RenderHandle renderHandle) => throw new NotImplementedException();
        public Task SetParametersAsync(ParameterView parameters) => throw new NotImplementedException();
        public string GetMessage() => Service.GetMessage();
    }

    private sealed class MyKeyedComponent : IComponent
    {
        public IMyService FirstService { get; init; }
        public IMyService SecondService { get; init; }

        public MyKeyedComponent([FromKeyedServices(nameof(FirstService))] IMyService firstService,
                                [FromKeyedServices(nameof(SecondService))] IMyService secondService)
        {
            FirstService = firstService ?? throw new ArgumentNullException(nameof(firstService));
            SecondService = secondService ?? throw new ArgumentNullException(nameof(secondService));
        }

        public void Attach(RenderHandle renderHandle) => throw new NotImplementedException();
        public Task SetParametersAsync(ParameterView parameters) => throw new NotImplementedException();
        public string FirstGetMessage() => FirstService.GetMessage();
        public string SecondGetMessage() => SecondService.GetMessage();
    }
}

