using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert.Dialogs
{
	public class SelectLineEventArgs : EventArgs
	{
		public int Line { get; private set; }

		public SelectLineEventArgs(int line)
		{
			this.Line = line;
		}
	}
}