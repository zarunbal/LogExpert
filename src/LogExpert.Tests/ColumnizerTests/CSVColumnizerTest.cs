using System.Reflection;

using ColumnizerLib;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;

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

    [TestCase(@".\TestData\organizations-10000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.System)]
    [TestCase(@".\TestData\organizations-1000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.System)]
    [TestCase(@".\TestData\people-10000.csv", new[] { "Index", "User Id", "First Name", "Last Name", "Sex", "Email", "Phone", "Date of birth", "Job Title" }, ReaderType.System)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Unit Test")]
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