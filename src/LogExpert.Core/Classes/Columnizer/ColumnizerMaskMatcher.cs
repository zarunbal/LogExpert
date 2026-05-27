using System.Text;
using System.Text.RegularExpressions;

using LogExpert.Core.Config;

namespace LogExpert.Core.Classes.Columnizer;

/// <summary>
/// Pure mask-matching for <see cref="ColumnizerMaskEntry"/>. Supports glob (<c>*</c>, <c>?</c>) and
/// .NET regular expression patterns. Match is case-insensitive (file names on Windows are case-insensitive).
/// </summary>
/// <remarks>
/// The matcher never throws — malformed input returns <see langword="false"/>. Glob translation rules:
/// <list type="bullet">
///   <item><c>*</c> → <c>.*</c></item>
///   <item><c>?</c> → <c>.</c> (single character)</item>
///   <item>Every other character is regex-escaped</item>
///   <item>Result is anchored with <c>^…$</c></item>
/// </list>
/// </remarks>
public static class ColumnizerMaskMatcher
{
    private static readonly RegexOptions _options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    public static bool Matches (ColumnizerMaskEntry entry, string fileName)
    {
        if (entry == null || string.IsNullOrEmpty(entry.Mask) || string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        var pattern = entry.Type == MaskType.Glob
            ? GlobToRegex(entry.Mask)
            : entry.Mask;

        try
        {
            return Regex.IsMatch(fileName, pattern, _options);
        }
        catch (ArgumentException)
        {
            // Malformed regex (user-supplied) — treat as non-match rather than throwing.
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static string GlobToRegex (string glob)
    {
        var sb = new StringBuilder(glob.Length + 4);
        _ = sb.Append('^');

        foreach (var ch in glob)
        {
            switch (ch)
            {
                case '*':
                    _ = sb.Append(".*");
                    break;
                case '?':
                    _ = sb.Append('.');
                    break;
                default:
                    _ = sb.Append(Regex.Escape(ch.ToString()));
                    break;
            }
        }

        _ = sb.Append('$');
        return sb.ToString();
    }
}
