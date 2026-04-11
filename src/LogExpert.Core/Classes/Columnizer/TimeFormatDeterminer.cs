using System.Globalization;

namespace LogExpert.Core.Classes.Columnizer;

internal class TimeFormatDeterminer
{
    #region FormatInfo helper class

    public class FormatInfo (string dateFormat, string timeFormat, CultureInfo cultureInfo)
    {

        #region Properties

        public string DateFormat { get; } = dateFormat;

        public string TimeFormat { get; } = timeFormat;

        public CultureInfo CultureInfo { get; } = cultureInfo;

        public string DateTimeFormat => DateFormat + " " + TimeFormat;

        public bool IgnoreFirstChar { get; set; }

        #endregion
    }

    #endregion

    private readonly FormatInfo formatInfo1 = new("dd.MM.yyyy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo2 = new("dd.MM.yyyy", "HH:mm:ss", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo3 = new("yyyy/MM/dd", "HH:mm:ss.fff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo4 = new("yyyy/MM/dd", "HH:mm:ss", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo5 = new("yyyy.MM.dd", "HH:mm:ss.fff", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo6 = new("yyyy.MM.dd", "HH:mm:ss", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo7 = new("dd.MM.yyyy", "HH:mm:ss,fff", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo8 = new("yyyy/MM/dd", "HH:mm:ss,fff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo9 = new("yyyy.MM.dd", "HH:mm:ss,fff", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo10 = new("yyyy-MM-dd", "HH:mm:ss.fff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo11 = new("yyyy-MM-dd", "HH:mm:ss,fff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo12 = new("yyyy-MM-dd", "HH:mm:ss", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo13 = new("dd MMM yyyy", "HH:mm:ss,fff", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo14 = new("dd MMM yyyy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo15 = new("dd MMM yyyy", "HH:mm:ss", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo16 = new("dd.MM.yy", "HH:mm:ss.fff", new CultureInfo("de-DE"));
    private readonly FormatInfo formatInfo17 = new("yyyy-MM-dd", "HH:mm:ss:ffff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo18 = new("dd/MM/yyyy", "HH:mm:ss.fff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo19 = new("dd/MM/yyyy", "HH:mm:ss:fff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo20 = new("yyyy-MM-dd", "HH:mm:ss.ffff", new CultureInfo("en-US"));
    private readonly FormatInfo formatInfo21 = new("yyyy-MM-dd", "HH:mm:ss,ffff", new CultureInfo("en-US"));

    /// <summary>
    /// Determines the date and time format information for the specified input line.
    /// </summary>
    /// <param name="line">The input string containing date and time data to analyze. Cannot be null.</param>
    /// <returns>A FormatInfo object that describes the detected date and time format of the input line.</returns>
    [Obsolete("Use DetermineDateTimeFormatInfo(ReadOnlySpan<char>) for better performance.")]
    public FormatInfo DetermineDateTimeFormatInfo (string line)
    {
        return DetermineDateTimeFormatInfo(line.AsSpan());
    }

    /// <summary>
    /// Determines the date and time format information for the specified character span.
    /// </summary>
    /// <remarks>This method inspects the structure of the input span to identify common date and time
    /// formats. It does not perform full parsing or validation of the date and time value. The method is optimized for
    /// performance and is suitable for scenarios where rapid format detection is required.</remarks>
    /// <param name="span">A read-only span of characters containing the date and time string to analyze. The span must be at least 21
    /// characters long.</param>
    /// <returns>A FormatInfo instance describing the detected date and time format, or null if the format could not be
    /// determined.</returns>
    public FormatInfo DetermineDateTimeFormatInfo (ReadOnlySpan<char> span)
    {
        if (span.Length < 21)
        {
            return null;
        }

        var ignoreFirst = false;

        // determine if string starts with bracket and remove it
        if (span[0] is '[' or '(' or '{')
        {
            span = span[1..];
            ignoreFirst = true;

        }

        // dirty hardcoded probing of date/time format (much faster than DateTime.ParseExact()
        if (span[2] == '.' && span[5] == '.' && span[13] == ':' && span[16] == ':')
        {
            if (span[19] == '.')
            {
                formatInfo1.IgnoreFirstChar = ignoreFirst;
                return formatInfo1;
            }
            else if (span[19] == ',')
            {
                formatInfo7.IgnoreFirstChar = ignoreFirst;
                return formatInfo7;
            }
            else
            {
                formatInfo2.IgnoreFirstChar = ignoreFirst;
                return formatInfo2;
            }
        }
        else if (span[2] == '/' && span[5] == '/' && span[13] == ':' && span[16] == ':')
        {
            if (span[19] == '.')
            {
                formatInfo18.IgnoreFirstChar = ignoreFirst;
                return formatInfo18;
            }
            else if (span[19] == ':')
            {
                formatInfo19.IgnoreFirstChar = ignoreFirst;
                return formatInfo19;
            }
        }
        else if (span[4] == '/' && span[7] == '/' && span[13] == ':' && span[16] == ':')
        {
            if (span[19] == '.')
            {
                formatInfo3.IgnoreFirstChar = ignoreFirst;
                return formatInfo3;
            }
            else if (span[19] == ',')
            {
                formatInfo8.IgnoreFirstChar = ignoreFirst;
                return formatInfo8;
            }
            else
            {
                formatInfo4.IgnoreFirstChar = ignoreFirst;
                return formatInfo4;
            }
        }
        else if (span[4] == '.' && span[7] == '.' && span[13] == ':' && span[16] == ':')
        {
            if (span[19] == '.')
            {
                formatInfo5.IgnoreFirstChar = ignoreFirst;
                return formatInfo5;
            }
            else if (span[19] == ',')
            {
                formatInfo9.IgnoreFirstChar = ignoreFirst;
                return formatInfo9;
            }
            else
            {
                formatInfo6.IgnoreFirstChar = ignoreFirst;
                return formatInfo6;
            }
        }
        else if (span[4] == '-' && span[7] == '-' && span[13] == ':' && span[16] == ':')
        {
            if (span[19] == '.')
            {
                if (span.Length > 23 && char.IsDigit(span[23]))
                {
                    formatInfo20.IgnoreFirstChar = ignoreFirst;
                    return formatInfo20;
                }
                else
                {
                    formatInfo10.IgnoreFirstChar = ignoreFirst;
                    return formatInfo10;
                }
            }
            else if (span[19] == ',')
            {
                if (span.Length > 23 && char.IsDigit(span[23]))
                {
                    formatInfo21.IgnoreFirstChar = ignoreFirst;
                    return formatInfo21;
                }
                else
                {
                    formatInfo11.IgnoreFirstChar = ignoreFirst;
                    return formatInfo11;
                }
            }
            else if (span[19] == ':')
            {
                formatInfo17.IgnoreFirstChar = ignoreFirst;
                return formatInfo17;
            }
            else
            {
                formatInfo12.IgnoreFirstChar = ignoreFirst;
                return formatInfo12;
            }
        }
        else if (span[2] == ' ' && span[6] == ' ' && span[14] == ':' && span[17] == ':')
        {
            if (span[20] == ',')
            {
                formatInfo13.IgnoreFirstChar = ignoreFirst;
                return formatInfo13;
            }
            else if (span[20] == '.')
            {
                formatInfo14.IgnoreFirstChar = ignoreFirst;
                return formatInfo14;
            }
            else
            {
                formatInfo15.IgnoreFirstChar = ignoreFirst;
                return formatInfo15;
            }
        }
        //dd.MM.yy HH:mm:ss.fff
        else if (span[2] == '.' && span[5] == '.' && span[11] == ':' && span[14] == ':' && span[17] == '.')
        {
            formatInfo16.IgnoreFirstChar = ignoreFirst;
            return formatInfo16;
        }

        return null;
    }

    /// <summary>
    /// Determines the time format information for the specified field name.
    /// </summary>
    /// <param name="field">The name of the field for which to retrieve time format information. Cannot be null.</param>
    /// <returns>A FormatInfo object containing details about the time format for the specified field.</returns>
    [Obsolete("Use DetermineTimeFormatInfo(ReadOnlySpan<char>) for better performance.")]
    public FormatInfo DetermineTimeFormatInfo (string field)
    {
        return DetermineTimeFormatInfo(field.AsSpan());
    }

    /// <summary>
    /// Determines the appropriate time format information for the specified character span representing a time value.
    /// </summary>
    /// <remarks>This method performs a fast, heuristic analysis of the input span to identify common time
    /// formats. It does not perform full validation or parsing of the time value. For unsupported or unrecognized
    /// formats, the method returns null.</remarks>
    /// <param name="span">A read-only span of characters containing the time value to analyze. The span is expected to be in a supported
    /// time format.</param>
    /// <returns>A FormatInfo instance describing the detected time format, or null if the format is not recognized.</returns>
    public FormatInfo DetermineTimeFormatInfo (ReadOnlySpan<char> span)
    {
        // dirty hardcoded probing of time format (much faster than DateTime.ParseExact()
        if (span[2] == ':' && span[5] == ':')
        {
            if (span.Length > 8)
            {
                if (span[8] == '.')
                {
                    return formatInfo1;
                }
                else if (span[8] == ',')
                {
                    return formatInfo7;
                }
            }
            else
            {
                return formatInfo2;
            }
        }

        return null;
    }
}