using System;
using System.Drawing;

namespace LogExpert
{
    [Serializable]
    public class ColorEntry
    {
        #region Private Fields

        public Color color;
        public string fileName;

        #endregion

        #region Ctor

        public ColorEntry(string fileName, Color color)
        {
            this.fileName = fileName;
            this.color = color;
        }

        #endregion
    }
}
