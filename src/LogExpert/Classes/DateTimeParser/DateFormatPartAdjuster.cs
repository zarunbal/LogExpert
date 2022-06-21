using System;
using System.Collections.Generic;

namespace LogExpert.Classes.DateTimeParser
{
    // Ensures we have constant width (number of characters) date formats
    internal static class DateFormatPartAdjuster
    {
        private static readonly IDictionary<string, string> _dateTimePartReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["y"] = "yyy",
            ["yyy"] = "yyyy",
            ["m"] = "mm",
            ["d"] = "dd",
            ["h"] = "hh",
            ["s"] = "ss"
        };

        public static string AdjustDateTimeFormatPart(string part)
        {
            if (!_dateTimePartReplacements.TryGetValue(part, out string adjustedPart))
            {
                return part;
            }

            if (char.IsUpper(part[0]))
            {
                return adjustedPart.ToUpper();
            }
            else
            {
                return adjustedPart.ToLower();
            }
        }
    }
}