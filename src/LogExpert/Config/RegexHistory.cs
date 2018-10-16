using System;
using System.Collections.Generic;

namespace LogExpert
{
    [Serializable]
    public class RegexHistory
    {
        #region Private Fields

        public List<string> expressionHistoryList = new List<string>();
        public List<string> testtextHistoryList = new List<string>();

        #endregion
    }
}
