using System.Collections.Generic;

namespace LogExpert
{
    /// <summary>
    ///     Methods to control the LogWindow from other views.
    /// </summary>
    public interface ILogView
    {
        #region Properties / Indexers

        ILogLineColumnizer CurrentColumnizer { get; }
        string FileName { get; }

        #endregion

        #region Public Methods

        void DeleteBookmarks(List<int> lineNumList);
        void RefreshLogView();
        void SelectAndEnsureVisible(int line, bool triggerSyncCall);

        void SelectLogLine(int lineNumber);

        #endregion
    }
}
