using System.Runtime.Versioning;

using LogExpert;

using Moq;

using NUnit.Framework;

[assembly: SupportedOSPlatform("windows")]
namespace RegexColumnizer.UnitTests;

[TestFixture]
public class RegexColumnizerBasicTests
{
    // The same amount of columns should be returned whether the line matches the regex or not.
    [TestCase("5 test message", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 2)]
    [TestCase("Error in com.example.core", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 2)]
    [TestCase("Simple line", @"(?'text'.*)", 1)]
    public void SplitLine_ColumnCountMatches(string lineToParse, string regex, int expectedNumberOfColumns)
    {
        var columnizer = CreateInitializedColumnizer(regex);

        TestLogLine testLogLine = new(4, lineToParse);
        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);

        Assert.That(parsedLogLine.ColumnValues.Length, Is.EqualTo(expectedNumberOfColumns));
    }

    //Using "" for empty string since string.Empty can't be passed to the TestCase attribute.
    [TestCase("5 test message", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 0, "5")]
    [TestCase("5 test message", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 1, "test message")]
    [TestCase("Error in com.example.core", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 0, "")] // doesn't match regex so should be empty
    [TestCase("Error in com.example.core", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 1, "Error in com.example.core")]
    public void SplitLine_ColumnValues(string lineToParse, string regex, int columnIndexToTest,
        string expectedColumnValue)
    {
        var columnizer = CreateInitializedColumnizer(regex);

        TestLogLine testLogLine = new(3, lineToParse);
        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);

        Assert.That(parsedLogLine.ColumnValues[columnIndexToTest].Text, Is.EqualTo(expectedColumnValue));
    }

    [Test]
    public void GetColumnNames_ExtractsNamedGroups()
    {
        var columnizer = CreateInitializedColumnizer(@"^(?<time>\d+)\s+(?<level>\w+)\s+(?<message>.*)$");

        var columnNames = columnizer.GetColumnNames();

        Assert.That(columnNames, Is.EqualTo(new[] { "time", "level", "message" }));
    }

    [Test]
    public void GetColumnCount_ReturnsCorrectCount()
    {
        var columnizer = CreateInitializedColumnizer(@"^(?<col1>\w+)\s+(?<col2>\w+)$");

        Assert.That(columnizer.GetColumnCount(), Is.EqualTo(2));
    }

    [Test]
    public void GetName_ReturnsConfiguredName()
    {
        var columnizer = CreateInitializedColumnizer(@"(?<text>.*)", "Custom Name");

        Assert.That(columnizer.GetName(), Is.EqualTo("Custom Name"));
    }

    [Test]
    public void GetName_ReturnsDefaultWhenNotConfigured()
    {
        var columnizer = new Regex1Columnizer();
        columnizer.LoadConfig(Path.GetTempPath()); // Load with defaults

        Assert.That(columnizer.GetName(), Is.EqualTo("Regex1"));
    }

    [Test]
    public void SplitLine_NonMatchingLine_PlacesInLastColumn()
    {
        var columnizer = CreateInitializedColumnizer(@"^(?<digits>\d+)\s+(?<text>.*)$");
        TestLogLine testLogLine = new(1, "No digits at start");

        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);

        // First column should be empty
        Assert.That(parsedLogLine.ColumnValues[0].Text, Is.Empty);
        // Last column should contain the full line
        Assert.That(parsedLogLine.ColumnValues[1].Text, Is.EqualTo("No digits at start"));
    }

    [Test]
    public void SplitLine_EmptyLine_HandlesGracefully()
    {
        var columnizer = CreateInitializedColumnizer(@"(?<text>.*)");
        TestLogLine testLogLine = new(1, "");

        var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);

        Assert.That(parsedLogLine.ColumnValues.Length, Is.EqualTo(1));
        Assert.That(parsedLogLine.ColumnValues[0].Text, Is.Empty);
    }

    private Regex1Columnizer CreateInitializedColumnizer(string regex, string name = "Test regex")
    {
        RegexColumnizerConfig columnizerConfig = new()
        {
            Expression = regex,
            Name = name
        };

        Regex1Columnizer columnizer = new();
        
        // Use reflection to set private _config field and call Init()
        var configField = typeof(BaseRegexColumnizer).GetField("_config", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, columnizerConfig);
        
        columnizer.Init();
        
        return columnizer;
    }

    private class TestLogLine : ILogLine
    {
        public TestLogLine(int lineNumber, string fullLine)
        {
            LineNumber = lineNumber;
            FullLine = fullLine;
        }

        public string FullLine { get; set; }

        public int LineNumber { get; set; }

        public string Text { get; set; }
    }
}
