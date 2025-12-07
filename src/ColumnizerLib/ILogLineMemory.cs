namespace ColumnizerLib;

public interface ILogLineMemory : ILogLine
{
    ReadOnlyMemory<char> FullLineMemory { get; }
}
