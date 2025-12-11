namespace ColumnizerLib;

public interface IColumnizedLogLineMemory : IColumnizedLogLine
{
    #region Properties

    ILogLineMemory LogLineMemory { get; }

    IColumnMemory[] ColumnMemoryValues { get; }

    #endregion
}