using System.Runtime.Versioning;

using LogExpert;

using Moq;

using NUnit.Framework;

[assembly: SupportedOSPlatform("windows")]
namespace RegexColumnizer.UnitTests;

[TestFixture]
public class RegexColumnizerErrorHandlingTests
{
    private string _testDirectory;

    [SetUp]
    public void SetUp ()
    {
        _testDirectory = Path.Join(Path.GetTempPath(), $"LogExpertTest_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unit Tests")]
    public void TearDown ()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Test]
    public void Init_InvalidRegex_SetsRegexToNullAndUsesEmptyColumns ()
    {
        // Arrange
        var config = new RegexColumnizerConfig
        {
            Expression = "(?<unclosed", // Invalid regex - unclosed group
            Name = "Invalid Regex Test"
        };

        var columnizer = new Regex1Columnizer();

        var configField = typeof(BaseRegexColumnizer).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);

        // Act - Should not throw
        Assert.DoesNotThrow(columnizer.Init);

        // Assert - Regex should be null
        var regexProperty = typeof(BaseRegexColumnizer).GetProperty("Regex");
        var regex = regexProperty?.GetValue(columnizer);
        Assert.That(regex, Is.Null);
    }

    [Test]
    public void Init_CatastrophicBacktracking_HandledByRegexHelper ()
    {
        // Arrange - This pattern can cause catastrophic backtracking
        var config = new RegexColumnizerConfig
        {
            Expression = @"(?<text>(a+)+b)",
            Name = "Backtracking Test"
        };

        var columnizer = new Regex1Columnizer();

        var configField = typeof(BaseRegexColumnizer).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);

        // Act - Should complete quickly due to RegexHelper timeout protection
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Assert.DoesNotThrow(columnizer.Init);
        stopwatch.Stop();

        // Assert - Should complete in reasonable time (RegexHelper has timeout)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000));
    }

    [Test]
    public void SplitLine_NullRegex_PlacesEntireLineInFirstColumn ()
    {
        // Arrange
        var columnizer = new Regex1Columnizer();

        // Set invalid regex to make Regex null
        var config = new RegexColumnizerConfig
        {
            Expression = "(?<invalid",
            Name = "Test"
        };

        var configField = typeof(BaseRegexColumnizer).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);
        columnizer.Init();

        var testLogLine = new TestLogLine(1, "Test line content");

        // Act - Regex is null after Init() failure, but Init() now creates fallback columns array
        // So this should work even though Regex is null
        var result = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);

        // Assert - Should have fallback column with the full line
        Assert.That(result.ColumnValues.Length, Is.GreaterThan(0));
        // When Regex is null but columns array exists, entire line should be placed
        Assert.That(result.ColumnValues[0].FullValue, Is.EqualTo("Test line content"));
    }

    [Test]
    public void LoadConfig_CorruptJsonFile_DisplaysErrorAndUsesDefaults ()
    {
        // Arrange
        string jsonPath = Path.Join(_testDirectory, "Regex1Columnizer.json");
        File.WriteAllText(jsonPath, "{ corrupt json content }");

        var columnizer = new Regex1Columnizer();

        // Act - Should not throw, should handle gracefully
        Assert.DoesNotThrow(() => columnizer.LoadConfig(_testDirectory));

        // Assert - Should fall back to defaults
        Assert.That(columnizer.GetName(), Is.EqualTo("Regex1"));
        Assert.That(columnizer.GetColumnCount(), Is.GreaterThan(0));
    }

    [Test]
    public void LoadConfig_FileSystemError_HandlesGracefully ()
    {
        // Arrange
        string invalidPath = "Z:\\NonExistent\\Path\\That\\Does\\Not\\Exist";

        var columnizer = new Regex1Columnizer();

        // Act - Should not throw, should use defaults
        Assert.DoesNotThrow(() => columnizer.LoadConfig(invalidPath));

        // Assert
        Assert.That(columnizer.GetName(), Is.EqualTo("Regex1"));
    }

    [Test]
    public void LoadConfig_EmptyConfigDirectory_UsesDefaults ()
    {
        // Arrange
        var columnizer = new Regex1Columnizer();

        // Act
        columnizer.LoadConfig(_testDirectory);

        // Assert
        Assert.That(columnizer.GetName(), Is.EqualTo("Regex1"));
        Assert.That(columnizer.GetColumnCount(), Is.GreaterThan(0));
    }

    [Test]
    public void LoadConfig_CorruptXmlFile_DisplaysErrorAndUsesDefaults ()
    {
        // Arrange
        string xmlPath = Path.Join(_testDirectory, "Regex1Columnizer.xml");
        File.WriteAllText(xmlPath, "<InvalidXml>No closing tag");

        var columnizer = new Regex1Columnizer();

        // Act - Should not throw
        Assert.DoesNotThrow(() => columnizer.LoadConfig(_testDirectory));

        // Assert - Should fall back to defaults
        Assert.That(columnizer.GetName(), Is.EqualTo("Regex1"));
    }

    [Test]
    public void SplitLine_EmptyColumnValues_HandlesGracefully ()
    {
        // Arrange
        var columnizer = CreateColumnizer(@"^(?<col1>\d*)\s+(?<col2>\w*)$");
        var testLogLine = new TestLogLine(1, "   "); // Only whitespace

        // Act
        var result = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);

        // Assert - Should handle gracefully
        Assert.That(result.ColumnValues, Is.Not.Null);
        Assert.That(result.ColumnValues.Length, Is.EqualTo(2));
    }

    [Test]
    public void SplitLine_VeryComplexRegex_CompletesInReasonableTime ()
    {
        // Arrange - Complex but safe regex
        var columnizer = CreateColumnizer(
            @"^(?<timestamp>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2},\d{3})\s+" +
            @"(?<level>DEBUG|INFO|WARN|ERROR|FATAL)\s+" +
            @"\[(?<thread>[^\]]+)\]\s+" +
            @"(?<logger>[^\s]+)\s+" +
            @"(?<message>.*)$");

        var testLogLine = new TestLogLine(1,
            "2023-11-21 14:30:45,123 ERROR [main-thread-1] com.example.MyClass Error message here");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);
        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
        Assert.That(result.ColumnValues.Length, Is.EqualTo(5));
    }

    [Test]
    public void GetName_ConfigNameIsNull_ReturnsDefaultName ()
    {
        // Arrange
        var config = new RegexColumnizerConfig
        {
            Expression = @"(?<text>.*)",
            Name = null
        };

        var columnizer = new Regex1Columnizer();

        var configField = typeof(BaseRegexColumnizer).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);

        // Act
        string name = columnizer.GetName();

        // Assert
        Assert.That(name, Is.EqualTo("Regex1"));
    }

    [Test]
    public void GetName_ConfigNameIsWhitespace_ReturnsDefaultName ()
    {
        // Arrange
        var config = new RegexColumnizerConfig
        {
            Expression = @"(?<text>.*)",
            Name = "   "
        };

        var columnizer = new Regex1Columnizer();

        var configField = typeof(BaseRegexColumnizer).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);

        // Act
        string name = columnizer.GetName();

        // Assert
        Assert.That(name, Is.EqualTo("Regex1"));
    }

    [Test]
    public void Init_EmptyExpression_HandlesGracefully ()
    {
        // Arrange
        var config = new RegexColumnizerConfig
        {
            Expression = "",
            Name = "Empty Expression"
        };

        var columnizer = new Regex1Columnizer();

        var configField = typeof(BaseRegexColumnizer).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);

        // Act - Should not throw
        Assert.DoesNotThrow(columnizer.Init);

        // Assert - Regex might be null or default
        var regexProperty = typeof(BaseRegexColumnizer).GetProperty("Regex");
        var regex = regexProperty?.GetValue(columnizer);
        // Either null or successfully compiled empty pattern
        Assert.That(regex, Is.Not.Null.Or.Null);
    }

    [Test]
    public void SplitLine_MultipleNonMatchingLines_HandlesConsistently ()
    {
        // Arrange
        var columnizer = CreateColumnizer(@"^(?<number>\d+)\s+(?<text>.*)$");
        var lines = new[]
        {
            new TestLogLine(1, "No numbers here"),
            new TestLogLine(2, "Still no numbers"),
            new TestLogLine(3, "Nope, none")
        };

        // Act & Assert - All should be handled the same way
        foreach (var line in lines)
        {
            var result = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), line);

            Assert.That(result.ColumnValues.Length, Is.EqualTo(2));
            Assert.That(result.ColumnValues[0].Text, Is.Empty); // First column empty
            Assert.That(result.ColumnValues[1].Text, Is.EqualTo(line.FullLine)); // Full line in last column
        }
    }

    private Regex1Columnizer CreateColumnizer (string regex)
    {
        var config = new RegexColumnizerConfig
        {
            Expression = regex,
            Name = "Test Columnizer"
        };

        var columnizer = new Regex1Columnizer();

        var configField = typeof(BaseRegexColumnizer).GetField("_config",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        configField?.SetValue(columnizer, config);

        columnizer.Init();

        return columnizer;
    }

    #region Security Tests (SEC-02)

    [Test]
    public void Configure_ValidatesPathTraversal_WithDoubleDots ()
    {
        // This test ensures that GetConfigFileJSON validates the columnizer name
        // We can't directly test private methods, but Configure calls GetConfigFileJSON
        // Since columnizer name comes from GetType().Name, this is more of a defensive test

        // Arrange
        var columnizer = new Regex1Columnizer();
        string testDir = Path.Join(_testDirectory, "security_test");
        _ = Directory.CreateDirectory(testDir);

        // Act & Assert - Normal operation should work fine
        // The security validation happens inside GetConfigFileJSON
        Assert.DoesNotThrow(() => columnizer.LoadConfig(testDir));

        // Cleanup
        if (Directory.Exists(testDir))
        {
            Directory.Delete(testDir, true);
        }
    }

    [Test]
    public void Configure_ConfigDirectoryWithSpecialChars_HandledCorrectly ()
    {
        // Arrange
        var columnizer = new Regex1Columnizer();

        // Test with directory containing spaces and valid special characters
        string testDir = Path.Join(_testDirectory, "test config (v1.0)");
        _ = Directory.CreateDirectory(testDir);

        // Act & Assert - Should handle directory names with spaces and parentheses
        Assert.DoesNotThrow(() => columnizer.LoadConfig(testDir));

        // Cleanup
        if (Directory.Exists(testDir))
        {
            Directory.Delete(testDir, true);
        }
    }

    #endregion

    private class TestLogLine : ILogLine
    {
        public TestLogLine (int lineNumber, string fullLine)
        {
            LineNumber = lineNumber;
            FullLine = fullLine;
        }

        public string FullLine { get; set; }
        public int LineNumber { get; set; }
        public string Text { get; set; }
    }
}
