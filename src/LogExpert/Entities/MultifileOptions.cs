using System;

namespace LogExpert.Entities
{
    [Serializable]
    public class MultiFileOptions
    {
        #region Fields

        private string _formatPattern = "*$J(.)";
        private int _maxDayTry = 3;

        #endregion

        #region Properties

        public int MaxDayTry
        {
            get => _maxDayTry;
            set => _maxDayTry = value;
        }

        public string FormatPattern
        {
            get => _formatPattern;
            set => _formatPattern = value;
        }

        #endregion
    }
}