using System.IO;

namespace LogExpert.Classes.Log
{
    /// <summary>
    /// This class is responsible for reading line from the log file. It also decodes characters with the appropriate charset encoding.
    /// PositionAwareStreamReaderSystem tries a BOM detection to determine correct file offsets when directly seeking into the file (on re-loading flushed buffers).
    /// UTF-8 handling is a bit slower, because after reading a character the byte length of the character must be determined.
    /// Lines are read char-by-char. StreamReader.ReadLine() is not used because StreamReader cannot tell a file position.
    /// </summary>
    public class PositionAwareStreamReaderSystem : PositionAwareStreamReaderBase
    {
        #region Fields

        private const int CHAR_CR = 0x0D;
        private const int CHAR_LF = 0x0A;

        private int _newLineSequenceLength;

        #endregion

        #region cTor

        public PositionAwareStreamReaderSystem(Stream stream, EncodingOptions encodingOptions)
            : base(stream, encodingOptions)
        {
            
        }

        #endregion

        #region Public methods

        public override string ReadLine()
        {
            StreamReader reader = this.GetStreamReader();

            if (this._newLineSequenceLength == 0)
            {
                this._newLineSequenceLength = this.guessNewLineSequenceLength(reader);
            }

            string line = reader.ReadLine();

            if (line != null)
            {
                this.MovePosition(this.Encoding.GetByteCount(line) + this._newLineSequenceLength);

                if (line.Length > MAX_LINE_LEN)
                {
                    line = line.Remove(MAX_LINE_LEN);
                }
            }

            return line;
        }

        #endregion

        #region Private Methods

        private int guessNewLineSequenceLength(StreamReader reader)
        {
            long currentPos = this.Position;

            try
            {
                string line = reader.ReadLine();

                if (line != null)
                {
                    this.Position += this.Encoding.GetByteCount(line);
                    
                    int firstChar = reader.Read();
                    if (firstChar == CHAR_CR) // check \r
                    {
                        int secondChar = reader.Read();
                        if (secondChar == CHAR_LF) // check \n
                        {
                            return this.Encoding.GetByteCount("\r\n");
                        }
                    }
                    return this.Encoding.GetByteCount(((char)firstChar).ToString());
                }

                return 0;
            }
            finally
            {
                this.Position = currentPos;
            }
        }

        #endregion
    }
}