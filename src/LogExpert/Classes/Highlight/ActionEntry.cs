using System;

namespace LogExpert
{
    [Serializable]
    public class ActionEntry
    {
        #region Private Fields

        public string actionParam;
        public string pluginName;

        #endregion

        #region Public Methods

        public ActionEntry Copy()
        {
            ActionEntry e = new ActionEntry();
            e.pluginName = pluginName;
            e.actionParam = actionParam;
            return e;
        }

        #endregion
    }
}
