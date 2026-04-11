namespace ColumnizerLib;

[Obsolete("This interface is deprecated. Use IColumnizedLogLineMemory for a memory-based implementation instead.")]
public interface IColumnizedLogLine
{
    #region Properties

    ILogLine LogLine { get; }

    IColumn[] ColumnValues { get; }

    #endregion
}