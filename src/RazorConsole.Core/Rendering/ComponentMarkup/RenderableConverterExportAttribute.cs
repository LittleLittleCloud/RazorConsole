using System;
using System.Composition;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class RenderableConverterExportAttribute : ExportAttribute
{
    public RenderableConverterExportAttribute(Type converterType)
        : base(typeof(IRenderableConverter))
    {
        ConverterType = converterType.AssemblyQualifiedName ?? converterType.FullName ?? converterType.Name ?? null;
    }

    public string? ConverterType { get; }
}
