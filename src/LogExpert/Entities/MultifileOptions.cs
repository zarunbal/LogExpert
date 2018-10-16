using System;

namespace LogExpert
{
    [Serializable]
    public class MultifileOptions
    {
        #region Private Fields

        private string formatPattern = "*$J(.)";
        private int maxDayTry = 3;

        #endregion

        #region Properties / Indexers

        public string FormatPattern
        {
            get => formatPattern;
            set => formatPattern = value;
        }

        public int MaxDayTry
        {
            get => maxDayTry;
            set => maxDayTry = value;
        }

        #endregion
    }
}
