// Copyright (c) RazorConsole. All rights reserved.

using RazorConsole.Core.Utilities;

namespace RazorConsole.Tests.Utilities;

public sealed class AnsiSequencesTests
{
    [Fact]
    public void IDN_ReturnsIndexEscapeSequence()
    {
        AnsiSequences.IDN().ShouldBe("\u001bD");
    }

    [Fact]
    public void NEL_ReturnsNextLineEscapeSequence()
    {
        AnsiSequences.NEL().ShouldBe("\u001bE");
    }

    [Fact]
    public void RI_ReturnsReverseIndexEscapeSequence()
    {
        AnsiSequences.RI().ShouldBe("\u001bM");
    }
}
