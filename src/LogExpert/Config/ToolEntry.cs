using System;
using LogExpert.Classes;

namespace LogExpert.Config
{
    [Serializable]
    public class ToolEntry
    {
        #region Fields

        public string args = "";
        public string cmd = "";
        public string columnizerName = "";
        public string iconFile;
        public int iconIndex;
        public bool isFavourite;
        public string name;
        public bool sysout = false;
        public string workingDir = "";

        #endregion

        #region Public methods

        public override string ToString()
        {
            return Util.IsNull(this.name) ? this.cmd : this.name;
        }

        public ToolEntry Clone()
        {
            ToolEntry clone = new ToolEntry();
            clone.cmd = this.cmd;
            clone.args = this.args;
            clone.name = this.name;
            clone.sysout = this.sysout;
            clone.columnizerName = this.columnizerName;
            clone.isFavourite = this.isFavourite;
            clone.iconFile = this.iconFile;
            clone.iconIndex = this.iconIndex;
            clone.workingDir = this.workingDir;
            return clone;
        }

        #endregion
    }
}