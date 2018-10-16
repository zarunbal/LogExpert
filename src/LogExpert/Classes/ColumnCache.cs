namespace LogExpert
{
    internal class ColumnCache
    {
        #region Private Fields

        private IColumnizedLogLine cachedColumns;
        private ILogLineColumnizer lastColumnizer;
        private int lastLineNumber = -1;

        #endregion

        internal IColumnizedLogLine GetColumnsForLine(LogfileReader logFileReader, int lineNumber,
                                                      ILogLineColumnizer columnizer,
                                                      LogWindow.ColumnizerCallback columnizerCallback)
        {
            if (lastColumnizer != columnizer || lastLineNumber != lineNumber && cachedColumns != null ||
                columnizerCallback.LineNum != lineNumber)
            {
                lastColumnizer = columnizer;
                lastLineNumber = lineNumber;
                ILogLine line = logFileReader.GetLogLineWithWait(lineNumber);
                if (line != null)
                {
                    columnizerCallback.LineNum = lineNumber;
                    cachedColumns = columnizer.SplitLine(columnizerCallback, line);
                }
                else
                {
                    cachedColumns = null;
                }
            }

            return cachedColumns;
        }
    }
}
