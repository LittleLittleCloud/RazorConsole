// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.AspNetCore.Razor.Language;

namespace RazorConsole.Tests;

/// <summary>
/// Tests for dynamic Razor compilation functionality.
/// These tests verify the Razor parsing and code generation capabilities.
/// </summary>
public class DynamicCompilerTests
{
    [Fact]
    public void RazorEngine_Should_Parse_Simple_Component()
    {
        // Arrange
        var razorCode = """
            @using Spectre.Console
            <Markup Content="Hello World" />
            """;

        // Create a Razor engine similar to DynamicComponentCompiler
        var fileSystem = RazorProjectFileSystem.Create(".");
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            fileSystem,
            builder =>
            {
                builder.SetNamespace("DynamicComponents");
                builder.SetBaseType("Microsoft.AspNetCore.Components.ComponentBase");
            });

        // Act
        var document = RazorSourceDocument.Create(razorCode, "TestComponent.razor");
        var codeDocument = engine.Process(
            document,
            fileKind: null,
            importSources: Array.Empty<RazorSourceDocument>(),
            tagHelpers: Array.Empty<TagHelperDescriptor>());

        var csharpDocument = codeDocument.GetCSharpDocument();

        // Assert
        Assert.NotNull(csharpDocument);
        Assert.NotNull(csharpDocument.GeneratedCode);
        Assert.Contains("class", csharpDocument.GeneratedCode);
        Assert.DoesNotContain(csharpDocument.Diagnostics, d => d.Severity == RazorDiagnosticSeverity.Error);
    }

    [Fact]
    public void RazorEngine_Should_Generate_CSharp_Code()
    {
        // Arrange
        var razorCode = """
            @code {
                private int count = 0;
                private void Increment() => count++;
            }
            <Panel Title="Test">
                <Markup Content="@count.ToString()" />
            </Panel>
            """;

        var fileSystem = RazorProjectFileSystem.Create(".");
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            fileSystem,
            builder =>
            {
                builder.SetNamespace("DynamicComponents");
                builder.SetBaseType("Microsoft.AspNetCore.Components.ComponentBase");
            });

        // Act
        var document = RazorSourceDocument.Create(razorCode, "CounterComponent.razor");
        var codeDocument = engine.Process(
            document,
            fileKind: null,
            importSources: Array.Empty<RazorSourceDocument>(),
            tagHelpers: Array.Empty<TagHelperDescriptor>());

        var csharpDocument = codeDocument.GetCSharpDocument();

        // Assert
        Assert.NotNull(csharpDocument.GeneratedCode);
        Assert.Contains("count", csharpDocument.GeneratedCode);
        Assert.Contains("Increment", csharpDocument.GeneratedCode);
    }

    [Fact]
    public void RazorEngine_Should_Handle_Component_With_Parameters()
    {
        // Arrange
        var razorCode = """
            @code {
                [Parameter]
                public string Title { get; set; } = "Default";
            }
            <Markup Content="@Title" />
            """;

        var fileSystem = RazorProjectFileSystem.Create(".");
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            fileSystem,
            builder =>
            {
                builder.SetNamespace("DynamicComponents");
                builder.SetBaseType("Microsoft.AspNetCore.Components.ComponentBase");
            });

        // Act
        var document = RazorSourceDocument.Create(razorCode, "ParameterComponent.razor");
        var codeDocument = engine.Process(
            document,
            fileKind: null,
            importSources: Array.Empty<RazorSourceDocument>(),
            tagHelpers: Array.Empty<TagHelperDescriptor>());

        var csharpDocument = codeDocument.GetCSharpDocument();

        // Assert
        Assert.NotNull(csharpDocument.GeneratedCode);
        Assert.Contains("Parameter", csharpDocument.GeneratedCode);
        Assert.Contains("Title", csharpDocument.GeneratedCode);
    }

    [Fact]
    public void RazorEngine_Should_Process_Without_Errors_For_Valid_Code()
    {
        // Arrange - Valid Razor code with C# block
        var razorCode = """
            @{
                var x = 42;
                var message = $"The answer is {x}";
            }
            <Markup Content="@message" />
            """;

        var fileSystem = RazorProjectFileSystem.Create(".");
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            fileSystem,
            builder =>
            {
                builder.SetNamespace("DynamicComponents");
                builder.SetBaseType("Microsoft.AspNetCore.Components.ComponentBase");
            });

        // Act
        var document = RazorSourceDocument.Create(razorCode, "ValidComponent.razor");
        var codeDocument = engine.Process(
            document,
            fileKind: null,
            importSources: Array.Empty<RazorSourceDocument>(),
            tagHelpers: Array.Empty<TagHelperDescriptor>());

        var csharpDocument = codeDocument.GetCSharpDocument();

        // Assert - Should generate valid C# code without errors
        Assert.NotNull(csharpDocument.GeneratedCode);
        Assert.Contains("message", csharpDocument.GeneratedCode);
        Assert.DoesNotContain(csharpDocument.Diagnostics, d => d.Severity == RazorDiagnosticSeverity.Error);
    }

    [Fact]
    public void RazorEngine_Should_Support_Using_Directives()
    {
        // Arrange
        var razorCode = """
            @using System.Linq
            @using Spectre.Console
            @code {
                private string[] items = new[] { "one", "two", "three" };
                private string joined => string.Join(", ", items);
            }
            <Markup Content="@joined" />
            """;

        var fileSystem = RazorProjectFileSystem.Create(".");
        var engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            fileSystem,
            builder =>
            {
                builder.SetNamespace("DynamicComponents");
                builder.SetBaseType("Microsoft.AspNetCore.Components.ComponentBase");
            });

        // Act
        var document = RazorSourceDocument.Create(razorCode, "UsingComponent.razor");
        var codeDocument = engine.Process(
            document,
            fileKind: null,
            importSources: Array.Empty<RazorSourceDocument>(),
            tagHelpers: Array.Empty<TagHelperDescriptor>());

        var csharpDocument = codeDocument.GetCSharpDocument();

        // Assert
        Assert.NotNull(csharpDocument.GeneratedCode);
        Assert.Contains("using System.Linq", csharpDocument.GeneratedCode);
        Assert.Contains("using Spectre.Console", csharpDocument.GeneratedCode);
    }
}
