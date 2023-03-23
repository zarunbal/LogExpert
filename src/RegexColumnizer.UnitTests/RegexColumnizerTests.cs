using LogExpert;
using Moq;
using NUnit.Framework;

namespace RegexColumnizer.UnitTests
{
    [TestFixture]
    public class RegexColumnizerTests
    {

        // The same amount of columns should be returned whether the line matches the regex or not.
        [TestCase("5 test message", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 2)]
        [TestCase("Error in com.example.core", @"^(?'time'[\d]+)\s+(?'Message'.+)$", 2)]
        public void SplitLine_ColumnCountMatches(string lineToParse, string regex, int expectedNumberOfColumns)
        {
            var columnizer = CreateInitializedColumnizer(regex);
            
            var testLogLine = new TestLogLine(4, lineToParse);
            var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);
            
            Assert.AreEqual(expectedNumberOfColumns, parsedLogLine.ColumnValues.Length);
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

            var testLogLine = new TestLogLine(3, lineToParse);
            var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), testLogLine);
            
            Assert.AreEqual(expectedColumnValue, parsedLogLine.ColumnValues[columnIndexToTest].Text);
        }
        
        private Regex1Columnizer CreateInitializedColumnizer(string regex)
        {
            var columnizerConfig = new RegexColumnizerConfig
            {
                Expression =  regex,
                Name = "Test regex"
            };
            var columnizer = new Regex1Columnizer();
            columnizer.Init(columnizerConfig);
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
    
    
}