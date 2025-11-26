using ColumnizerLib;
using ColumnizerLib.Extensions;

using NUnit.Framework;

namespace LogExpert.ColumnizerLib.Tests.Extensions;

[TestFixture]

internal class LogLineExtensionsTests
{
    private class TestingLogLine : ILogLine
    {
        public string FullLine { get; set; }

        public int LineNumber { get; set; }

        public string Text { get; set; }
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Test")]
    public void ToClipBoardText_ReturnsExpected ()
    {
        var underTest = new TestingLogLine
        {
            FullLine = "a fullLine",
            LineNumber = 89,
            Text = "a text"
        };
        Assert.That(underTest.ToClipBoardText(), Is.EqualTo("\t90\ta fullLine"));
    }
}
