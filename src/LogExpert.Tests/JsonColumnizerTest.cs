using NUnit.Framework;
using System;
using System.IO;

namespace LogExpert.Tests
{
    [TestFixture]
    public class JsonColumnizerTest
    {
        [TestCase(@".\TestData\JsonColumnizerTest_01.txt", "time @m level")]
        public void GetColumnNames_HappyFile_ColumnNameMatches(string fileName, string expectedHeaders)
        {
            var jsonColumnizer = new JsonColumnizer.JsonColumnizer();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            LogfileReader reader = new LogfileReader(path, new EncodingOptions(), true, 40, 50, new MultifileOptions());
            reader.ReadFiles();

            ILogLine line = reader.GetLogLineWithWait(0);
            if (line != null)
            {
                jsonColumnizer.SplitLine(null, line);
            }

            line = reader.GetLogLineWithWait(1);
            if (line != null)
            {
                jsonColumnizer.SplitLine(null, line);
            }

            var columnHeaders = jsonColumnizer.GetColumnNames();
            var result = string.Join(" ", columnHeaders);
            Assert.AreEqual(result, expectedHeaders);
        }
    }
}
