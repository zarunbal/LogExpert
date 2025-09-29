using LogExpert.Core.Classes;

using NUnit.Framework;

namespace LogExpert.Core.UnitTests.Classes;

[TestFixture]
public class UtilTests
{
    [TestCase(@"C:\path\to\file.txt", "file.txt")]
    [TestCase(@"C:\path\to\file", "file")]
    [TestCase(@"/path/to/file.txt", "file.txt")]
    [TestCase(@"/path/to/file", "file")]
    [TestCase("file.txt", "file.txt")]
    [TestCase("file", "file")]
    public void GetNameFromPath_ValidPaths_ReturnsFileName(string path, string expected)
    {
        var result = Util.GetNameFromPath(path);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("file.txt", "file")]
    [TestCase("archive.tar.gz", "archive.tar")]
    [TestCase("file", "fil")] // Current behavior: removes last char when no extension
    [TestCase("file.", "file")]
    public void StripExtension_ValidFileNames_ReturnsNameWithoutExtension(string fileName, string expected)
    {
        var result = Util.StripExtension(fileName);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("file.txt", "txt")]
    [TestCase("archive.tar.gz", "gz")]
    [TestCase("file", "")]
    [TestCase("file.", "")]
    public void GetExtension_ValidFileNames_ReturnsExtension(string fileName, string expected)
    {
        var result = Util.GetExtension(fileName);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(512, "512 bytes")]
    [TestCase(1024, "1 KB")]
    [TestCase(1536, "1 KB")]
    [TestCase(1048576, "1.00 MB")]
    [TestCase(1572864, "1.50 MB")]
    [TestCase(0, "0 bytes")]
    public void GetFileSizeAsText_VariousSizes_ReturnsFormattedString(long size, string expected)
    {
        var result = Util.GetFileSizeAsText(size);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("text", false)]
    [TestCase(" ", false)]
    public void IsNull_VariousStrings_ReturnsCorrectResult(string input, bool expected)
    {
        var result = Util.IsNull(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("   ", true)]
    [TestCase("text", false)]
    [TestCase(" text ", false)]
    public void IsNullOrSpaces_VariousStrings_ReturnsCorrectResult(string input, bool expected)
    {
        var result = Util.IsNullOrSpaces(input);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("hello", "hello", 0)]
    [TestCase("hello", "world", 4)]
    [TestCase("", "", 0)]
    [TestCase("hello", "", 5)]
    [TestCase("", "world", 5)]
    [TestCase("kitten", "sitting", 3)]
    public void DamerauLevenshteinDistance_VariousStrings_ReturnsCorrectDistance(string source, string destination, int expected)
    {
        var result = Util.DamerauLevenshteinDistance(source, destination);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void AssertTrue_TrueCondition_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => Util.AssertTrue(true, "Should not throw"));
    }

    [Test]
    public void AssertTrue_FalseCondition_ThrowsException()
    {
        var exception = Assert.Throws<Exception>(() => Util.AssertTrue(false, "Test message"));
        Assert.That(exception.Message, Is.EqualTo("Test message"));
    }
}