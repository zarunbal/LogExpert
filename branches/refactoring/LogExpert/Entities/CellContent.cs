using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class CellContent
	{
		public CellContent(string value, int x)
		{
			this.Value = value;
			this.CellPosX = x;
		}

		public string Value { get; set; }

		public int CellPosX { get; set; }
	}
}