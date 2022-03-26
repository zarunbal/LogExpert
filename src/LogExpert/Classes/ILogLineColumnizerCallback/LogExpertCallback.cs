using System.Collections.Generic;

namespace LogExpert.Classes.ILogLineColumnizerCallback
{
    public class LogExpertCallback : ColumnizerCallback, ILogExpertCallback
    {
        #region cTor

        public LogExpertCallback(LogWindow logWindow)
            : base(logWindow)
        {
        }

        #endregion

        #region Public methods

        public void AddTempFileTab(string fileName, string title)
        {
            logWindow.AddTempFileTab(fileName, title);
        }

        public void AddPipedTab(IList<LineEntry> lineEntryList, string title)
        {
            logWindow.WritePipeTab(lineEntryList, title);
        }

        public string GetTabTitle()
        {
            return logWindow.Text;
        }

        #endregion
    }
}
