using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	[Serializable]
	public class MultifileOptions
	{
		private int maxDayTry = 3;
		private string formatPattern = "*$J(.)";

		public int MaxDayTry
		{
			get
			{
				return maxDayTry;
			}
			set
			{
				maxDayTry = value;
			}
		}

		public string FormatPattern
		{
			get
			{
				return formatPattern;
			}
			set
			{
				formatPattern = value;
			}
		}
	}
}