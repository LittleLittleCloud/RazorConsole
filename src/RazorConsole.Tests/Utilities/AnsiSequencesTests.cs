using RazorConsole.Core.Utilities;
using Xunit;

namespace RazorConsole.Tests.Utilities;

public sealed class AnsiSequencesTests
{
    [Fact]
    public void IDN_ReturnsIndexEscapeSequence()
    {
        Assert.Equal("\u001bD", AnsiSequences.IDN());
    }

    [Fact]
    public void NEL_ReturnsNextLineEscapeSequence()
    {
        Assert.Equal("\u001bE", AnsiSequences.NEL());
    }

    [Fact]
    public void RI_ReturnsReverseIndexEscapeSequence()
    {
        Assert.Equal("\u001bM", AnsiSequences.RI());
    }
}
