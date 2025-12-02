namespace LogExpert.Core.Interface;

internal interface ILogStreamReaderSpan : ILogStreamReader
{

    bool TryReadLine (out ReadOnlyMemory<char> lineMemory);

    void ReturnMemory (ReadOnlyMemory<char> memory);
}
