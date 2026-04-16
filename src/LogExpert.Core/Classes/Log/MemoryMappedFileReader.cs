using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Text;

using ColumnizerLib;

namespace LogExpert.Core.Classes.Log;

/// <summary>
/// Reads log lines via memory-mapped file access. Builds a line-offset index on load.
/// Supports tail mode by re-mapping when the file grows.
/// </summary>
internal sealed class MemoryMappedFileReader (string filePath, Encoding encoding) : IDisposable
{
    private readonly Encoding _encoding = encoding;
    private readonly LineOffsetIndex _lineIndex = new();
    private MemoryMappedFile _mmf;
    private MemoryMappedViewAccessor _accessor;
    private long _mappedLength;
    private readonly string _filePath = filePath;

    public int LineCount => _lineIndex.LineCount;

    /// <summary>
    /// Builds (or rebuilds) the line-offset index by scanning for newline characters.
    /// In tail mode, call with startOffset = previously mapped length to index only new content.
    /// </summary>
    public void BuildIndex (long startOffset = 0)
    {
        using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var fileLength = fs.Length;

        if (startOffset == 0)
        {
            _lineIndex.Clear();
            _lineIndex.Add(0); // first line starts at offset 0
        }

        fs.Position = startOffset;
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            int bytesRead;
            var position = startOffset;
            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (var i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == (byte)'\n')
                    {
                        _lineIndex.Add(position + i + 1);
                    }
                }

                position += bytesRead;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        // Re-map the file
        RemapFile(fileLength);
    }

    /// <summary>
    /// Extends the mapping to cover new file content (for tail mode).
    /// </summary>
    public void ExtendIndex ()
    {
        BuildIndex(_mappedLength);
    }

    /// <summary>
    /// Reads a single line by its zero-based line number.
    /// Returns the line as an ILogLineMemory.
    /// </summary>
    public ILogLineMemory GetLine (int lineNum)
    {
        var offset = _lineIndex.GetOffset(lineNum);
        if (offset < 0 || _accessor == null)
        {
            return null;
        }

        var length = _lineIndex.GetLineLength(lineNum);
        if (length < 0)
        {
            // Last line — read to the end of file or a reasonable limit
            length = Math.Min(_mappedLength - offset, 1024 * 1024);
        }

        if (length <= 0)
        {
            return new LogLine(ReadOnlyMemory<char>.Empty, lineNum);
        }

        // Read bytes from the mapped view
        var bytes = new byte[length];
        _ = _accessor.ReadArray(offset, bytes, 0, (int)length);

        // Trim trailing \r\n
        var end = (int)length;
        if (end > 0 && bytes[end - 1] == '\n')
        {
            end--;
        }

        if (end > 0 && bytes[end - 1] == '\r')
        {
            end--;
        }

        var text = _encoding.GetString(bytes, 0, end);
        return new LogLine(text, lineNum);
    }

    private void RemapFile (long fileLength)
    {
        _accessor?.Dispose();
        _mmf?.Dispose();

        if (fileLength == 0)
        {
            _mappedLength = 0;
            return;
        }

        _mmf = MemoryMappedFile.CreateFromFile(
            _filePath,
            FileMode.Open,
            mapName: null,
            capacity: fileLength,
            MemoryMappedFileAccess.Read);

        _accessor = _mmf.CreateViewAccessor(0, fileLength, MemoryMappedFileAccess.Read);
        _mappedLength = fileLength;
    }

    public void Dispose ()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}
