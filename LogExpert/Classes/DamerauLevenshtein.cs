using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogExpert.Classes
{
	public class DamerauLevenshtein
	{
		public static bool TestFilterCondition(FilterParams filterParams, string line, ILogLineColumnizerCallback columnizerCallback)
		{
			if (filterParams.lastLine.Equals(line))
				return filterParams.lastResult;

			bool match = TestFilterMatch(filterParams, line, columnizerCallback);
			filterParams.lastLine = line;

			if (filterParams.isRangeSearch)
			{
				if (!filterParams.isInRange)
				{
					if (match)
					{
						filterParams.isInRange = true;
					}
				}
				else
				{
					if (!match)
					{
						match = true;
					}
					else
					{
						filterParams.isInRange = false;
					}
				}
			}
			if (filterParams.isInvert)
				match = !match;
			filterParams.lastResult = match;
			return match;
		}

		private static bool TestFilterMatch(FilterParams filterParams, string line, ILogLineColumnizerCallback columnizerCallback)
		{
			string lowerSearchText;
			string searchText;
			Regex rex;
			if (filterParams.isInRange)
			{
				lowerSearchText = filterParams.lowerRangeSearchText;
				searchText = filterParams.rangeSearchText;
				rex = filterParams.rangeRex;
			}
			else
			{
				lowerSearchText = filterParams.lowerSearchText;
				searchText = filterParams.searchText;
				rex = filterParams.rex;
			}

			if (searchText == null || lowerSearchText == null || searchText.Length == 0)
				return false;

			if (filterParams.columnRestrict)
			{
				string[] columns = filterParams.currentColumnizer.SplitLine(columnizerCallback, line);
				bool found = false;
				foreach (int colIndex in filterParams.columnList)
				{
					if (colIndex < columns.Length) // just to be sure, maybe the columnizer has changed anyhow
					{
						if (columns[colIndex].Trim().Length == 0)
						{
							if (filterParams.emptyColumnUsePrev)
							{
								string prevValue = (string)filterParams.lastNonEmptyCols[colIndex];
								if (prevValue != null)
								{
									if (TestMatchSub(filterParams, prevValue, lowerSearchText, searchText, rex, filterParams.exactColumnMatch))
										found = true;
								}
							}
							else if (filterParams.emptyColumnHit)
							{
								return true;
							}
						}
						else
						{
							filterParams.lastNonEmptyCols[colIndex] = columns[colIndex];
							if (TestMatchSub(filterParams, columns[colIndex], lowerSearchText, searchText, rex, filterParams.exactColumnMatch))
								found = true;
						}
					}
				}
				return found;
			}
			else
			{
				return TestMatchSub(filterParams, line, lowerSearchText, searchText, rex, false);
			}
		}

		private static bool TestMatchSub(FilterParams filterParams, string line, string lowerSearchText, string searchText, Regex rex, bool exactMatch)
		{
			if (filterParams.isRegex)
			{
				if (rex.IsMatch(line))
				{
					return true;
				}
			}
			else
			{
				if (!filterParams.isCaseSensitive)
				{
					if (exactMatch)
					{
						if (line.ToLower().Trim().Equals(lowerSearchText))
						{
							return true;
						}
					}
					else
					{
						if (line.ToLower().Contains(lowerSearchText))
						{
							return true;
						}
					}
				}
				else
				{
					if (exactMatch)
					{
						if (line.Equals(searchText))
						{
							return true;
						}
					}
					else
					{
						if (line.Contains(searchText))
						{
							return true;
						}
					}
				}

				if (filterParams.fuzzyValue > 0)
				{
					int range = line.Length - searchText.Length;
					if (range > 0)
					{
						for (int i = 0; i < range; ++i)
						{
							string src = line.Substring(i, searchText.Length);
							if (!filterParams.isCaseSensitive)
								src = src.ToLower();
							string dest = filterParams.isCaseSensitive ? searchText : lowerSearchText;
							int dist = Distance(src, searchText);
							if ((float)(searchText.Length + 1) / (float)(dist + 1) >= 11F / (float)(filterParams.fuzzyValue + 1F))
							{
								return true;
							}
						}
					}
					return false;
				}
			}
			return false;
		}

		private static int Distance(string src, string dest)
		{
			int[,] d = new int[src.Length + 1, dest.Length + 1];
			int i, j, cost;
			char[] str1 = src.ToCharArray();
			char[] str2 = dest.ToCharArray();

			for (i = 0; i <= str1.Length; i++)
			{
				d[i, 0] = i;
			}
			for (j = 0; j <= str2.Length; j++)
			{
				d[0, j] = j;
			}
			for (i = 1; i <= str1.Length; i++)
			{
				for (j = 1; j <= str2.Length; j++)
				{
					if (str1[i - 1] == str2[j - 1])
						cost = 0;
					else
						cost = 1;

					d[i, j] =
						Math.Min(d[i - 1, j] + 1, // Deletion
							Math.Min(d[i, j - 1] + 1, // Insertion
								d[i - 1, j - 1] + cost));     // Substitution

					if ((i > 1) && (j > 1) && (str1[i - 1] == str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
					{
						d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
					}
				}
			}
			return d[str1.Length, str2.Length];
		}
	}
}
