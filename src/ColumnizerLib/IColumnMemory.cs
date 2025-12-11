namespace ColumnizerLib;

public interface IColumnMemory : IColumn, ITextValueMemory
{
    #region Properties

    IColumnizedLogLineMemory ParentMemory { get; }

    ReadOnlyMemory<char> FullValueMemory { get; }

    ReadOnlyMemory<char> DisplayValueMemory { get; }

    #endregion
}