// Copyright (c) RazorConsole. All rights reserved.

using System.Diagnostics.CodeAnalysis;

using RazorConsole.Core.Vdom;

namespace RazorConsole.Core.Extensions;

public static class VNodeExtensions
{
    /// <summary>
    /// Attempts to get an attribute value from a VNode and convert it to the specified type.
    /// </summary>
    /// <typeparam name="TValue">The type to convert the attribute value to.</typeparam>
    /// <param name="node">The VNode to get the attribute from.</param>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="value">When this method returns, contains the converted value if the attribute exists and can be converted; otherwise, the default value for the type.</param>
    /// <returns><c>true</c> if the attribute exists and was successfully converted; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <para>
    /// For component nodes (VNodeKind.Component), this method searches in the <c>Attrs</c> dictionary which contains <c>object?</c> values.
    /// For element nodes (VNodeKind.Element), this method searches in the <c>Attributes</c> dictionary which contains <c>string?</c> values.
    /// </para>
    /// <para>
    /// Type conversion is performed using pattern matching. If the value is already of type <typeparamref name="TValue"/>, it is returned directly.
    /// For element nodes, if <typeparamref name="TValue"/> is <c>string</c> or <c>string?</c>, the string value is returned directly.
    /// </para>
    /// </remarks>
    public static bool TryGetAttributeValue<TValue>(
        this VNode node,
        string key,
        [MaybeNullWhen(false)]
        out TValue? value)
    {
        value = default;
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (!node.Attrs.TryGetValue(key, out var objValue))
        {
            return false;
        }

        if (objValue is null)
        {
            var type = typeof(TValue);
            if (type.IsValueType && Nullable.GetUnderlyingType(type) is null)
            {
                return false;
            }

            return true;
        }

        if (objValue is TValue typedValue)
        {
            value = typedValue;
            return true;
        }

        // Try to cast (for compatible types)
        try
        {
            value = (TValue)objValue;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets an attribute value from a VNode and converts it to the specified type, or returns a fallback value if the attribute is not found or cannot be converted.
    /// </summary>
    /// <typeparam name="TValue">The type to convert the attribute value to.</typeparam>
    /// <param name="node">The VNode to get the attribute from.</param>
    /// <param name="key">The name of the attribute.</param>
    /// <param name="fallback">The value to return if the attribute is not found or cannot be converted.</param>
    /// <returns>The converted attribute value if found and successfully converted; otherwise, the fallback value.</returns>
    [return: NotNullIfNotNull(nameof(fallback))]
    public static TValue? GetAttributeValue<TValue>(
        this VNode node,
        string key,
        TValue? fallback = default)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (TryGetAttributeValue<TValue>(node, key, out var value))
        {
            return value;
        }

        return fallback;
    }
}
