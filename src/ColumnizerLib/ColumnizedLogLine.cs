namespace ColumnizerLib;

public class ColumnizedLogLine : IColumnizedLogLineMemory
{
    #region Properties

    public ILogLineMemory LogLine { get; set; }

    public IColumnMemory[] ColumnValues { get; set; }

    #endregion
}