using System.Globalization;
using System.Text.RegularExpressions;

using ColumnizerLib;

namespace LogExpert.Core.Classes.Columnizer;

public partial class ClfColumnizer : ILogLineMemoryColumnizer
{
    private const string DATE_TIME_FORMAT = "dd/MMM/yyyy:HH:mm:ss zzz";

    #region Fields

    private readonly Regex _lineRegex = LineRegex();

    private readonly CultureInfo _cultureInfo = new("en-US");
    private int _timeOffset;

    #endregion

    #region cTor

    // anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html"

    public ClfColumnizer ()
    {
    }

    #endregion

    #region Public methods

    public bool IsTimeshiftImplemented ()
    {
        return true;
    }

    public void SetTimeOffset (int msecOffset)
    {
        _timeOffset = msecOffset;
    }

    public int GetTimeOffset ()
    {
        return _timeOffset;
    }

    public DateTime GetTimestamp (ILogLineColumnizerCallback callback, ILogLine line)
    {
        return GetTimestamp(callback as ILogLineMemoryColumnizerCallback, line as ILogLineMemory);
    }

    public void PushValue (ILogLineColumnizerCallback callback, int column, string value, string oldValue)
    {
        PushValue(callback as ILogLineMemoryColumnizerCallback, column, value, oldValue);
    }

    public string GetName ()
    {
        return "Webserver CLF Columnizer";
    }

    public string GetDescription ()
    {
        return "Common Logfile Format used by webservers.";
    }

    public int GetColumnCount ()
    {
        return 8;
    }

    public string[] GetColumnNames ()
    {
        return ["IP", "User", "Date/Time", "Request", "Status", "Bytes", "Referrer", "User agent"];
    }

    public IColumnizedLogLine SplitLine (ILogLineColumnizerCallback callback, ILogLine line)
    {
        return SplitLine(callback as ILogLineMemoryColumnizerCallback, line as ILogLineMemory);
    }

