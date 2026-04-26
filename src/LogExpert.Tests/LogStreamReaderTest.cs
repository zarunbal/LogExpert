using System.Text;

using LogExpert.Core.Classes.Log.Streamreaders;
using LogExpert.Core.Entities;

using NUnit.Framework;

namespace LogExpert.Tests;

[TestFixture]
public class LogStreamReaderTest
{
    [Test]
    [TestCase("Line 1\nLine 2\nLine 3", 3)]
    [TestCase("Line 1\nLine 2\nLine 3\n", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3\r\n", 3)]
    [TestCase("Line 1\rLine 2\rLine 3", 3)]
    [TestCase("Line 1\rLine 2\rLine 3\r", 3)]
    public void ReadLinesWithSystemNewLine (string text, int expectedLines)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
        using var reader = new PositionAwareStreamReaderSystem(stream, new EncodingOptions(), 500);
        var lineCount = 0;
        while (true)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            lineCount += 1;

            Assert.That(line.StartsWith($"Line {lineCount}", StringComparison.OrdinalIgnoreCase), $"Invalid line: {line}");
        }

        Assert.That(expectedLines, Is.EqualTo(lineCount), $"Unexpected lines:\n{text}");
    }

    [Test]
    [TestCase("\n\n\n", 3)]
    [TestCase("\r\n\r\n\r\n", 3)]
    [TestCase("\r\r\r", 3)]
    public void CountLinesWithSystemNewLine (string text, int expectedLines)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
        using var reader = new PositionAwareStreamReaderSystem(stream, new EncodingOptions(), 500);
        var lineCount = 0;
        while (reader.ReadLine() != null)
        {
            lineCount += 1;
        }

