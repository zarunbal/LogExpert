using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	class DefaultLogfileColumnizer : ILogLineColumnizer
	{
		#region ILogLineColumnizer Members
		
		public string GetName()
		{
			return "Default (single line)";
		}
		
		public string GetDescription()
		{
			return "No column splitting. The whole line is displayed in a single column.";
		}
		
		public int GetColumnCount()
		{
			return 1;
		}
		
		public string[] GetColumnNames()
		{
			return new string[] { "Text" };
		}
		
		public string[] SplitLine(ILogLineColumnizerCallback callback, string line)
		{
			return new string[] { line };
		}
		
		public string Text
		{
			get
			{
				return GetName();
			}
		}
		
		#endregion
		
		#region ILogLineColumnizer Not implemented Members
		
		public bool IsTimeshiftImplemented()
		{
			return false;
		}
		
		public void SetTimeOffset(int msecOffset)
		{
			throw new NotImplementedException();
		}
		
		public int GetTimeOffset()
		{
			throw new NotImplementedException();
		}
		
		public DateTime GetTimestamp(ILogLineColumnizerCallback callback, string line)
		{
			throw new NotImplementedException();
		}
		
		public void PushValue(ILogLineColumnizerCallback callback, int column, string value, string oldValue)
		{
		}
	
		#endregion
	}
}