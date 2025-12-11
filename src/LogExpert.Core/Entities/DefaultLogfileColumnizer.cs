using ColumnizerLib;

namespace LogExpert.Core.Entities;

public class DefaultLogfileColumnizer : ILogLineMemoryColumnizer
{
    #region ILogLineColumnizer Members

    public string GetName ()
    {
        return Resources.LogExpert_DefaultLogfileColumnicer_Name;
    }

    public string GetDescription ()
    {
        return Resources.LogExpert_DefaultLogfileColumnicer_Description;
    }

    public int GetColumnCount ()
    {
        return 1;
    }

    public string[] GetColumnNames ()
    {
        return ["Text"];
    }

    /// <summary>
    /// Splits the specified log line into columns using the provided callback.
    /// </summary>
    /// <param name="callback">An object that provides callback methods for columnizing the log line. May be used to customize or influence the
    /// columnization process.</param>
    /// <param name="line">The log line to be split into columns. Cannot be null.</param>
    /// <returns>An object representing the columnized version of the specified log line.</returns>
    public IColumnizedLogLineMemory SplitLine (ILogLineColumnizerCallback callback, ILogLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        ColumnizedLogLine cLogLine = new()
        {
            LogLine = line
        };

        cLogLine.ColumnValues =
        [
            new Column
            {
                FullValue = line.FullLine,
                Parent = cLogLine
            }
        ];

        return cLogLine;
    }

    /// <summary>
    /// Splits the specified log line into columns using the provided callback.
    /// </summary>
    /// <param name="callback">A callback interface that can be used to customize or influence the columnization process. May be null if no
    /// callback behavior is required.</param>
    /// <param name="line">The log line to be split into columns. Cannot be null.</param>
    /// <returns>An object representing the columnized version of the specified log line.</returns>
    public IColumnizedLogLineMemory SplitLine (ILogLineMemoryColumnizerCallback callback, ILogLineMemory line)
    {
        ArgumentNullException.ThrowIfNull(line);

        ColumnizedLogLine cLogLine = new()
        {
            LogLineMemory = line
        };

        cLogLine.ColumnValues =
        [
            new Column
            {
                FullValueMemory = line.FullLineMemory,
                Parent = cLogLine
            }
        ];

        return cLogLine;
    }

    public DateTime GetTimestamp (ILogLineMemoryColumnizerCallback callback, ILogLineMemory line)
    {
        // No special handling needed for default columnizer
        return DateTime.MinValue;
    }

    public void PushValue (ILogLineMemoryColumnizerCallback callback, int column, string value, string oldValue)
    {
        // No special handling needed for default columnizer
    }

    public string Text => GetName();

    public static Priority GetPriority (string fileName, IEnumerable<ILogLine> samples)
    {
        return Priority.CanSupport;
    }
    #endregion

    #region ILogLineColumnizer Not implemented Members

    public bool IsTimeshiftImplemented ()
    {
        return false;
    }

    public void SetTimeOffset (int msecOffset)
    {
        // No special handling needed for default columnizer
    }

    public int GetTimeOffset ()
    {
        // No special handling needed for default columnizer
        return int.MinValue;
    }

    public DateTime GetTimestamp (ILogLineColumnizerCallback callback, ILogLine line)
    {
        // No special handling needed for default columnizer
        return DateTime.MinValue;
    }

    public void PushValue (ILogLineColumnizerCallback callback, int column, string value, string oldValue)
    {
        // No special handling needed for default columnizer
    }

    public string GetCustomName ()
    {
        return GetName();
    }

    #endregion
}