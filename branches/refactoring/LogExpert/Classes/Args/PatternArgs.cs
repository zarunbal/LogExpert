using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public class PatternArgs
	{
		public int maxMisses = 5;
		public int maxDiffInBlock = 5;
		public int minWeight = 15;
		public int fuzzy = 6;
		public int startLine = 0;
		public int endLine = 0;
	}
}