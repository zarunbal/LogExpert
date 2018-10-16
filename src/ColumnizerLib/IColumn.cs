namespace LogExpert
{
    public interface IColumn : ITextValue
    {
        #region Properties / Indexers

        string DisplayValue { get; }

        string FullValue { get; }

        IColumnizedLogLine Parent { get; }

        #endregion
    }
}
