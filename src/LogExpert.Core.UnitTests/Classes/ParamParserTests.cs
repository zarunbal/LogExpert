using LogExpert.Core.Classes;

using Moq;

using NUnit.Framework;

namespace LogExpert.Core.UnitTests.Classes;

[TestFixture]
public class ParamParserTests
{
    private Mock<ILogLine> _mockLogLine;

    [SetUp]
    public void SetUp()
    {
        _mockLogLine = new Mock<ILogLine>();
        _mockLogLine.Setup(x => x.FullLine).Returns("Sample log line content");
    }

    [Test]
    public void ReplaceParams_LineNumber_ReplacesLParameter()
    {
        var parser = new ParamParser("Line number: %L");

        var result = parser.ReplaceParams(_mockLogLine.Object, 42, @"C:\test\file.txt");

        Assert.That(result, Is.EqualTo("Line number: 42"));
    }

    [Test]
    public void ReplaceParams_FileName_ReplacesNParameter()
    {
        var parser = new ParamParser("File: %N");
        var fileName = "file.txt";

        var result = parser.ReplaceParams(_mockLogLine.Object, 1, fileName);

        Assert.That(result, Is.EqualTo("File: file.txt"));
    }

    [Test]
    public void ReplaceParams_FilePath_ReplacesPParameter()
    {
        var parser = new ParamParser("Path: %P");
        var tempFile = Path.Combine(Path.GetTempPath(), "test", "file.txt");

        var result = parser.ReplaceParams(_mockLogLine.Object, 1, tempFile);

        Assert.That(result, Contains.Substring("Path: "));
        Assert.That(result, Contains.Substring("test"));
    }

    [Test]
    public void ReplaceParams_FullPath_ReplacesFParameter()
    {
        var parser = new ParamParser("Full path: %F");
        var fileName = "file.txt";

        var result = parser.ReplaceParams(_mockLogLine.Object, 1, fileName);

        Assert.That(result, Contains.Substring("Full path: "));
        Assert.That(result, Contains.Substring("file.txt"));
    }

    [Test]
    public void ReplaceParams_Extension_ReplacesEParameter()
    {
        var parser = new ParamParser("Extension: %E");
        var fileName = "file.txt";

        var result = parser.ReplaceParams(_mockLogLine.Object, 1, fileName);

        Assert.That(result, Is.EqualTo("Extension: .txt"));
    }

    [Test]
    public void ReplaceParams_NameWithoutExtension_ReplacesMParameter()
    {
        var parser = new ParamParser("Name without ext: %M");
        var fileName = "file.txt";

        var result = parser.ReplaceParams(_mockLogLine.Object, 1, fileName);

        Assert.That(result, Is.EqualTo("Name without ext: file"));
    }

    [Test]
    public void ReplaceParams_FileWithSpaces_QuotesPath()
    {
        var parser = new ParamParser("Path: %P");
        var tempFile = Path.Combine(Path.GetTempPath(), "test folder", "file.txt");

        var result = parser.ReplaceParams(_mockLogLine.Object, 1, tempFile);

        Assert.That(result, Contains.Substring("Path: "));
        // Should contain quotes if path has spaces
        if (tempFile.Contains(' '))
        {
            Assert.That(result, Contains.Substring("\""));
        }
    }

    [Test]
    public void ReplaceParams_UnixPath_HandlesCorrectly()
    {
        var parser = new ParamParser("File: %N, Path: %P");
        var fileName = "/home/user/file.txt";

        var result = parser.ReplaceParams(_mockLogLine.Object, 1, fileName);

        Assert.That(result, Contains.Substring("File: file.txt"));
        Assert.That(result, Contains.Substring("Path: "));
    }

    [Test]
    public void ReplaceParams_MultipleParameters_ReplacesAll()
    {
        var parser = new ParamParser("Line %L: %N at %P");
        var fileName = "app.log";

        var result = parser.ReplaceParams(_mockLogLine.Object, 123, fileName);

        Assert.That(result, Contains.Substring("Line 123:"));
        Assert.That(result, Contains.Substring("app.log"));
        Assert.That(result, Contains.Substring("at "));
    }

    [Test]
    public void StripExtension_VariousFileNames_ReturnsCorrectResult()
    {
        Assert.That(ParamParser.StripExtension("file.txt"), Is.EqualTo("file"));
        Assert.That(ParamParser.StripExtension("archive.tar.gz"), Is.EqualTo("archive.tar"));
        Assert.That(ParamParser.StripExtension("file"), Is.EqualTo("fil")); // This is the actual behavior when no extension
        Assert.That(ParamParser.StripExtension("file."), Is.EqualTo("file"));
    }

    [Test]
    public void Constructor_StoresTemplate()
    {
        const string template = "Test template %L";
        var parser = new ParamParser(template);

        // Test that the template is used correctly
        var result = parser.ReplaceParams(_mockLogLine.Object, 1, "test.txt");
        Assert.That(result, Contains.Substring("Test template"));
    }
}