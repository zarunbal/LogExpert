using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;

using NUnit.Framework;

namespace LogExpert.Tests;

[TestFixture]
public class JsonColumnizerTest
{
    [TestCase(@".\TestData\JsonColumnizerTest_01.txt", "time @m level")]
    public void GetColumnNames_HappyFile_ColumnNameMatches (string fileName, string expectedHeaders)
    {
        var jsonColumnizer = new JsonColumnizer.JsonColumnizer();
        var path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, fileName);
        LogfileReader reader = new(path, new EncodingOptions(), true, 40, 50, new MultiFileOptions(), false, LogExpert.PluginRegistry.PluginRegistry.Instance);
        reader.ReadFiles();

        var line = reader.GetLogLine(0);
        if (line != null)
        {
            _ = jsonColumnizer.SplitLine(null, line);
        }

        line = reader.GetLogLine(1);
        if (line != null)
        {
            _ = jsonColumnizer.SplitLine(null, line);
        }

        var columnHeaders = jsonColumnizer.GetColumnNames();
        var result = string.Join(" ", columnHeaders);
        Assert.That(expectedHeaders, Is.EqualTo(result));
    }
}
