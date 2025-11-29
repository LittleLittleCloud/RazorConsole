// Copyright (c) RazorConsole. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RazorConsole.Core.Rendering;

namespace RazorConsole.Tests.Rendering;

public sealed class ConsoleRendererOptionsTests
{
    [Fact]
    public void Constructor_WithoutOptions_UsesDefaultOptions()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var translator = TestHelpers.CreateTestTranslator();

        using var renderer = new ConsoleRenderer(services, NullLoggerFactory.Instance, translator);

        renderer.ShouldNotBeNull();
    }
}

