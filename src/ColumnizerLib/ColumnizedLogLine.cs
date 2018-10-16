namespace LogExpert
{
    public class ColumnizedLogLine : IColumnizedLogLine
    {
        #region Interface IColumnizedLogLine

        public IColumn[] ColumnValues { get; set; }

        public ILogLine LogLine { get; set; }

        #endregion
    }
}
