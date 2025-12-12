namespace LogExpert.Core.Interface;

public interface ISpanLineReader
{
    bool TryReadLine (out ReadOnlySpan<char> line);

    long Position { get; }
}