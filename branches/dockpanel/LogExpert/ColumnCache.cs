using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  internal class ColumnCache
  {
    private string[] cachedColumns = null;
    private ILogLineColumnizer lastColumnizer;
    private int lastLineNumber = -1;

    internal ColumnCache()
    {

    }

    internal string[] GetColumnsForLine(LogfileReader logFileReader, int lineNumber, ILogLineColumnizer columnizer, LogExpert.LogWindow.ColumnizerCallback columnizerCallback)
    {
      if (this.lastColumnizer != columnizer || this.lastLineNumber != lineNumber || this.cachedColumns == null)
      {
        this.lastColumnizer = columnizer;
        this.lastLineNumber = lineNumber;
        string line = logFileReader.GetLogLineWithWait(lineNumber);
        if (line != null)
        {
          columnizerCallback.LineNum = lineNumber;
          this.cachedColumns = columnizer.SplitLine(columnizerCallback, line);
        }
        else
        {
          this.cachedColumns = null;
        }
      }
      return this.cachedColumns;
    }
  }
}
