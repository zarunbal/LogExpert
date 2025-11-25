using System.Globalization;

using ColumnizerLib;

using LogExpert;

namespace GlassfishColumnizer;

internal class GlassfishColumnizer : ILogLineXmlColumnizer
{
    #region Fields

    public const int COLUMN_COUNT = 2;
    private const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzzz";
    private const string DATETIME_FORMAT_OUT = "yyyy-MM-dd HH:mm:ss.fff";
    private const char SEPARATOR_CHAR = '|';

    private static readonly XmlConfig xmlConfig = new();

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
        return xmlConfig;
    }

    public ILogLine GetLineTextForClipboard (ILogLine logLine, ILogLineColumnizerCallback callback)
    {
        GlassFishLogLine line = new()
        {
            FullLine = logLine.FullLine.Replace(SEPARATOR_CHAR, '|'),
            LineNumber = logLine.LineNumber
        };

        return line;
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

    public IColumnizedLogLine SplitLine (ILogLineColumnizerCallback callback, ILogLine line)
    {
        ColumnizedLogLine cLogLine = new()
        {
            LogLine = line
        };

        var temp = line.FullLine;

        var columns = Column.CreateColumns(COLUMN_COUNT, cLogLine);
        cLogLine.ColumnValues = [.. columns.Select(a => a as IColumn)];

        // delete '[#|' and '|#]'
        if (temp.StartsWith("[#|", StringComparison.OrdinalIgnoreCase))
        {
            temp = temp[3..];
        }

        if (temp.EndsWith("|#]", StringComparison.OrdinalIgnoreCase))
        {
            temp = temp[..^3];
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
                columns[0].FullValue = newDate;
            }
            catch (Exception)
            {
                columns[0].FullValue = "n/a";
            }

            var timestmp = columns[0];

            string[] cols;
            cols = temp.Split(trimChars, COLUMN_COUNT, StringSplitOptions.None);

            if (cols.Length != COLUMN_COUNT)
            {
                columns[0].FullValue = string.Empty;
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
        var temp = logLine.FullLine;

        // delete '[#|' and '|#]'
        if (temp.StartsWith("[#|", StringComparison.OrdinalIgnoreCase))
        {
            temp = temp[3..];
        }

        if (temp.EndsWith("|#]", StringComparison.OrdinalIgnoreCase))
        {
            temp = temp[..^3];
        }

        if (temp.Length < 28)
        {
            return DateTime.MinValue;
        }

        var endIndex = temp.IndexOf(SEPARATOR_CHAR, 1);
        if (endIndex is > 28 or < 0)
        {
            return DateTime.MinValue;
        }

        var value = temp[..endIndex];

        try
        {
            // convert glassfish timestamp into a readable format:
            return DateTime.TryParseExact(value, DATETIME_FORMAT, cultureInfo, DateTimeStyles.None, out var timestamp)
                ? timestamp.AddMilliseconds(timeOffset)
                : DateTime.MinValue;
        }
        catch (Exception)
        {
            return DateTime.MinValue;
        }
    }

    public void PushValue (ILogLineColumnizerCallback callback, int column, string value, string oldValue)
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

    #endregion
}