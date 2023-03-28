using LogExpert;

namespace CsvColumnizer
{
    public class CsvLogLine : ILogLine
    {
        public CsvLogLine(string fullLine, int lineNumber)
        {
            FullLine = fullLine;
            LineNumber = lineNumber;
        }

        #region Properties

        public string FullLine { get; set; }

        public int LineNumber { get; set; }

        string ITextValue.Text => FullLine;

        #endregion
    }

}