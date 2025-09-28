using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using RazorConsole.Core.Rendering.ComponentMarkup;

namespace RazorConsole.Components;

public abstract class RenderableConverterComponentBase<TConverter> : ConsoleComponentBase
    where TConverter : class, IRenderableConverter, new()
{
    private const string ConverterAttributeName = "data-converter";

    private readonly TConverter _defaultConverter = new();

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IRenderableConverter? Converter { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?>? AdditionalAttributes { get; set; }

    protected IRenderableConverter ConverterInstance => Converter ?? _defaultConverter;

    protected string? ConverterTypeAttribute
    {
        get
        {
            var type = ConverterInstance.GetType();
            return type.AssemblyQualifiedName ?? type.FullName;
        }
    }

    protected override void OnParametersSet()
    {
        Converter ??= _defaultConverter;
        EnsureConverterAttributeNotProvided();
        EnsureConverterAttributeProvided();
        _resolvedAttributes = BuildResolvedAttributes();
        base.OnParametersSet();
    }

    protected IReadOnlyDictionary<string, object?> RemainingAttributes => _resolvedAttributes ??= BuildResolvedAttributes();

    private IReadOnlyDictionary<string, object?>? _resolvedAttributes;

    private void EnsureConverterAttributeNotProvided()
    {
        if (AdditionalAttributes is not null && AdditionalAttributes.Keys.Any(attribute =>
                string.Equals(attribute, ConverterAttributeName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"The '{ConverterAttributeName}' attribute is reserved and cannot be set explicitly.");
        }
    }

    private void EnsureConverterAttributeProvided()
    {
        if (string.IsNullOrWhiteSpace(ConverterTypeAttribute))
        {
            throw new InvalidOperationException("The 'data-converter' attribute must be set by components deriving from RenderableConverterComponentBase.");
        }
    }

    private IReadOnlyDictionary<string, object?> BuildResolvedAttributes()
    {
        var converterType = ConverterTypeAttribute ?? throw new InvalidOperationException("Converter type could not be resolved.");

        if (AdditionalAttributes is null || AdditionalAttributes.Count == 0)
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                [ConverterAttributeName] = converterType,
            };
        }

        var attributes = new Dictionary<string, object?>(AdditionalAttributes, StringComparer.OrdinalIgnoreCase)
        {
            [ConverterAttributeName] = converterType,
        };

        return attributes;
    }
}
