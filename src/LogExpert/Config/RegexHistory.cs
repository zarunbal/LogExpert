using System;
using System.Collections.Generic;

namespace LogExpert.Config
{
    [Serializable]
    public class RegexHistory
    {
        #region Fields

        public List<string> expressionHistoryList = new List<string>();
        public List<string> testtextHistoryList = new List<string>();

        #endregion
    }
}