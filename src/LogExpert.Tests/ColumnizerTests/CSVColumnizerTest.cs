using System.Reflection;

using ColumnizerLib;

using CsvColumnizer;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;

using Moq;

using NUnit.Framework;

namespace LogExpert.Tests.ColumnizerTests;

[TestFixture]
public class CSVColumnizerTest
{
    [SetUp]
    public void Setup ()
    {
        // Reset singleton for testing (same pattern as PluginRegistryTests)
        ResetPluginRegistrySingleton();

        // Initialize plugin registry with proper test directory
        var testDataPath = Path.Join(Path.GetTempPath(), "LogExpertTests", Guid.NewGuid().ToString());
        _ = Directory.CreateDirectory(testDataPath);

        var pluginRegistry = PluginRegistry.PluginRegistry.Create(testDataPath, 250);

        // Verify the local file system plugin is registered
        var localPlugin = pluginRegistry.FindFileSystemForUri(@"C:\test.txt");
        Assert.That(localPlugin, Is.Not.Null, "Local file system plugin not registered!");
    }

    [TearDown]
    public void TearDown ()
    {
        ResetPluginRegistrySingleton();
    }

    /// <summary>
    /// Uses reflection to reset the singleton instance for testing.
    /// This ensures each test starts with a fresh PluginRegistry state.
    /// </summary>
    private static void ResetPluginRegistrySingleton ()
    {
        var instanceField = typeof(PluginRegistry.PluginRegistry).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        instanceField?.SetValue(null, null);
    }

    /// <summary>
    /// Sets a private field on the CsvColumnizer via reflection.
    /// Used to reach edge-case states (e.g. _isValidCsv=true with _firstLine=null)
    /// that are not reachable through the public API alone.
    /// </summary>
    private static void SetPrivateField (CsvColumnizer.CsvColumnizer columnizer, string fieldName, object? value)
    {
        var field = typeof(CsvColumnizer.CsvColumnizer).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field '{fieldName}' not found on CsvColumnizer");
        field!.SetValue(columnizer, value);
    }

    [Test]
    public void Selected_HasFieldNames_FirstLineNull_FallsBackToCallback ()
    {
        // Arrange: _isValidCsv=true but _firstLine=null (edge case reachable via legacy adapter)
        CsvColumnizer.CsvColumnizer columnizer = new();
        SetPrivateField(columnizer, "_isValidCsv", true);

        var callbackLine = new CsvLogLine("name;age;city", 0);
        var mockCallback = new Mock<ILogLineMemoryColumnizerCallback>();
        _ = mockCallback.Setup(c => c.GetLogLineMemory(0)).Returns(callbackLine);

        // Act
        columnizer.Selected(mockCallback.Object);

        // Assert: columns detected from callback line
        Assert.That(columnizer.GetColumnCount(), Is.EqualTo(3));
        Assert.That(columnizer.GetColumnNames(), Is.EqualTo(["name", "age", "city"]));
    }

