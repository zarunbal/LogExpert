using LogExpert;

namespace CsvColumnizer
{

    public partial class CsvColumnizer
    {
        public class CsvLogLine : ILogLine
        {
            #region Properties

            public string FullLine { get; set; }

            public int LineNumber { get; set; }

            string ITextValue.Text => FullLine;

            #endregion
        }
    }
}