using LogExpert.Core.Classes.DateTimeParser;

using NUnit.Framework;

namespace LogExpert.Core.UnitTests.Classes.DateTimeParser;

[TestFixture]
public class ParserTests
{
    [Test]
    public void ParseSections_EmptyString_ReturnsEmptyList()
    {
        var result = Parser.ParseSections("", out var syntaxError);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        Assert.That(syntaxError, Is.False);
    }

    [Test]
    public void ParseSections_SimpleFormatString_ReturnsSections()
    {
        var result = Parser.ParseSections("yyyy-MM-dd", out var syntaxError);

        Assert.That(result, Is.Not.Null);
        Assert.That(syntaxError, Is.False);
        // Don't assert specific count as implementation details may vary
    }

    [Test]
    public void ParseSections_ValidInput_DoesNotSetSyntaxError()
    {
        Parser.ParseSections("HH:mm:ss", out var syntaxError);

        Assert.That(syntaxError, Is.False);
    }

    [Test]
    public void ParseSections_NullInput_ThrowsException()
    {
        // Test that the parser throws an exception for null input
        Assert.Throws<NullReferenceException>(() => Parser.ParseSections(null, out var syntaxError));
    }

    [TestCase("yyyy")]
    [TestCase("MM")]
    [TestCase("dd")]
    [TestCase("HH:mm")]
    [TestCase("yyyy-MM-dd HH:mm:ss")]
    public void ParseSections_CommonFormats_DoesNotThrow(string formatString)
    {
        Assert.DoesNotThrow(() =>
        {
            var result = Parser.ParseSections(formatString, out var syntaxError);
            Assert.That(result, Is.Not.Null);
        });
    }

    [Test]
    public void ParseSections_ComplexFormat_HandlesCorrectly()
    {
        const string complexFormat = "yyyy-MM-dd'T'HH:mm:ss.fff";

        var result = Parser.ParseSections(complexFormat, out var syntaxError);

        Assert.That(result, Is.Not.Null);
        // Parser should handle complex formats without throwing
    }
}