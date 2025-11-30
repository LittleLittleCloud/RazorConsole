// Copyright (c) RazorConsole. All rights reserved.

using System.Reflection;
using Spectre.Console;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal static class ComponentMarkupUtilities
{
    public static string CreateStyledMarkup(string? style, string content, bool requiresEscape)
    {
        if (requiresEscape)
        {
            content = Markup.Escape(content);
        }

        if (string.IsNullOrWhiteSpace(style))
        {
            return content;
        }

        return string.Concat("[", style, "]", content, "[/]");
    }

    public static Spinner ResolveSpinner(string? spinnerType)
    {
        if (!string.IsNullOrWhiteSpace(spinnerType))
        {
            var property = typeof(Spinner.Known)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(p => string.Equals(p.Name, spinnerType, StringComparison.OrdinalIgnoreCase));

            if (property?.GetValue(null) is Spinner spinner)
            {
                return spinner;
            }
        }

        return Spinner.Known.Dots;
    }
}
