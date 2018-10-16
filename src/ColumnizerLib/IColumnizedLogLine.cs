namespace LogExpert
{
    public interface IColumnizedLogLine
    {
        #region Properties / Indexers

        IColumn[] ColumnValues { get; }

        ILogLine LogLine { get; }

        #endregion
    }
}
