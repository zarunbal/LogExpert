using System;

namespace LogExpert
{
	[Serializable]
	public class ActionEntry
	{
		public string pluginName;
		public string actionParam;

		public ActionEntry Copy()
		{
			ActionEntry e = new ActionEntry();
			e.pluginName = pluginName;
			e.actionParam = actionParam;
			return e;
		}
	}
}