    [Test]
    public void Selected_HasFieldNames_NoLineAvailable_FallsBackToTextColumn ()
    {
        // Arrange: _isValidCsv=true, _firstLine=null, callback returns null
        CsvColumnizer.CsvColumnizer columnizer = new();
        SetPrivateField(columnizer, "_isValidCsv", true);

        var mockCallback = new Mock<ILogLineMemoryColumnizerCallback>();
        _ = mockCallback.Setup(c => c.GetLogLineMemory(0)).Returns((ILogLineMemory?)null);

        // Act
        columnizer.Selected(mockCallback.Object);

        // Assert: graceful fallback to single "Text" column
        Assert.That(columnizer.GetColumnCount(), Is.EqualTo(1));
        Assert.That(columnizer.GetColumnNames(), Is.EqualTo(["Text"]));
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Tests")]
    public void SplitLine_EmptyFullLine_ReturnsSingleColumn ()
    {
        // Arrange: columnizer in valid-CSV mode with columns established
        CsvColumnizer.CsvColumnizer columnizer = new();
        _ = columnizer.PreProcessLine("a;b;c".AsMemory(), 0, 0);

        var mockCallback = new Mock<ILogLineMemoryColumnizerCallback>();
        columnizer.Selected(mockCallback.Object);

        // Act: split a line with empty FullLine
        var emptyLine = new CsvLogLine(ReadOnlyMemory<char>.Empty, 1);
        var result = columnizer.SplitLine(null, emptyLine);

        // Assert: single column with empty content, no crash
        Assert.That(result.ColumnValues, Has.Length.EqualTo(1));
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Tests")]
    public void SplitLine_ValidCsvLine_ReturnsCorrectColumns ()
    {
        // Arrange: columnizer in valid-CSV mode
        CsvColumnizer.CsvColumnizer columnizer = new();
        _ = columnizer.PreProcessLine("name;age;city".AsMemory(), 0, 0);

        var mockCallback = new Mock<ILogLineMemoryColumnizerCallback>();
        columnizer.Selected(mockCallback.Object);

        // Act: split a normal data line
        var dataLine = new CsvLogLine("Alice;30;London", 1);
        var result = columnizer.SplitLine(null, dataLine);

        // Assert: three columns with correct values
        Assert.That(result.ColumnValues, Has.Length.EqualTo(3));
        Assert.That(result.ColumnValues[0].FullValue.ToString(), Is.EqualTo("Alice"));
        Assert.That(result.ColumnValues[1].FullValue.ToString(), Is.EqualTo("30"));
        Assert.That(result.ColumnValues[2].FullValue.ToString(), Is.EqualTo("London"));
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Unit Tests")]
    public void SplitLine_BadCsvData_ReturnsSingleColumnInsteadOfCrash ()
    {
        // Arrange: columnizer with semicolon delimiter (default) but line uses commas with quoted fields
        // This triggers CsvHelper.BadDataException because quotes appear mid-field in semicolon mode
        CsvColumnizer.CsvColumnizer columnizer = new();
        _ = columnizer.PreProcessLine("header1;header2;header3".AsMemory(), 0, 0);

        var mockCallback = new Mock<ILogLineMemoryColumnizerCallback>();
        columnizer.Selected(mockCallback.Object);

        // Act: line with comma-separated data containing quoted fields — bad data for semicolon delimiter
        var badLine = new CsvLogLine("6,6774DC1dB00BD11,\"Farmer, Edwards and Andrade\",http://wolfe-boyd.com/,Norfolk Island,Virtual leadingedge benchmark,2003,Mental Health Care,3503", 1);
        var result = columnizer.SplitLine(null, badLine);

        // Assert: returns single-column fallback, no crash
        Assert.That(result.ColumnValues, Has.Length.EqualTo(1));
        Assert.That(result.ColumnValues[0].FullValue.ToString(), Does.Contain("6774DC1dB00BD11"));
    }

    [TestCase(@".\TestData\organizations-10000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.System)]
    [TestCase(@".\TestData\organizations-1000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.System)]
    [TestCase(@".\TestData\people-10000.csv", new[] { "Index", "User Id", "First Name", "Last Name", "Sex", "Email", "Phone", "Date of birth", "Job Title" }, ReaderType.System)]
    public void Instantiat_CSVFile_BuildCorrectColumnizer (string filename, string[] expectedHeaders, ReaderType readerType)
    {
        CsvColumnizer.CsvColumnizer csvColumnizer = new();
        var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, filename);
        LogfileReader reader = new(path, new EncodingOptions(), true, 40, 50, new MultiFileOptions(), readerType, PluginRegistry.PluginRegistry.Instance, 500);
        reader.ReadFiles();
        var line = reader.GetLogLineMemory(0);
        IColumnizedLogLineMemory logline = new ColumnizedLogLine();
        if (line != null)
        {
            logline = csvColumnizer.SplitLine(null, line);
        }

        var expectedResult = string.Join(",", expectedHeaders);
        Assert.That(logline.LogLine.FullLine.ToString(), Is.EqualTo(expectedResult));
    }
}