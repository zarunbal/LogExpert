namespace ColumnizerLib;

public interface IColumnizedLogLineMemory
{
    #region Properties

    ILogLineMemory LogLine { get; }

    IColumnMemory[] ColumnValues { get; }

    #endregion
}