// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Vdom;
using Spectre.Console.Rendering;

namespace RazorConsole.Core.Rendering.Vdom;

/// <summary>
/// Provides context for translating VNodes, allowing recursive translation of child nodes.
/// </summary>
public sealed class TranslationContext
{
    private readonly VdomSpectreTranslator _translator;

    internal TranslationContext(VdomSpectreTranslator translator)
    {
        _translator = translator;
    }

    /// <summary>
    /// Attempts to translate a VNode to an IRenderable.
    /// </summary>
    /// <param name="node">The VNode to translate.</param>
    /// <param name="renderable">The resulting renderable if successful.</param>
    /// <returns>True if translation was successful; otherwise, false.</returns>
    public bool TryTranslate(VNode node, out IRenderable? renderable)
        => _translator.TryTranslateInternal(node, this, out renderable);
}
