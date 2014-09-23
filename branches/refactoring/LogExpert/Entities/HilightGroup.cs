using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	[Serializable]
	public class HilightGroup
	{
		private string groupName = "";
		private List<HilightEntry> hilightEntryList = new List<HilightEntry>();

		public string GroupName
		{
			get
			{
				return this.groupName;
			}
			set
			{
				this.groupName = value;
			}
		}

		public List<HilightEntry> HilightEntryList
		{
			get
			{
				return this.hilightEntryList;
			}
			set
			{
				this.hilightEntryList = value;
			}
		}
	}
}