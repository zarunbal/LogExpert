namespace ColumnizerLib;

public class ColumnizedLogLine : IColumnizedLogLineMemory
{
    #region Properties

    public ILogLine LogLine { get; set; }

    public IColumn[] ColumnValues { get; set; }

    public ILogLineMemory LogLineMemory { get; set; }

    public IColumnMemory[] ColumnMemoryValues { get; set; }

    #endregion
}