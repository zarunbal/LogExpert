
using ColumnizerLib;

using RegexColumnizer;

namespace LogExpert.RegexColumnizer.Tests;

internal class TestLogLine (int lineNumber, string fullLine) : ILogLineMemory
{
    public int LineNumber { get; set; } = lineNumber;

    public ReadOnlyMemory<char> FullLine { get; } = fullLine.AsMemory();

    public ReadOnlyMemory<char> Text { get; }

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
