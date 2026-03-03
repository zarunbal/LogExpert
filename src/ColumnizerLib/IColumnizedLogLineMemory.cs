namespace ColumnizerLib;

public interface IColumnizedLogLineMemory : IColumnizedLogLine
{
    #region Properties

    new ILogLineMemory LogLine { get; }

    new IColumnMemory[] ColumnValues { get; }

    #endregion
}