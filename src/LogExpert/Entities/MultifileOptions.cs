using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    [Serializable]
    public class MultifileOptions
    {
        #region Fields

        private string formatPattern = "*$J(.)";
        private int maxDayTry = 3;

        #endregion

        #region Properties

        public int MaxDayTry
        {
            get { return maxDayTry; }
            set { maxDayTry = value; }
        }

        public string FormatPattern
        {
            get { return formatPattern; }
            set { formatPattern = value; }
        }

        #endregion
    }
}