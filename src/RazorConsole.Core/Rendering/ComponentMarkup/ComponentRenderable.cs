using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal readonly record struct ComponentRenderable(string Markup, IRenderable Renderable);
