using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    public class CellContent
    {
        #region Fields

        #endregion

        #region cTor

        public CellContent(string value, int x)
        {
            this.Value = value;
            this.CellPosX = x;
        }

        #endregion

        #region Properties

        public string Value { get; set; }

        public int CellPosX { get; set; }

        #endregion
    }
}