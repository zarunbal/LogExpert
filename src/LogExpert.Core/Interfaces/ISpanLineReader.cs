namespace LogExpert.Core.Interfaces;

public interface ISpanLineReader
{
    bool TryReadLine (out ReadOnlySpan<char> line);

    long Position { get; }
}