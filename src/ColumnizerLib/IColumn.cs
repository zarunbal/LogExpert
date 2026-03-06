namespace ColumnizerLib;

[Obsolete("This interface is deprecated. Use IColumnMemory for a memory-based implementation instead.")]
public interface IColumn : ITextValue
{
    #region Properties

    IColumnizedLogLine Parent { get; }

    string FullValue { get; }

    string DisplayValue { get; }

    #endregion
}