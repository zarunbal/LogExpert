using System.Globalization;

using ColumnizerLib;

namespace GlassfishColumnizer;

internal class GlassfishColumnizer : ILogLineMemoryXmlColumnizer
{
    #region Fields

    public const int COLUMN_COUNT = 2;
    private const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzzz";
    private const string DATETIME_FORMAT_OUT = "yyyy-MM-dd HH:mm:ss.fff";
    private const char SEPARATOR_CHAR = '|';

    private static readonly XmlConfig _xmlConfig = new();

    private readonly char[] trimChars = ['|'];
    private readonly CultureInfo cultureInfo = new("en-US");
    private int timeOffset;

    #endregion

    #region cTor

    public GlassfishColumnizer ()
    {
    }

    #endregion

    #region Public methods

    public IXmlLogConfiguration GetXmlLogConfiguration ()
    {
        return _xmlConfig;
    }

    public ILogLine GetLineTextForClipboard (ILogLine logLine, ILogLineColumnizerCallback callback)
    {
        return GetLineTextForClipboard(logLine, callback);
    }

    public string GetName ()
    {
        return "Glassfish";
    }

    public string GetCustomName ()
    {
        return GetName();
    }

    public string GetDescription ()
    {
        return "Parse the timestamps in Glassfish logfiles.";
    }

    public int GetColumnCount ()
    {
        return COLUMN_COUNT;
    }

    public string[] GetColumnNames ()
    {
        return ["Date/Time", "Message"];
    }

    public ILogLineMemory GetLineTextForClipboard (ILogLineMemory logLine, ILogLineMemoryColumnizerCallback callback)
    {
        return new GlassFishLogLine(ReplaceInMemory(logLine.FullLine, SEPARATOR_CHAR, '|'), logLine.Text, logLine.LineNumber);
    }

