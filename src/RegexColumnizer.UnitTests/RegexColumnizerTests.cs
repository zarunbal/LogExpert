using LogExpert;
using Moq;
using NUnit.Framework;

namespace RegexColumnizer.UnitTests
{
    [TestFixture]
    public class RegexColumnizerTests
    {
        [Test]
        public void SplitLineLogRegexDoesNotMatchLine()
        {
            var columnizerConfig = new RegexColumnizerConfig
            {
                //We just need a regex that matches the line.
                Expression = @"^(?'time'[\d]+)\s+(?'Message'.+)$",
                Name = "Test regex"
            };

            var columnizer = new Regex1Columnizer();
            columnizer.Init(columnizerConfig);
            
            var notMatchingLogLine = new TestLogLine(5, "Error in com.example.core");
            
            var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), notMatchingLogLine);
            
            // verify that the expected number of columns are created
            Assert.AreEqual(2, parsedLogLine.ColumnValues.Length);

            //verify that the entire text is in the last column
            Assert.AreEqual("Error in com.example.core", parsedLogLine.ColumnValues[1].Text);
            
            // Verify that the other columns are empty strings
            Assert.AreEqual(string.Empty, parsedLogLine.ColumnValues[0].Text);
        }

        [Test]
        public void SplitLineRegexMatchesLine()
        {
            var columnizerConfig = new RegexColumnizerConfig
            {
                //We just need a regex that matches the line.
                Expression = @"^(?'time'[\d]+)\s+(?'Message'.+)$",
                Name = "Test regex"
            };

            var columnizer = new Regex1Columnizer();
            columnizer.Init(columnizerConfig);
            
            var notMatchingLogLine = new TestLogLine(5, "5 test message");
            
            
            var parsedLogLine = columnizer.SplitLine(Mock.Of<ILogLineColumnizerCallback>(), notMatchingLogLine);
            
            // verify that the expected number of columns are created
            Assert.AreEqual(2, parsedLogLine.ColumnValues.Length);

            //verify that the correct data is set to the correct columns
            Assert.AreEqual("5", parsedLogLine.ColumnValues[0].Text);
            Assert.AreEqual("test message", parsedLogLine.ColumnValues[1].Text);
            
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