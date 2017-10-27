using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    /// <summary>
    /// Represents a log file and its window. Used as a kind of handle for menus or list of open files.
    /// </summary>
    public class WindowFileEntry
    {
        #region Fields

        private const int MAX_LEN = 40;

        #endregion

        #region cTor

        public WindowFileEntry(LogWindow logWindow)
        {
            this.LogWindow = logWindow;
        }

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                string title = this.LogWindow.Text;
                if (title.Length > MAX_LEN)
                {
                    title = "..." + title.Substring(title.Length - MAX_LEN);
                }
                return title;
            }
        }

        public string FileName
        {
            get { return this.LogWindow.FileName; }
        }


        public LogWindow LogWindow { get; }

        #endregion
    }
}