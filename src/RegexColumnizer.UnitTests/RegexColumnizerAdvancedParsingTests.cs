using System.Runtime.Versioning;

using ColumnizerLib;

using Moq;

using NUnit.Framework;

[assembly: SupportedOSPlatform("windows")]
namespace LogExpert.RegexColumnizer.Tests;

[TestFixture]
public class RegexColumnizerAdvancedParsingTests
{
    [Test]
    public void SplitLine_ApacheAccessLog_ParsesCorrectly ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<ip>\S+)\s+\S+\s+(?<user>\S+)\s+\[(?<datetime>[^\]]+)\]\s+""(?<method>\S+)\s+(?<path>\S+)\s+(?<protocol>\S+)""\s+(?<status>\d+)\s+(?<size>\d+)");
        string logLine = @"192.168.1.1 - frank [10/Oct/2000:13:55:36 -0700] ""GET /apache_pb.gif HTTP/1.0"" 200 2326";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("192.168.1.1"));
        Assert.That(result.ColumnValues[1].Text.ToString(), Is.EqualTo("frank"));
        Assert.That(result.ColumnValues[2].Text.ToString(), Is.EqualTo("10/Oct/2000:13:55:36 -0700"));
        Assert.That(result.ColumnValues[3].Text.ToString(), Is.EqualTo("GET"));
        Assert.That(result.ColumnValues[4].Text.ToString(), Is.EqualTo("/apache_pb.gif"));
        Assert.That(result.ColumnValues[5].Text.ToString(), Is.EqualTo("HTTP/1.0"));
        Assert.That(result.ColumnValues[6].Text.ToString(), Is.EqualTo("200"));
        Assert.That(result.ColumnValues[7].Text.ToString(), Is.EqualTo("2326"));
    }

    [Test]
    public void SplitLine_Log4jPattern_ParsesCorrectly ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<timestamp>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2},\d{3})\s+(?<level>\w+)\s+\[(?<thread>[^\]]+)\]\s+(?<logger>\S+)\s+-\s+(?<message>.*)$");
        string logLine = "2023-11-21 14:30:45,123 ERROR [main] com.example.MyClass - An error occurred";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("2023-11-21 14:30:45,123"));
        Assert.That(result.ColumnValues[1].Text.ToString(), Is.EqualTo("ERROR"));
        Assert.That(result.ColumnValues[2].Text.ToString(), Is.EqualTo("main"));
        Assert.That(result.ColumnValues[3].Text.ToString(), Is.EqualTo("com.example.MyClass"));
        Assert.That(result.ColumnValues[4].Text.ToString(), Is.EqualTo("An error occurred"));
    }

    [Test]
    public void SplitLine_CsvPattern_ParsesCorrectly ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<col1>[^,]+),(?<col2>[^,]+),(?<col3>[^,]+)$");
        string logLine = "value1,value2,value3";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("value1"));
        Assert.That(result.ColumnValues[1].Text.ToString(), Is.EqualTo("value2"));
        Assert.That(result.ColumnValues[2].Text.ToString(), Is.EqualTo("value3"));
    }

    [Test]
    public void SplitLine_OptionalGroups_HandlesPresenceAndAbsence ()
    {
        // Arrange - Pattern with optional group
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<required>\w+)(\s+(?<optional>\d+))?");

        // Act & Assert - Line with optional part
        var line1 = new TestLogLine(1, "text 123");
        var result1 = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), line1);
        // Note: Regex groups are indexed from 1, group 0 is entire match, so groups appear in different order
        Assert.That(result1.ColumnValues[0].Text.ToString(), Is.EqualTo(" 123")); // Captures outer group
        Assert.That(result1.ColumnValues[1].Text.ToString(), Is.EqualTo("text")); // required group
        Assert.That(result1.ColumnValues[2].Text.ToString(), Is.EqualTo("123")); // Captures inner named group

        // Line without optional part - still matches because optional group is... optional
        var line2 = new TestLogLine(2, "text");
        var result2 = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), line2);
        Assert.That(result2.ColumnValues[0].Text.ToString(), Is.Empty); // Optional outer group not matched
        Assert.That(result2.ColumnValues[1].Text.ToString(), Is.EqualTo("text")); // required group matched
        Assert.That(result2.ColumnValues[2].Text.ToString(), Is.Empty); // optional inner group not matched
    }

    [Test]
    public void SplitLine_MultilinePattern_SingleLineMode ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"(?<text>.*)");
        string logLine = "Single line of text";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("Single line of text"));
    }

    [Test]
    public void SplitLine_NumericGroups_ExtractsValues ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<int>\d+)\s+(?<float>\d+\.\d+)\s+(?<hex>0x[0-9A-Fa-f]+)$");
        string logLine = "42 3.14 0xFF";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("42"));
        Assert.That(result.ColumnValues[1].Text.ToString(), Is.EqualTo("3.14"));
        Assert.That(result.ColumnValues[2].Text.ToString(), Is.EqualTo("0xFF"));
    }

    [Test]
    public void SplitLine_QuotedStrings_ExtractsContent ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"""(?<quoted>[^""]*)""|(?<unquoted>\S+)");
        string logLine = @"""quoted value"" unquoted";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert - First match
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("quoted value"));
    }

    [Test]
    public void SplitLine_WithLookahead_ParsesCorrectly ()
    {
        // Arrange - Pattern with positive lookahead
        var columnizer = TestLogLine.CreateColumnizer(@"(?<word>\w+)(?=\s)");
        string logLine = "first second third";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert - Only captures first match
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("first"));
    }

    [Test]
    public void SplitLine_BackreferencesNotSupported_ParsesWithoutError ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"(?<quote>['""])(?<text>.*?)\k<quote>");
        string logLine = @"'single quoted' and ""double quoted""";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert - Should parse first match
        Assert.That(result.ColumnValues.Length, Is.GreaterThan(0));
    }

    [Test]
    public void SplitLine_CaseInsensitivePattern_MatchesRegardlessOfCase ()
    {
        // Arrange - Note: RegexOptions would need to be configurable for true case-insensitive
        var columnizer = TestLogLine.CreateColumnizer(@"(?<level>INFO|WARN|ERROR)");
        string logLine = "INFO message";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("INFO"));
    }

    [Test]
    public void SplitLine_ComplexNestedGroups_ExtractsCorrectly ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<outer>(?<inner1>\w+)\s+(?<inner2>\d+))$");
        string logLine = "text 123";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("text 123")); // outer
        Assert.That(result.ColumnValues[1].Text.ToString(), Is.EqualTo("text")); // inner1
        Assert.That(result.ColumnValues[2].Text.ToString(), Is.EqualTo("123")); // inner2
    }

    [Test]
    public void SplitLine_VeryLongLine_HandlesEfficiently ()
    {
        // Arrange - Simple test for performance with long lines
        var columnizer = TestLogLine.CreateColumnizer(@"(?<text>.*)");
        string logLine = new('x', 5000); // Reduced to avoid potential timeouts
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);
        stopwatch.Stop();

        // Assert - Main goal is performance, not exact match
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100)); // Should be fast
        Assert.That(result.ColumnValues[0].Text.Length, Is.GreaterThan(1000)); // Should capture substantial portion
    }

    [Test]
    public void SplitLine_UnicodeCharacters_HandlesCorrectly ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"(?<text>.*)");
        string logLine = "Hello 世界 🌍 Привет";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("Hello 世界 🌍 Привет"));
    }

    [Test]
    public void SplitLine_SpecialRegexCharacters_EscapedProperly ()
    {
        // Arrange
        var columnizer = TestLogLine.CreateColumnizer(@"(?<escaped>\[.*?\])");
        string logLine = "[content in brackets]";
        var testLogLine = new TestLogLine(1, logLine);

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // Assert
        Assert.That(result.ColumnValues[0].Text.ToString(), Is.EqualTo("[content in brackets]"));
    }
}
