using ColumnizerLib;

using RegexColumnizer;

namespace LogExpert.RegexColumnizer.Tests;

internal class TestLogLine (int lineNumber, string fullLine) : ILogLine
{
    public string FullLine { get; set; } = fullLine;

    public int LineNumber { get; set; } = lineNumber;

    public string Text { get; set; }

    public static Regex1Columnizer CreateColumnizer (string regex, string customName = "Test Columnizer")
    {
        var config = new RegexColumnizerConfig
        {
            Expression = regex,
            Name = customName
        };

        var columnizer = new Regex1Columnizer();

        var configField = typeof(BaseRegexColumnizer).GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);

        columnizer.Init();

        return columnizer;
    }
}
