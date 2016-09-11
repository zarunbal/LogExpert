using System;

namespace LogExpert
{
	/// <summary>
	/// Class for storing word-wise hilight matches. Used for colouring different matches on one line.
	/// </summary>
	public class HilightMatchEntry
	{
		public HilightEntry HilightEntry { get; set; }

		public int StartPos { get; set; }

		public int Length { get; set; }

		public override String ToString()
		{
			return string.Format("{0}/{1}/{2}", HilightEntry.SearchText, StartPos, Length);
		}
	}
}