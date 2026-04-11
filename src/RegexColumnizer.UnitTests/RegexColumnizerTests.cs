using System.Runtime.Versioning;

using ColumnizerLib;

using Moq;

using NUnit.Framework;

using RegexColumnizer;

[assembly: SupportedOSPlatform("windows")]
namespace LogExpert.RegexColumnizer.Tests;

[TestFixture]
public partial class RegexColumnizerBasicTests
{
    // The same amount of columns should be returned whether the line matches the regex or not.
    [TestCase("5 test message", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 2)]
    [TestCase("Error in com.example.core", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 2)]
    [TestCase("Simple line", @"(?'text'.*)", 1)]
    public void SplitLine_ColumnCountMatches (string lineToParse, string regex, int expectedNumberOfColumns)
    {
        var columnizer = TestLogLine.CreateColumnizer(regex);

        TestLogLine testLogLine = new(4, lineToParse);
        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        Assert.That(parsedLogLine.ColumnValues.Length, Is.EqualTo(expectedNumberOfColumns));
    }

    //Using "" for empty string since string.Empty can't be passed to the TestCase attribute.
    [TestCase("5 test message", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 0, "5")]
    [TestCase("5 test message", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 1, "test message")]
    [TestCase("Error in com.example.core", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 0, "")] // doesn't match regex so should be empty
    [TestCase("Error in com.example.core", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 1, "Error in com.example.core")]
    public void SplitLine_ColumnValues (string lineToParse, string regex, int columnIndexToTest,
        string expectedColumnValue)
    {
        var columnizer = TestLogLine.CreateColumnizer(regex);

        TestLogLine testLogLine = new(3, lineToParse);
        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        Assert.That(parsedLogLine.ColumnValues[columnIndexToTest].Text.ToString(), Is.EqualTo(expectedColumnValue));
    }

    [Test]
    public void GetColumnNames_ExtractsNamedGroups ()
    {
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<time>\d+)\s+(?<level>\w+)\s+(?<message>.*)$");

        var columnNames = columnizer.GetColumnNames();

        Assert.That(columnNames, Is.EqualTo(["time", "level", "message"]));
    }

    [Test]
    public void GetColumnCount_ReturnsCorrectCount ()
    {
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<col1>\w+)\s+(?<col2>\w+)$");

        Assert.That(columnizer.GetColumnCount(), Is.EqualTo(2));
    }

    [Test]
    public void GetName_ReturnsConfiguredName ()
    {
        var columnizer = TestLogLine.CreateColumnizer(@"(?<text>.*)", "Custom Name");

        Assert.That(columnizer.GetName(), Is.EqualTo("Custom Name"));
    }

    [Test]
    public void GetName_ReturnsDefaultWhenNotConfigured ()
    {
        var columnizer = new Regex1Columnizer();
        columnizer.LoadConfig(Path.GetTempPath()); // Load with defaults

        Assert.That(columnizer.GetName(), Is.EqualTo("Regex1"));
    }

    [Test]
    public void SplitLine_NonMatchingLine_PlacesInLastColumn ()
    {
        var columnizer = TestLogLine.CreateColumnizer(@"^(?<digits>\d+)\s+(?<text>.*)$");
        TestLogLine testLogLine = new(1, "No digits at start");

        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        // First column should be empty
        Assert.That(parsedLogLine.ColumnValues[0].Text.ToString(), Is.Empty);
        // Last column should contain the full line
        Assert.That(parsedLogLine.ColumnValues[1].Text.ToString(), Is.EqualTo("No digits at start"));
    }

    [Test]
    public void SplitLine_EmptyLine_HandlesGracefully ()
    {
        var columnizer = TestLogLine.CreateColumnizer(@"(?<text>.*)");
        TestLogLine testLogLine = new(1, "");

        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineMemoryColumnizerCallback>(), testLogLine);

        Assert.That(parsedLogLine.ColumnValues.Length, Is.EqualTo(1));
        Assert.That(parsedLogLine.ColumnValues[0].Text.ToString(), Is.Empty);
    }
}
