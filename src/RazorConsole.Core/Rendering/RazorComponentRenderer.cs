using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;

namespace RazorConsole.Core.Rendering;

public class RazorComponentRenderer
{
    private readonly HtmlRenderer _renderer;

    public RazorComponentRenderer(IServiceProvider serviceProvider)
    {
        _renderer = serviceProvider.GetRequiredService<HtmlRenderer>();
    }

    public Task<string> RenderAsync<TComponent>(object? parameters = null)
        where TComponent : IComponent
    {
        var parameterView = CreateParameterView(parameters);

        return _renderer.Dispatcher.InvokeAsync(async () =>
        {
            var rootComponent = await _renderer.RenderComponentAsync<TComponent>(parameterView);
            return rootComponent.ToHtmlString();
        });
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
