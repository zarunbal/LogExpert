using System;

namespace LogExpert
{
    [Serializable]
    public class ColumnizerHistoryEntry
    {
        #region Fields

        public string columnizerName;
        public string fileName;

        #endregion

        #region cTor

        public ColumnizerHistoryEntry(string fileName, string columnizerName)
        {
            this.fileName = fileName;
            this.columnizerName = columnizerName;
        }

        #endregion
    }
}