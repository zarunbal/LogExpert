using System;

namespace LogExpert
{
    [Serializable]
    public class ToolEntry
    {
        #region Private Fields

        public string args = string.Empty;
        public string cmd = string.Empty;
        public string columnizerName = string.Empty;
        public string iconFile;
        public int iconIndex;
        public bool isFavourite;
        public string name;
        public bool sysout;
        public string workingDir = string.Empty;

        #endregion

        #region Public Methods

        public ToolEntry Clone()
        {
            ToolEntry clone = new ToolEntry();
            clone.cmd = cmd;
            clone.args = args;
            clone.name = name;
            clone.sysout = sysout;
            clone.columnizerName = columnizerName;
            clone.isFavourite = isFavourite;
            clone.iconFile = iconFile;
            clone.iconIndex = iconIndex;
            clone.workingDir = workingDir;
            return clone;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Util.IsNull(name) ? cmd : name;
        }

        #endregion
    }
}
