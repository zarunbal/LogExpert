using System.Text;

using LogExpert.Core.Classes.Log.Streamreaders;
using LogExpert.Core.Entities;

using NUnit.Framework;

namespace LogExpert.Tests.StreamReaderTests;

[TestFixture]
public class PositionAwareStreamReaderDirectTests
{
    private static MemoryStream CreateStream (string text, Encoding? encoding = null)
    {
        return new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(text));
    }

    #region Basic line reading

    [Test]
    [TestCase("Line 1\nLine 2\nLine 3", 3)]
    [TestCase("Line 1\nLine 2\nLine 3\n", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3", 3)]
    [TestCase("Line 1\r\nLine 2\r\nLine 3\r\n", 3)]
    public void TryReadLine_CorrectLineCount (string text, int expectedLines)
    {
        using var stream = CreateStream(text);
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 500);
        var count = 0;
        while (reader.TryReadLine(out _))
        {
            count++;
        }

        Assert.That(count, Is.EqualTo(expectedLines));
    }

    [Test]
    [TestCase("Line 1\nLine 2\nLine 3")]
    [TestCase("Line 1\r\nLine 2\r\nLine 3")]
    public void TryReadLine_CorrectContent (string text)
    {
        using var stream = CreateStream(text);
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 500);

        Assert.That(reader.TryReadLine(out var line1), Is.True);
        Assert.That(line1.Span.ToString(), Is.EqualTo("Line 1"));

        Assert.That(reader.TryReadLine(out var line2), Is.True);
        Assert.That(line2.Span.ToString(), Is.EqualTo("Line 2"));

        Assert.That(reader.TryReadLine(out var line3), Is.True);
        Assert.That(line3.Span.ToString(), Is.EqualTo("Line 3"));

        Assert.That(reader.TryReadLine(out _), Is.False);
    }

    [Test]
    public void TryReadLine_EmptyFile_ReturnsFalse ()
    {
        using var stream = CreateStream("");
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 500);

        Assert.That(reader.TryReadLine(out _), Is.False);
    }

    [Test]
    public void TryReadLine_EmptyLines_Preserved ()
    {
        using var stream = CreateStream("\n\n\n");
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 500);

        var count = 0;
        while (reader.TryReadLine(out var line))
        {
            Assert.That(line.Length, Is.EqualTo(0));
            count++;
        }

        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Tests")]
    public void TryReadLine_NoTrailingNewline_EmitsLastLine ()
    {
        using var stream = CreateStream("line1\nline2");
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 500);

        Assert.That(reader.TryReadLine(out var line1), Is.True);
        Assert.That(line1.Span.ToString(), Is.EqualTo("line1"));

        Assert.That(reader.TryReadLine(out var line2), Is.True);
        Assert.That(line2.Span.ToString(), Is.EqualTo("line2"));

        Assert.That(reader.TryReadLine(out _), Is.False);
    }

    #endregion

    #region MaximumLineLength

    [Test]
    public void TryReadLine_LongLine_Truncated ()
    {
        var longLine = new string('x', 1000);
        using var stream = CreateStream(longLine + "\nshort\n");
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 100);

        Assert.That(reader.TryReadLine(out var line1), Is.True);
        Assert.That(line1.Length, Is.EqualTo(100));

        Assert.That(reader.TryReadLine(out var line2), Is.True);
        Assert.That(line2.Span.ToString(), Is.EqualTo("short"));
    }

    #endregion

    #region Cross-block boundary

    [Test]
    public void TryReadLine_LineCrossesBlockBoundary_Correct ()
    {
        // Create content that forces a line to cross the internal block boundary.
        // Use a known block size and a line longer than remaining space.
        var sb = new StringBuilder();
        // Fill near the block boundary with short lines, then one long line
        for (var i = 0; i < 400; i++)
        {
            _ = sb.AppendLine(new string('A', 80)); // ~81 chars per line with \n
        }
        // At this point we're ~32,400 chars in. Add a long line that crosses the boundary.
        var crossingLine = new string('B', 500);
        _ = sb.AppendLine(crossingLine);
        _ = sb.AppendLine("final line");

        var text = sb.ToString();
        using var stream = CreateStream(text);
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 1000);

        // Read all lines and verify the crossing line
        var lineNum = 0;
        var foundCrossing = false;
        while (reader.TryReadLine(out ReadOnlyMemory<char> line))
        {
            if (lineNum >= 400 && line.Length == 500)
            {
                Assert.That(line.Span.ToString(), Is.EqualTo(crossingLine));
                foundCrossing = true;
            }

            lineNum++;
        }

        Assert.That(foundCrossing, Is.True, "Should have found the 500-char crossing line");
    }

    #endregion

    #region Position parity with System reader

    [Test]
    [TestCase("Line 1\nLine 2\nLine 3\n")]
    [TestCase("Line 1\r\nLine 2\r\nLine 3\r\n")]
    [TestCase("Short\nMedium line here\nA much longer line with more content\n")]
    public void Position_MatchesSystemReader_UTF8 (string text)
    {
        ComparePositions(text, Encoding.UTF8);
    }

    [Test]
    [TestCase("Line 1\nLine 2\nLine 3\n")]
    public void Position_MatchesSystemReader_ASCII (string text)
    {
        ComparePositions(text, Encoding.ASCII);
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Tests")]
    public void Position_MatchesSystemReader_UTF8_MultibyteChars ()
    {
        var text = "Héllo wörld\nCafé résumé\nこんにちは\n";
        ComparePositions(text, Encoding.UTF8);
    }

    private static void ComparePositions (string text, Encoding encoding)
    {
        var bytes = encoding.GetBytes(text);

        using var s1 = new MemoryStream(bytes);
        using var s2 = new MemoryStream(bytes);
        using var systemReader = new PositionAwareStreamReaderSystem(s1, new EncodingOptions { Encoding = encoding }, 500);
        using var directReader = new PositionAwareStreamReaderDirect(s2, new EncodingOptions { Encoding = encoding }, 500);

        var lineNum = 0;
        while (true)
        {
            var systemHasLine = systemReader.TryReadLine(out var systemLine);
            var directHasLine = directReader.TryReadLine(out var directLine);

            Assert.That(directHasLine, Is.EqualTo(systemHasLine), $"Line {lineNum}: EOF mismatch");

            if (!systemHasLine)
            {
                break;
            }

            Assert.That(directLine.Span.ToString(), Is.EqualTo(systemLine.Span.ToString()),
                $"Line {lineNum}: content mismatch");
            Assert.That(directReader.Position, Is.EqualTo(systemReader.Position),
                $"Line {lineNum}: position mismatch (System={systemReader.Position}, Direct={directReader.Position})");

            lineNum++;
        }
    }

    #endregion

    #region Content parity with System reader

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Unit Tests")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Unit Tests")]
    public void ContentParity_LargeFile ()
    {
        var sb = new StringBuilder();
        var rng = new Random(42);
        for (var i = 0; i < 10_000; i++)
        {
            _ = sb.Append("2026-04-23 12:00:00.");
            _ = sb.Append(i % 1000);
            _ = sb.Append(" [INFO] Thread-");
            _ = sb.Append(rng.Next(1, 32));
            _ = sb.Append(" SomeClass - Log message number ");
            _ = sb.AppendLine(i.ToString());
        }

        var text = sb.ToString();
        var bytes = Encoding.UTF8.GetBytes(text);

        using var s1 = new MemoryStream(bytes);
        using var s2 = new MemoryStream(bytes);
        using var systemReader = new PositionAwareStreamReaderSystem(s1, new EncodingOptions { Encoding = Encoding.UTF8 }, 500);
        using var directReader = new PositionAwareStreamReaderDirect(s2, new EncodingOptions { Encoding = Encoding.UTF8 }, 500);

        var lineNum = 0;
        while (systemReader.TryReadLine(out var systemLine))
        {
            Assert.That(directReader.TryReadLine(out var directLine), Is.True, $"Direct reader ended early at line {lineNum}");
            Assert.That(directLine.Span.ToString(), Is.EqualTo(systemLine.Span.ToString()), $"Line {lineNum}: content mismatch");
            lineNum++;
        }

        Assert.That(directReader.TryReadLine(out _), Is.False, "Direct reader has extra lines");
        Assert.That(lineNum, Is.EqualTo(10_000));
    }

    #endregion

    #region DetachBlocks

    [Test]
    public void DetachBlocks_ReturnsCompletedBlocks ()
    {
        // Create enough content to fill multiple blocks
        var sb = new StringBuilder();
        for (var i = 0; i < 1000; i++)
        {
            _ = sb.AppendLine(new string('X', 80));
        }

        using var stream = CreateStream(sb.ToString());
        using var reader = new PositionAwareStreamReaderDirect(stream, new EncodingOptions(), 500);

        // Read all lines
        while (reader.TryReadLine(out _))
        {
        }

        var blocks = reader.DetachBlocks();
        Assert.That(blocks.Count, Is.GreaterThan(0), "Should have completed blocks to detach");

        // Second detach should be empty
        var blocks2 = reader.DetachBlocks();
        Assert.That(blocks2.Count, Is.EqualTo(0));
    }

    #endregion
}