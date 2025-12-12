namespace ColumnizerLib;

public interface IColumnMemory : IColumn, ITextValueMemory
{
    #region Properties

    new IColumnizedLogLineMemory Parent { get; }

    new ReadOnlyMemory<char> FullValue { get; }

    new ReadOnlyMemory<char> DisplayValue { get; }

    #endregion
}