    public IColumnizedLogLine SplitLine (ILogLineColumnizerCallback callback, ILogLine line)
    {
        return SplitLine(callback as ILogLineMemoryColumnizerCallback, line as ILogLineMemory);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Intentionally passed")]
    public IColumnizedLogLineMemory SplitLine (ILogLineMemoryColumnizerCallback callback, ILogLineMemory line)
    {
        ColumnizedLogLine cLogLine = new()
        {
            LogLine = line
        };

        var temp = line.FullLine;
        var span = temp.Span;

        var columns = Column.CreateColumns(COLUMN_COUNT, cLogLine);
        cLogLine.ColumnValues = [.. columns.Select(a => a as IColumnMemory)];

        // delete '[#|' and '|#]'
        if (span.StartsWith("[#|", StringComparison.OrdinalIgnoreCase))
        {
            span = span[3..];
        }

        if (span.EndsWith("|#]", StringComparison.OrdinalIgnoreCase))
        {
            span = span[..^3];
        }

        // If the line is too short (i.e. does not follow the format for this columnizer) return the whole line content
        // in colum 8 (the log message column). Date and time column will be left blank.
        if (temp.Length < 28)
        {
            columns[1].FullValue = temp;
        }
        else
        {
            try
            {
                var dateTime = GetTimestamp(callback, line);
                if (dateTime == DateTime.MinValue)
                {
                    columns[1].FullValue = temp;
                }

                var newDate = dateTime.ToString(DATETIME_FORMAT_OUT, CultureInfo.InvariantCulture);
                columns[0].FullValue = newDate.AsMemory();
            }
            catch (Exception ex) when (ex is ArgumentException or
                                             FormatException or
                                             ArgumentOutOfRangeException)
            {
                columns[0].FullValue = "n/a".AsMemory();
            }

            var timestmp = columns[0];

            ReadOnlyMemory<char>[] cols;
            cols = SplitIntoTwo(temp, SEPARATOR_CHAR);

            if (cols.Length != COLUMN_COUNT)
            {
                columns[0].FullValue = ReadOnlyMemory<char>.Empty;
                columns[1].FullValue = temp;
            }
            else
            {
                columns[0] = timestmp;
                columns[1].FullValue = cols[1];
            }
        }

        return cLogLine;
    }

    /// <summary>
    /// Splits ReadOnlyMemory into two parts at the first occurrence of separator
    /// </summary>
    /// <param name="input">The memory to split</param>
    /// <param name="separator">The separator character</param>
    /// <returns>Array with 2 elements: [before separator, after separator].
    /// If separator not found, returns [input, Empty]</returns>
    private static ReadOnlyMemory<char>[] SplitIntoTwo (ReadOnlyMemory<char> input, char separator)
    {
        var span = input.Span;
        var index = span.IndexOf(separator);

        if (index == -1)
        {
            // No separator found - return whole input in first element
            return [input, ReadOnlyMemory<char>.Empty];
        }

        // Split at the separator
        return
        [
            input[..index],           // Before separator
            input[(index + 1)..]      // After separator (skip the separator itself)
        ];
    }

    public bool IsTimeshiftImplemented ()
    {
        return true;
    }

    public void SetTimeOffset (int msecOffset)
    {
        timeOffset = msecOffset;
    }

    public int GetTimeOffset ()
    {
        return timeOffset;
    }

    public DateTime GetTimestamp (ILogLineColumnizerCallback callback, ILogLine logLine)
    {
        return GetTimestamp(callback as ILogLineMemoryColumnizerCallback, logLine as ILogLineMemory);
    }

    public void PushValue (ILogLineColumnizerCallback callback, int column, string value, string oldValue)
    {
        PushValue(callback as ILogLineMemoryColumnizerCallback, column, value, oldValue);
    }

    public DateTime GetTimestamp (ILogLineMemoryColumnizerCallback callback, ILogLineMemory logLine)
    {
        var temp = logLine.FullLine;
        var span = temp.Span;

        // delete '[#|' and '|#]'
        if (span.StartsWith("[#|", StringComparison.OrdinalIgnoreCase))
        {
            temp = temp[3..];
        }

        if (span.EndsWith("|#]", StringComparison.OrdinalIgnoreCase))
        {
            temp = temp[..^3];
        }

        if (temp.Length < 28)
        {
            return DateTime.MinValue;
        }

        var endIndex = span.IndexOf(SEPARATOR_CHAR);
        if (endIndex is > 28 or < 0)
        {
            return DateTime.MinValue;
        }

        var value = temp[..endIndex];

        try
        {
            // convert glassfish timestamp into a readable format:
            return DateTime.TryParseExact(value.ToString(), DATETIME_FORMAT, cultureInfo, DateTimeStyles.None, out var timestamp)
                ? timestamp.AddMilliseconds(timeOffset)
                : DateTime.MinValue;
        }
        catch (Exception ex) when (ex is ArgumentException or
                                         FormatException or
                                         ArgumentOutOfRangeException)
        {
            return DateTime.MinValue;
        }
    }

    public void PushValue (ILogLineMemoryColumnizerCallback callback, int column, string value, string oldValue)
    {
        if (column == 0)
        {
            try
            {
                var newDateTime = DateTime.ParseExact(value, DATETIME_FORMAT_OUT, cultureInfo);
                var oldDateTime = DateTime.ParseExact(oldValue, DATETIME_FORMAT_OUT, cultureInfo);
                var mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                var mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                timeOffset = (int)(mSecsNew - mSecsOld);
            }
            catch (FormatException)
            {
            }
        }
    }

    /// <summary>
    /// Replaces all occurrences of a character in ReadOnlyMemory<char> (optimized)
    /// </summary>
    //TODO: Extract to a common utility class
    private static ReadOnlyMemory<char> ReplaceInMemory (ReadOnlyMemory<char> input, char oldChar, char newChar)
    {
        var span = input.Span;

        // check is there anything to replace?
        if (!span.Contains(oldChar))
        {
            return input;
        }

        // Allocate new buffer only when needed
        var buffer = new char[input.Length];

        for (var i = 0; i < span.Length; i++)
        {
            buffer[i] = span[i] == oldChar ? newChar : span[i];
        }

        return buffer.AsMemory();
    }

    #endregion
}