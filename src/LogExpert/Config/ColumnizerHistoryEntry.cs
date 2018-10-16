using System;

namespace LogExpert
{
    [Serializable]
    public class ColumnizerHistoryEntry
    {
        #region Private Fields

        public string columnizerName;
        public string fileName;

        #endregion

        #region Ctor

        public ColumnizerHistoryEntry(string fileName, string columnizerName)
        {
            this.fileName = fileName;
            this.columnizerName = columnizerName;
        }

        #endregion
    }
}