    public DateTime GetTimestamp (ILogLineMemoryColumnizerCallback callback, ILogLineMemory line)
    {
        // Use SplitLine to parse, then extract timestamp column
        var cols = SplitLine(callback, line);

        if (cols == null || cols.ColumnValues.Length < 8)
        {
            return DateTime.MinValue;
        }

        if (cols.ColumnValues[2] is not IColumnMemory dateColumn || dateColumn.FullValue.IsEmpty)
        {
            return DateTime.MinValue;
        }

        try
        {
            // Parse from Span
            return DateTime.ParseExact(dateColumn.FullValue.Span, DATE_TIME_FORMAT, _cultureInfo);
        }
        catch (Exception ex) when (ex is ArgumentException or
                                         FormatException or
                                         ArgumentOutOfRangeException)
        {
            return DateTime.MinValue;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Intentionally Passed")]
    public IColumnizedLogLineMemory SplitLine (ILogLineMemoryColumnizerCallback callback, ILogLineMemory line)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        ColumnizedLogLine cLogLine = new()
        {
            LogLine = line
        };

        var columns = new Column[8]
        {
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine},
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine},
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine},
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine},
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine},
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine},
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine},
            new() {FullValue = ReadOnlyMemory<char>.Empty, Parent = cLogLine}
        };

        cLogLine.ColumnValues = [.. columns.Select(a => a as IColumnMemory)];

        var lineMemory = line.FullLine;

        if (lineMemory.Length > 1024)
        {
            columns[3].FullValue = lineMemory[..1024];
            return cLogLine;
        }

        var span = line.FullLine.Span;

        // 0         1         2         3         4         5         6         7         8         9         10        11        12        13        14        15        16
        // anon-212-34-174-126.suchen.de - - [08/Mar/2008:00:41:10 +0100] "GET /wiki/index.php?title=Bild:Poster_small.jpg&printable=yes&printable=yes HTTP/1.1" 304 0 "http://www.captain-kloppi.de/wiki/index.php?title=Bild:Poster_small.jpg&printable=yes" "gonzo1[P] +http://www.suchen.de/faq.html"
        if (!_lineRegex.IsMatch(span))
        {
            // Pattern didn't match - put entire line in request column
            columns[3].FullValue = lineMemory;
            return cLogLine;
        }

        var lineString = line.ToString();
        var match = _lineRegex.Match(lineString);

        if (match.Groups.Count == 10)
        {
            columns[0].FullValue = GetGroupMemory(lineMemory, match.Groups[1]);
            columns[1].FullValue = GetGroupMemory(lineMemory, match.Groups[3]);
            columns[3].FullValue = GetGroupMemory(lineMemory, match.Groups[5]);
            columns[4].FullValue = GetGroupMemory(lineMemory, match.Groups[6]);
            columns[5].FullValue = GetGroupMemory(lineMemory, match.Groups[7]);
            columns[6].FullValue = GetGroupMemory(lineMemory, match.Groups[8]);
            columns[7].FullValue = GetGroupMemory(lineMemory, match.Groups[9]);

            var dateTimeMemory = GetGroupMemory(lineMemory, match.Groups[4]);

            if (dateTimeMemory.Length > 2)
            {
                // Skip '[' at start and ']' at end
                dateTimeMemory = dateTimeMemory[1..^1];
            }

            var dateSpan = dateTimeMemory.Span;

            // dirty probing of date/time format (much faster than DateTime.ParseExact()
            if (dateSpan.Length >= 12 && dateSpan[2] == '/' && dateSpan[6] == '/' && dateSpan[11] == ':')
            {
                if (_timeOffset != 0)
                {
                    try
                    {
                        var dateTime = DateTime.ParseExact(dateSpan, DATE_TIME_FORMAT, _cultureInfo);
                        dateTime = dateTime.Add(new TimeSpan(0, 0, 0, 0, _timeOffset));
                        var newDate = dateTime.ToString(DATE_TIME_FORMAT, _cultureInfo);
                        columns[2].FullValue = newDate.AsMemory();
                    }
                    catch (Exception ex) when (ex is ArgumentException or
                                                     FormatException or
                                                     ArgumentOutOfRangeException)
                    {
                        columns[2].FullValue = "n/a".AsMemory();
                    }
                }
                else
                {
                    columns[2].FullValue = dateTimeMemory;
                }
            }
            else
            {
                columns[2].FullValue = dateTimeMemory;
            }
        }
        else
        {
            columns[3].FullValue = lineMemory;
        }

        return cLogLine;
    }

    /// <summary>
    /// Converts a Regex Group capture to ReadOnlyMemory slice from original line
    /// </summary>
    private static ReadOnlyMemory<char> GetGroupMemory (ReadOnlyMemory<char> lineMemory, Group group)
    {
        if (!group.Success || group.Length == 0)
        {
            return ReadOnlyMemory<char>.Empty;
        }

        // Use group's Index and Length to slice original memory
        // This avoids allocating a new string for the group value
        return lineMemory.Slice(group.Index, group.Length);
    }

    public string GetCustomName ()
    {
        return GetName();
    }

    public void PushValue (ILogLineMemoryColumnizerCallback callback, int column, string value, string oldValue)
    {
        if (column == 2)
        {
            try
            {
                var newDateTime = DateTime.ParseExact(value, DATE_TIME_FORMAT, _cultureInfo);
                var oldDateTime = DateTime.ParseExact(oldValue, DATE_TIME_FORMAT, _cultureInfo);
                var mSecsOld = oldDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                var mSecsNew = newDateTime.Ticks / TimeSpan.TicksPerMillisecond;
                _timeOffset = (int)(mSecsNew - mSecsOld);
            }
            catch (FormatException)
            {
            }
        }
    }

    [GeneratedRegex("(.*) (-) (.*) (\\[.*\\]) (\".*\") (.*) (.*) (\".*\") (\".*\")")]
    private static partial Regex LineRegex ();

    #endregion
}