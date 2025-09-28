using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Core.Controllers;
using RazorConsole.Core.Rendering.ComponentMarkup;
using RazorConsole.Core.Rendering.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering;

public interface IRazorComponentRenderer
{
    Task<ConsoleViewResult> RenderAsync<TComponent>(object? parameters = null, CancellationToken cancellationToken = default)
        where TComponent : IComponent;
}

public class RazorComponentRenderer : IRazorComponentRenderer
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RazorComponentRenderer(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

    public async Task<ConsoleViewResult> RenderAsync<TComponent>(object? parameters = null, CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var scope = _scopeFactory.CreateAsyncScope();
        var renderer = scope.ServiceProvider.GetRequiredService<HtmlRenderer>();
        var parameterView = CreateParameterView(parameters);

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rootComponent = await renderer.RenderComponentAsync<TComponent>(parameterView).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return rootComponent.ToHtmlString();
        }).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return CreateViewResult(typeof(TComponent), html);
    }

    private static ConsoleViewResult CreateViewResult(Type componentType, string html)
    {
        HtmlVdomConverter.TryConvert(html, out var vdomRoot);

        if (!SpectreRenderableFactory.TryCreateRenderable(vdomRoot, out var renderable, out var animatedRenderables) || renderable is null)
        {
            throw new InvalidOperationException($"Unable to create a Spectre.Console renderable for '{componentType.FullName}'.");
        }

        return ConsoleViewResult.Create(html, vdomRoot, renderable, animatedRenderables);
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
            return ParameterView.FromDictionary(new Dictionary<string, object?>(readOnlyDictionary));
        }

        var props = parameters
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetMethod is not null)
            .ToDictionary(property => property.Name, property => property.GetValue(parameters));

        return ParameterView.FromDictionary(props);
    }
}
