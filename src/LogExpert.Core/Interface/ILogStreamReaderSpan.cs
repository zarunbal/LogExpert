namespace LogExpert.Core.Interface;

public interface ILogStreamReaderSpan : ILogStreamReader
{

    bool TryReadLine (out ReadOnlyMemory<char> lineMemory);

    void ReturnMemory (ReadOnlyMemory<char> memory);
}
