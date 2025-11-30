// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

/// <summary>
/// Represents a translator that can convert a VNode to a Spectre.Console IRenderable.
/// </summary>
public interface IVdomElementTranslator
{
    /// <summary>
    /// Gets the priority of this translator. Lower values are processed first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Attempts to translate a VNode to an IRenderable.
    /// </summary>
    /// <param name="node">The VNode to translate.</param>
    /// <param name="context">The translation context for recursive translation.</param>
    /// <param name="renderable">The resulting renderable if successful.</param>
    /// <returns>True if translation was successful; otherwise, false.</returns>
    bool TryTranslate(VNode node, TranslationContext context, out IRenderable? renderable);
}
