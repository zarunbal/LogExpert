using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace LogExpert
{
    public static class Extensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
            {
                return true;
            }

            if (!collection.Any())
            {
                return true;
            }

            return false;
        }

        public static bool IsEmpty<T>(this IList<T> list)
        {
            if (list == null)
            {
                return true;
            }

            if (list.Count == 0)
            {
                return true;
            }

            return false;
        }
    }
}