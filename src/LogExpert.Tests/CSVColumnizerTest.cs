using ColumnizerLib;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;

using NUnit.Framework;

namespace LogExpert.Tests;

[TestFixture]
public class CSVColumnizerTest
{
    [TestCase(@".\TestData\organizations-10000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.Pipeline)]
    [TestCase(@".\TestData\organizations-1000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.Pipeline)]
    [TestCase(@".\TestData\people-10000.csv", new[] { "Index", "User Id", "First Name", "Last Name", "Sex", "Email", "Phone", "Date of birth", "Job Title" }, ReaderType.Pipeline)]
    [TestCase(@".\TestData\organizations-10000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.System)]
    [TestCase(@".\TestData\organizations-1000.csv", new[] { "Index", "Organization Id", "Name", "Website", "Country", "Description", "Founded", "Industry", "Number of employees" }, ReaderType.System)]
    [TestCase(@".\TestData\people-10000.csv", new[] { "Index", "User Id", "First Name", "Last Name", "Sex", "Email", "Phone", "Date of birth", "Job Title" }, ReaderType.System)]
    public void Instantiat_CSVFile_BuildCorrectColumnizer (string filename, string[] expectedHeaders, ReaderType readerType)
    {
        CsvColumnizer.CsvColumnizer csvColumnizer = new();
        var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, filename);
        LogfileReader reader = new(path, new EncodingOptions(), true, 40, 50, new MultiFileOptions(), readerType, PluginRegistry.PluginRegistry.Instance, 500);
        reader.ReadFiles();
        var line = reader.GetLogLine(0);
        IColumnizedLogLineMemory logline = new ColumnizedLogLine();
        if (line != null)
        {
            logline = csvColumnizer.SplitLine(null, line);
        }

        var expectedResult = string.Join(",", expectedHeaders);
        Assert.That(logline.LogLine.FullLine, Is.EqualTo(expectedResult));
    }
}
