// Copyright (c) RazorConsole. All rights reserved.

namespace RazorConsole.Core.Abstarctions.Components;

/// <summary>
/// Marks a component as a synthetic component that is identified and translated by its component type rather than HTML attributes.
/// </summary>
/// <remarks>
/// <para>
/// Synthetic components are RazorConsole built-in components that are identified by their component type
/// rather than by HTML element attributes (such as <c>class</c> or <c>data-*</c>).
/// </para>
/// <para>
/// Synthetic components are translated directly into Spectre.Console renderables with a 1-to-1 mapping. Unlike regular components
/// that render HTML elements with attributes (e.g., <c>&lt;div class="panel"&gt;</c>), synthetic components are typically headless
/// and are processed directly by element translators.
/// </para>
/// </remarks>
public interface ISyntheticComponent;

