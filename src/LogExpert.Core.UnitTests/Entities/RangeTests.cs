using LogExpert.Core.Entities;

using NUnit.Framework;

using Range = LogExpert.Core.Entities.Range;

namespace LogExpert.Core.UnitTests.Entities;

[TestFixture]
public class RangeTests
{
    [Test]
    public void Constructor_Default_InitializesWithZeroValues()
    {
        var range = new Range();

        Assert.That(range.StartLine, Is.EqualTo(0));
        Assert.That(range.EndLine, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithParameters_InitializesCorrectly()
    {
        const int startLine = 10;
        const int endLine = 20;

        var range = new Range(startLine, endLine);

        Assert.That(range.StartLine, Is.EqualTo(startLine));
        Assert.That(range.EndLine, Is.EqualTo(endLine));
    }

    [Test]
    public void Properties_SetAndGet_WorkCorrectly()
    {
        var range = new Range();
        const int startLine = 5;
        const int endLine = 15;

        range.StartLine = startLine;
        range.EndLine = endLine;

        Assert.That(range.StartLine, Is.EqualTo(startLine));
        Assert.That(range.EndLine, Is.EqualTo(endLine));
    }

    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(1, 10)]
    [TestCase(-1, 5)]
    [TestCase(100, 50)] // End before start - should be allowed for flexibility
    public void Constructor_VariousValues_StoresCorrectly(int startLine, int endLine)
    {
        var range = new Range(startLine, endLine);

        Assert.That(range.StartLine, Is.EqualTo(startLine));
        Assert.That(range.EndLine, Is.EqualTo(endLine));
    }
}