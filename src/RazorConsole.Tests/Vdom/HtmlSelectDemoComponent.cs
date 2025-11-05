using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RazorConsole.Tests.Vdom;

/// <summary>
/// Demo component to showcase HTML select tag support
/// </summary>
public class HtmlSelectDemoComponent : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Basic select with options
        builder.OpenElement(0, "select");
        builder.AddAttribute(1, "value", "blazor");

        builder.OpenElement(3, "option");
        builder.AddAttribute(4, "value", "react");
        builder.AddContent(5, "React");
        builder.CloseElement();

        builder.OpenElement(6, "option");
        builder.AddAttribute(7, "value", "vue");
        builder.AddContent(8, "Vue.js");
        builder.CloseElement();

        builder.OpenElement(9, "option");
        builder.AddAttribute(10, "value", "angular");
        builder.AddContent(11, "Angular");
        builder.CloseElement();

        builder.OpenElement(12, "option");
        builder.AddAttribute(13, "value", "blazor");
        builder.AddContent(14, "Blazor");
        builder.CloseElement();

        builder.CloseElement();
    }
}
