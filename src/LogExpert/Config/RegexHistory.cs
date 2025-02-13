using System;
using System.Collections.Generic;

namespace LogExpert.Config
{
    [Serializable]
    public class RegexHistory
    {
        #region Fields

        public List<string> expressionHistoryList = [];
        public List<string> testtextHistoryList = [];

        #endregion
    }
}