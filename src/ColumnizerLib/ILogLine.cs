namespace LogExpert
{
    public interface ILogLine : ITextValue
    {
        #region Properties / Indexers

        string FullLine { get; }

        int LineNumber { get; }

        #endregion
    }
}
