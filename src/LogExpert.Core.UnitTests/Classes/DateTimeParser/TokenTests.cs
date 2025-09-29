using LogExpert.Core.Classes.DateTimeParser;

using NUnit.Framework;

using System;

namespace LogExpert.Core.UnitTests.Classes.DateTimeParser;

[TestFixture]
public class TokenTests
{
    [TestCase("y", true)]
    [TestCase("yyyy", true)]
    [TestCase("Y", true)]
    [TestCase("YYYY", true)]
    [TestCase("m", true)]
    [TestCase("MM", true)]
    [TestCase("M", true)]
    [TestCase("mm", true)]
    [TestCase("d", true)]
    [TestCase("dd", true)]
    [TestCase("D", true)]
    [TestCase("DD", true)]
    [TestCase("s", true)]
    [TestCase("ss", true)]
    [TestCase("S", true)]
    [TestCase("SS", true)]
    [TestCase("h", true)]
    [TestCase("hh", true)]
    [TestCase("H", true)]
    [TestCase("HH", true)]
    [TestCase("tt", true)]
    [TestCase("TT", true)]
    public void IsDatePart_ValidDateTokens_ReturnsTrue(string token, bool expected)
    {
        var result = Token.IsDatePart(token);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("x", false)]
    [TestCase("z", false)]
    [TestCase("xyz", false)]
    [TestCase("abc", false)]
    [TestCase("123", false)]
    [TestCase("-", false)]
    [TestCase(":", false)]
    [TestCase(" ", false)]
    [TestCase("", false)]
    [TestCase("ttt", false)]
    [TestCase("t", false)]
    public void IsDatePart_InvalidTokens_ReturnsFalse(string token, bool expected)
    {
        var result = Token.IsDatePart(token);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void IsDatePart_NullToken_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Token.IsDatePart(null));
    }

    [TestCase("Y")]
    [TestCase("M")]
    [TestCase("D")]
    [TestCase("S")]
    [TestCase("H")]
    [TestCase("TT")]
    public void IsDatePart_CaseInsensitive_ReturnsTrue(string token)
    {
        var result = Token.IsDatePart(token);

        Assert.That(result, Is.True);
    }

    [TestCase("yy")]
    [TestCase("yyyy")]
    [TestCase("yMd")]
    [TestCase("hmmss")]
    public void IsDatePart_ComplexDateTokens_ReturnsTrue(string token)
    {
        var result = Token.IsDatePart(token);

        Assert.That(result, Is.True);
    }
}