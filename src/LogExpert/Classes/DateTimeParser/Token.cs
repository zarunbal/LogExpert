using System;

namespace LogExpert
{
    internal static class Token
    {
        public static bool IsDatePart(string token)
        {
            return
                token.StartsWith("y", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("m", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("d", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("s", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("h", StringComparison.OrdinalIgnoreCase) ||
                string.Compare(token, "tt", StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}