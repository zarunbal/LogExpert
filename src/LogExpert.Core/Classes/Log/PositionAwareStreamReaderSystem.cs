using System.Text;

using LogExpert.Core.Entities;
using LogExpert.Core.Interfaces;

namespace LogExpert.Core.Classes.Log;

/// <summary>
/// This class is responsible for reading line from the log file. It also decodes characters with the appropriate charset encoding.
/// PositionAwareStreamReaderSystem tries a BOM detection to determine correct file offsets when directly seeking into the file (on re-loading flushed buffers).
/// UTF-8 handling is a bit slower, because after reading a character the byte length of the character must be determined.
/// Lines are read char-by-char. StreamReader.ReadLine() is not used because StreamReader cannot tell a file position.
/// </summary>
public class PositionAwareStreamReaderSystem : PositionAwareStreamReaderBase, ILogStreamReaderMemory
{
    #region Fields

    private const int CHAR_CR = 0x0D;
    private const int CHAR_LF = 0x0A;

    private int _newLineSequenceLength;

    private string _currentLine; // Store current line for Memory<char> access

    public override bool IsDisposed { get; protected set; }

    #endregion

    #region cTor

    public PositionAwareStreamReaderSystem (Stream stream, EncodingOptions encodingOptions, int maximumLineLength) : base(stream, encodingOptions, maximumLineLength)
    {
    }

    #endregion

    #region Public methods

    public override string ReadLine ()
    {
        var reader = GetStreamReader();

        if (_newLineSequenceLength == 0)
        {
            _newLineSequenceLength = GuessNewLineSequenceLength(reader);
        }

        var line = reader.ReadLine();

        if (line != null)
        {
            MovePosition(Encoding.GetByteCount(line) + _newLineSequenceLength);

            if (line.Length > MaximumLineLength)
            {
                line = line[..MaximumLineLength];
            }
        }

        return line;
    }

    /// <summary>
    /// Attempts to read the next line from the stream without allocating a new string.
    /// The returned Memory&lt;char&gt; is valid until the next call to TryReadLine or ReturnMemory.
    /// </summary>
    public bool TryReadLine (out ReadOnlyMemory<char> lineMemory)
    {
        var reader = GetStreamReader();

        if (_newLineSequenceLength == 0)
        {
            _newLineSequenceLength = GuessNewLineSequenceLength(reader);
        }

        var line = reader.ReadLine();

        if (line != null)
        {
            MovePosition(Encoding.GetByteCount(line) + _newLineSequenceLength);

            if (line.Length > MaximumLineLength)
            {
                line = line[..MaximumLineLength];
            }

            // Store line for Memory access
            _currentLine = line;
            lineMemory = line.AsMemory();
            return true;
        }

        lineMemory = default;
        return false;
    }

    /// <summary>
    /// Returns the memory buffer. For System reader, this is a no-op since we use string-backed Memory.
    /// </summary>
    public void ReturnMemory (ReadOnlyMemory<char> memory)
    {
        // No-op for System reader - string is already managed by GC
        _currentLine = null;
    }

    #endregion

    #region Private Methods

    private int GuessNewLineSequenceLength (StreamReader reader)
    {
        var currentPos = Position;

        try
        {
            var line = reader.ReadLine();

            if (line != null)
            {
                Position += Encoding.GetByteCount(line);

                var firstChar = reader.Read();
                if (firstChar == CHAR_CR) // check \r
                {
                    var secondChar = reader.Read();
                    if (secondChar == CHAR_LF) // check \n
                    {
                        // Use stackalloc or SpanOwner instead of string
                        Span<char> newline = ['\r', '\n'];
                        return Encoding.GetByteCount(newline);
                        //return Encoding.GetByteCount("\r\n");
                    }
                }

                Span<char> singleChar = [(char)firstChar];
                return Encoding.GetByteCount(singleChar);
            }

            return 0;
        }
        finally
        {
            Position = currentPos;
        }
    }

    #endregion
}