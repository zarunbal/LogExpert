using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class PatternBlock
	{
		#region cTor

		public PatternBlock()
		{
			QualityInfoList = new Dictionary<int, QualityInfo>();
			SrcLines = new SortedDictionary<int, int>();
			TargetLines = new SortedDictionary<int, int>();
		}

		#endregion cTor

		#region Properties

		public Dictionary<int, QualityInfo> QualityInfoList { get; set; }

		public int StartLine { get; set; }

		public int EndLine { get; set; }

		public int TargetStart { get; set; }

		public int TargetEnd { get; set; }

		public int Weigth { get; set; }

		public int BlockId { get; set; }

		public SortedDictionary<int, int> SrcLines { get; set; }

		public SortedDictionary<int, int> TargetLines { get; set; }

		#endregion Properties

		#region Overrides

		public override string ToString()
		{
			return string.Format("srcStart={0}, srcEnd={1}, targetStart={2}, targetEnd={3}, weight={4}", StartLine, EndLine, TargetStart, TargetEnd, Weigth);
		}

		#endregion Overrides
	}
}