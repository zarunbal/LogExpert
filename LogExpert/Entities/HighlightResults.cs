using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class HighlightResults
	{
		private IList<HilightEntry> highlightEntryList = new List<HilightEntry>();

		public IList<HilightEntry> HighlightEntryList
		{
			get
			{
				return this.highlightEntryList;
			}
			set
			{
				this.highlightEntryList = value;
			}
		}
	}
}