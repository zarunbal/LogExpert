namespace ColumnizerLib;

public class ColumnizedLogLine : IColumnizedLogLineMemory
{
    #region Properties

    [Obsolete("Use the Property of IColumnizedLogLineMemory")]
    ILogLine IColumnizedLogLine.LogLine { get; }

    [Obsolete("Use the Property of IColumnizedLogLineMemory")]
    IColumn[] IColumnizedLogLine.ColumnValues { get; }

    public ILogLineMemory LogLine { get; set; }

    public IColumnMemory[] ColumnValues { get; set; }

    #endregion
}