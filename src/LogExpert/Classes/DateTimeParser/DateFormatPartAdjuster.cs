using System;
using System.Collections.Generic;

namespace LogExpert
{
    // Ensures we have constant width (number of characters) date formats
    internal static class DateFormatPartAdjuster
    {
        private static IDictionary<string, string> dateTimePartReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
            if (!dateTimePartReplacements.TryGetValue(part, out string adjustedPart))
                return part;

            if (char.IsUpper(part[0]))
                return adjustedPart.ToUpper();
            else
                return adjustedPart.ToLower();
        }

        public static IEnumerable<string> AdjustDateTimeFormatParts(params string[] parts)
        {
            foreach (var part in parts)
                yield return AdjustDateTimeFormatPart(part);
        }
    }
}