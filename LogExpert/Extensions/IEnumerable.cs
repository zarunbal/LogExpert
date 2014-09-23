using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
	public static partial class Extensions
	{
		public static int GetNextIndex<T>(this ICollection<T> list, int currentIndex, bool isForward)
		{
			bool temp = false;
			return GetNextIndex(list, currentIndex, isForward, out temp);
		}

		public static int GetNextIndex<T>(this ICollection<T> list, int currentIndex, bool isForward, out bool wrapped)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return GetNextIndex(list.Count, currentIndex, isForward, out wrapped);
		}

		public static int GetNextIndex(this int listCount, int currentIndex, bool isForward)
		{
			bool wrapped;

			return GetNextIndex(listCount, currentIndex, isForward, out wrapped);
		}

		public static int GetNextIndex(this int listCount, int currentIndex, bool isForward, out bool wrapped)
		{
			wrapped = false;

			//TODO should we throw an exception?
			if (listCount == 0)
			{
				return 0;
			}

			if (isForward)
			{
				currentIndex++;
			}
			else
			{
				currentIndex--;
			}

			currentIndex = SanitizeIndex(listCount, currentIndex, out wrapped);

			return currentIndex;
		}

		public static int SanitizeIndex(this int listCount, int currentIndex)
		{
			bool temp = false;
			return SanitizeIndex(listCount, currentIndex, out temp);
		}

		public static int SanitizeIndex(this int listCount, int currentIndex, out bool wrapped)
		{
			wrapped = false;
			if (currentIndex >= listCount)
			{
				//Overflow jump to first entry
				currentIndex = 0;
				wrapped = true;
			}
			else if (currentIndex < 0)
			{
				//Underflow jump to last entry;
				currentIndex = listCount - 1;
				wrapped = true;
			}

			return currentIndex;
		}
	}
}