        Assert.That(expectedLines, Is.EqualTo(lineCount), $"Unexpected lines:\n{text}");
    }

    [Test]
    [TestCase("Line 1\nLine 2\nLine 3", 3)]
    [TestCase("Line 1\nLine 2\nLine 3\n", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3\r\n", 3)]
    [TestCase("Line 1\rLine 2\rLine 3", 3)]
    [TestCase("Line 1\rLine 2\rLine 3\r", 3)]
    public void ReadLinesWithLegacyNewLine (string text, int expectedLines)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
        using var reader = new PositionAwareStreamReaderLegacy(stream, new EncodingOptions(), 500);
        var lineCount = 0;
        while (true)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            lineCount += 1;

            Assert.That(line.StartsWith($"Line {lineCount}", StringComparison.OrdinalIgnoreCase), $"Invalid line: {line}");
        }

        Assert.That(expectedLines, Is.EqualTo(lineCount), $"Unexpected lines:\n{text}");
    }
    [Test]
    [TestCase("\n\n\n", 3)]
    [TestCase("\r\n\r\n\r\n", 3)]
    [TestCase("\r\r\r", 3)]
    public void CountLinesWithLegacyNewLine (string text, int expectedLines)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
        using var reader = new PositionAwareStreamReaderLegacy(stream, new EncodingOptions(), 500);
        var lineCount = 0;
        while (reader.ReadLine() != null)
        {
            lineCount += 1;
        }

        Assert.That(expectedLines, Is.EqualTo(lineCount), $"Unexpected lines:\n{text}");
    }

    [Test]
    [TestCase("Line 1\nLine 2\nLine 3", 3)]
    [TestCase("Line 1\nLine 2\nLine 3\n", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3\r\n", 3)]
    [TestCase("Line 1\rLine 2\rLine 3", 3)]
    [TestCase("Line 1\rLine 2\rLine 3\r", 3)]
    public void ReadLinesWithPipelineNewLine (string text, int expectedLines)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
        using var reader = new PositionAwareStreamReaderPipeline(stream, new EncodingOptions(), 500);
        var lineCount = 0;
        while (true)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            lineCount += 1;

            Assert.That(line.StartsWith($"Line {lineCount}", StringComparison.OrdinalIgnoreCase), $"Invalid line: {line}");
        }

        Assert.That(expectedLines, Is.EqualTo(lineCount), $"Unexpected lines:\n{text}");
    }

    [Test]
    [TestCase("\n\n\n", 3)]
    [TestCase("\r\n\r\n\r\n", 3)]
    [TestCase("\r\r\r", 3)]
    public void CountLinesWithPipelineNewLine (string text, int expectedLines)
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(text));
        using var reader = new PositionAwareStreamReaderPipeline(stream, new EncodingOptions(), 500);
        var lineCount = 0;
        while (reader.ReadLine() != null)
        {
            lineCount += 1;
        }

        Assert.That(expectedLines, Is.EqualTo(lineCount), $"Unexpected lines:\n{text}");
    }

    [Test]
    public void PipelineReaderShouldTrackPositionCorrectly ()
    {
        var text = "Line 1\nLine 2\nLine 3\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        using var reader = new PositionAwareStreamReaderPipeline(stream, new EncodingOptions(), 500);

        var line1 = reader.ReadLine();
        var pos1 = reader.Position;
        Assert.That(line1, Is.EqualTo("Line 1"));
        Assert.That(pos1, Is.EqualTo(7)); // "Line 1\n" = 7 bytes

        var line2 = reader.ReadLine();
        var pos2 = reader.Position;
        Assert.That(line2, Is.EqualTo("Line 2"));
        Assert.That(pos2, Is.EqualTo(14)); // 7 + "Line 2\n" = 14 bytes

        var line3 = reader.ReadLine();
        var pos3 = reader.Position;
        Assert.That(line3, Is.EqualTo("Line 3"));
        Assert.That(pos3, Is.EqualTo(21)); // 14 + "Line 3\n" = 21 bytes
    }

    [Test]
    public void PipelineReaderShouldSupportSeeking ()
    {
        var text = "Line 1\nLine 2\nLine 3\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        using var reader = new PositionAwareStreamReaderPipeline(stream, new EncodingOptions(), 500);

        // Read first line
        var line1 = reader.ReadLine();
        Assert.That(line1, Is.EqualTo("Line 1"));

        // Seek back to beginning
        reader.Position = 0;

        // Should read first line again
        var line1Again = reader.ReadLine();
        Assert.That(line1Again, Is.EqualTo("Line 1"));

        // Seek to middle
        reader.Position = 7; // After "Line 1\n"

        // Should read second line
        var line2 = reader.ReadLine();
        Assert.That(line2, Is.EqualTo("Line 2"));
    }

    [Test]
    public void PipelineReaderShouldHandleMaximumLineLength ()
    {
        var longLine = new string('X', 1000);
        var text = $"{longLine}\nShort line\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        using var reader = new PositionAwareStreamReaderPipeline(stream, new EncodingOptions(), 100);

        // First line should be truncated to 100 chars
        var line1 = reader.ReadLine();
        Assert.That(line1, Has.Length.EqualTo(100));
        Assert.That(line1, Is.EqualTo(new string('X', 100)));

        // Second line should be normal
        var line2 = reader.ReadLine();
        Assert.That(line2, Is.EqualTo("Short line"));
    }

    [Test]
    public void PipelineReaderShouldHandleUnicode ()
    {
        var text = "Hello 世界\nСпасибо\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        using var reader = new PositionAwareStreamReaderPipeline(stream, new EncodingOptions { Encoding = Encoding.UTF8 }, 500);

        var line1 = reader.ReadLine();
        Assert.That(line1, Is.EqualTo("Hello 世界"));

        var line2 = reader.ReadLine();
        Assert.That(line2, Is.EqualTo("Спасибо"));
    }

    [Test]
    public void PipelineReaderShouldHandleEmptyLines ()
    {
        var text = "Line 1\n\nLine 3\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        using var reader = new PositionAwareStreamReaderPipeline(stream, new EncodingOptions(), 500);

        var line1 = reader.ReadLine();
        Assert.That(line1, Is.EqualTo("Line 1"));

        var line2 = reader.ReadLine();
        Assert.That(line2, Is.EqualTo(""));

        var line3 = reader.ReadLine();
        Assert.That(line3, Is.EqualTo("Line 3"));

        var eof = reader.ReadLine();
        Assert.That(eof, Is.Null);
    }
}
