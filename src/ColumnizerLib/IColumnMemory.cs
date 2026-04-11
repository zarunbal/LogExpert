namespace ColumnizerLib;

public interface IColumnMemory : ITextValueMemory
{
    #region Properties

    IColumnizedLogLineMemory Parent { get; }

    ReadOnlyMemory<char> FullValue { get; }

    ReadOnlyMemory<char> DisplayValue { get; }

    #endregion
}