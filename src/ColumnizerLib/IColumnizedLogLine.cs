namespace ColumnizerLib;

public interface IColumnizedLogLine
{
    #region Properties

    ILogLine LogLine { get; }

    IColumn[] ColumnValues { get; }

    #endregion
}