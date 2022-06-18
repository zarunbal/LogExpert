using System.Collections.Generic;

namespace LogExpert.Classes.ILogLineColumnizerCallback
{
    public class ColumnizerCallback : LogExpert.ILogLineColumnizerCallback, IAutoLogLineColumnizerCallback
    {
        #region Fields

        protected LogWindow logWindow;

        #endregion

        #region cTor

        public ColumnizerCallback(LogWindow logWindow)
        {
            this.logWindow = logWindow;
        }

        private ColumnizerCallback(ColumnizerCallback original)
        {
            logWindow = original.logWindow;
            LineNum = original.LineNum;
        }

        #endregion

        #region Properties

        public int LineNum { get; set; }

        #endregion

        #region Public methods

        public ColumnizerCallback CreateCopy()
        {
            return new ColumnizerCallback(this);
        }

        public int GetLineNum()
        {
            return LineNum;
        }

        public string GetFileName()
        {
            return logWindow.GetCurrentFileName(LineNum);
        }

        public ILogLine GetLogLine(int lineNum)
        {
            return logWindow.GetLine(lineNum);
        }

        public IList<ILogLineColumnizer> GetRegisteredColumnizers()
        {
            return PluginRegistry.GetInstance().RegisteredColumnizers;
        }

        public int GetLineCount()
        {
            return logWindow._logFileReader.LineCount;
        }

        #endregion
    }
}
