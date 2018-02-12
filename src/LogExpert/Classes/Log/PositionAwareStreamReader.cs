using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Linq;
using System.Text;
using System.IO;

namespace LogExpert
{
    /// <summary>
    /// This class is responsible for reading line from the log file. It also decodes characters with the appropriate charset encoding.
    /// PositionAwareStreamReader tries a BOM detection to determine correct file offsets when directly seeking into the file (on re-loading flushed buffers).
    /// UTF-8 handling is a bit slower, because after reading a character the byte length of the character must be determined.
    /// Lines are read char-by-char. StreamReader.ReadLine() is not used because StreamReader cannot tell a file position.
    /// </summary>
    public class PositionAwareStreamReader : ILogStreamReader
    {
        #region Fields

        private const int MAX_LINE_LEN = 20000;
        private readonly char[] charBuffer = new char[MAX_LINE_LEN];
        private readonly int posInc_precomputed;
        private readonly int preambleLength = 0;

        private readonly StreamReader reader;

        private readonly Stream stream;
        private readonly bool useSystemReaderMethod;
        private int charBufferPos = 0;
        private Encoding detectedEncoding;
        private int newLineSequenceLength;
        private long pos;

        //StringBuilder builder;
        private int state;

        #endregion

        #region cTor

        public PositionAwareStreamReader(Stream stream, EncodingOptions encodingOptions, bool useSystemReaderMethod)
        {
            this.useSystemReaderMethod = useSystemReaderMethod;
            this.stream = new BufferedStream(stream);
            preambleLength = DetectPreambleLengthAndEncoding();

            Encoding usedEncoding;
            if (detectedEncoding != null && encodingOptions.Encoding == null)
            {
                usedEncoding = detectedEncoding;
            }
            else if (encodingOptions.Encoding != null)
            {
                usedEncoding = encodingOptions.Encoding;
            }
            else
            {
                usedEncoding = encodingOptions.DefaultEncoding != null
                    ? encodingOptions.DefaultEncoding
                    : Encoding.Default;
            }

            if (usedEncoding is UTF8Encoding)
            {
                posInc_precomputed = 0;
            }
            else if (usedEncoding is UnicodeEncoding)
            {
                posInc_precomputed = 2;
            }
            else
            {
                posInc_precomputed = 1;
            }

            reader = new StreamReader(this.stream, usedEncoding, true);
            ResetReader();
            Position = 0;

            if (this.useSystemReaderMethod)
            {
                newLineSequenceLength = guessNewLineSequenceLength();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public long Position
        {
            get { return pos; }
            set
            {
                /*
                 * 1: Irgendwann mal auskommentiert (+this.Encoding.GetPreamble().Length)
                 * 2: Stand bei 1.1 3207
                 * 3: Stand nach Fehlermeldung von Piet wegen Unicode-Bugs. 
                 *    Keihne Ahnung, ob das jetzt endgültig OK ist.
                 * 4: 27.07.09: Preamble-Length wird jetzt im CT ermittelt, da Encoding.GetPreamble().Length
                 *    immer eine fixe Länge liefert (unabhängig vom echtem Dateiinhalt)
                 */
                pos = value; //  +this.Encoding.GetPreamble().Length;      // 1
                //this.stream.Seek(this.pos, SeekOrigin.Begin);     // 2
                //this.stream.Seek(this.pos + this.Encoding.GetPreamble().Length, SeekOrigin.Begin);  // 3
                stream.Seek(pos + preambleLength, SeekOrigin.Begin); // 4
                ResetReader();
            }
        }

        public Encoding Encoding
        {
            get { return reader.CurrentEncoding; }
        }

        public bool IsBufferComplete
        {
            get { return true; }
        }

        #endregion

        #region Public methods

        public void Close()
        {
            stream.Close();
        }

        public unsafe int ReadChar()
        {
            int readInt;
            try
            {
                readInt = reader.Read();
                if (readInt != -1)
                {
                    char readChar = (char) readInt;
                    int posInc = posInc_precomputed != 0
                        ? posInc_precomputed
                        : reader.CurrentEncoding.GetByteCount(&readChar, 1);
                    pos += posInc;
                }
            }
            catch (IOException)
            {
                readInt = -1;
            }

            return readInt;
        }

        public string ReadLine()
        {
            return useSystemReaderMethod ? ReadLineNew() : ReadLineOld();
        }

        #endregion

        #region Private Methods

        private int guessNewLineSequenceLength()
        {
            long currentPos = Position;
            int len = 0;
            string line = reader.ReadLine();
            if (line != null)
            {
                stream.Seek(Encoding.GetByteCount(line) + preambleLength, SeekOrigin.Begin);
                ResetReader();
                int b = reader.Read();
                // int b = this.stream.ReadByte();
                if (b == 0x0d)
                {
                    // b = this.stream.ReadByte();
                    b = reader.Read();
                    if (b == 0x0a)
                    {
                        len = 2;
                    }
                    else
                    {
                        len = 1;
                    }
                }
                else
                {
                    len = 1;
                }

                len *= Encoding.GetByteCount("\r");
            }

            Position = currentPos;
            return len;
        }

        //private string GetLineAndResetBuilder()
        //{
        //  string result;
        //  result = this.builder.ToString();
        //  this.state = 0;
        //  if (this.builder.Length > MAX_LINE_LEN)
        //    result = result.Substring(0, MAX_LINE_LEN);
        //  NewBuilder();
        //  return result;
        //}

        //private void appendToBuilder(char[] readChar)
        //{
        //  this.builder.Append(Char.ToString(readChar[0]));
        //}

        //private void NewBuilder()
        //{
        //  this.builder = new StringBuilder(400);
        //}

        private string GetLineAndResetBuilder()
        {
            string result = new string(charBuffer, 0, charBufferPos);
            NewBuilder();
            state = 0;
            return result;
        }

        private void appendToBuilder(char readChar)
        {
            if (charBufferPos >= MAX_LINE_LEN)
            {
                return;
            }

            charBuffer[charBufferPos++] = readChar;
        }

        private void NewBuilder()
        {
            charBufferPos = 0;
        }

        private void ResetReader()
        {
            state = 0;
            NewBuilder();
            reader.DiscardBufferedData();
        }

        /// <summary>
        /// Determines the actual number of preamble bytes in the file.
        /// </summary>
        /// <returns></returns>
        private int DetectPreambleLengthAndEncoding()
        {
            /*
            UTF-8:                                EF BB BF 
            UTF-16-Big-Endian-Bytereihenfolge:    FE FF 
            UTF-16-Little-Endian-Bytereihenfolge: FF FE 
            UTF-32-Big-Endian-Bytereihenfolge:    00 00 FE FF 
            UTF-32-Little-Endian-Bytereihenfolge: FF FE 00 00 
            */

            detectedEncoding = null;
            byte[] readPreamble = new byte[4];
            int readLen = stream.Read(readPreamble, 0, 4);
            if (readLen < 2)
            {
                return 0;
            }

            Encoding[] encodings = new Encoding[]
            {
                Encoding.UTF8,
                Encoding.Unicode,
                Encoding.BigEndianUnicode,
                Encoding.UTF32
            };
            byte[][] preambles = new byte[4][]
            {
                Encoding.UTF8.GetPreamble(),
                Encoding.Unicode.GetPreamble(),
                Encoding.BigEndianUnicode.GetPreamble(),
                Encoding.UTF32.GetPreamble()
            };
            foreach (Encoding encoding in encodings)
            {
                byte[] preamble = encoding.GetPreamble();
                bool fail = false;
                for (int i = 0; i < readLen && i < preamble.Length; ++i)
                {
                    if (readPreamble[i] != preamble[i])
                    {
                        fail = true;
                        break;
                    }
                }

                if (!fail)
                {
                    detectedEncoding = encoding;
                    return preamble.Length;
                }
            }

            return 0;
        }

        #endregion

        protected string ReadLineNew()
        {
            if (newLineSequenceLength == 0)
            {
                newLineSequenceLength = guessNewLineSequenceLength();
            }

            string line = reader.ReadLine();
            if (line != null)
            {
                pos += Encoding.GetByteCount(line);
                if (!reader.EndOfStream) //TO avoid setting position ahead of the file
                {
                    pos += newLineSequenceLength;
                }

                if (line.Length > MAX_LINE_LEN)
                {
                    line = line.Remove(MAX_LINE_LEN);
                }
            }

            return line;
        }

        protected string ReadLineOld()
        {
            string result;
            int readInt;

            while (-1 != (readInt = ReadChar()))
            {
                char readChar = (char) readInt;

                // state: 0: looking for \r or \n, 
                //        1: looking for \n after \r
                //        2: looking for 
                switch (readChar)
                {
                    case '\r':
                        switch (state)
                        {
                            case 0:
                                state = 1;
                                break;
                            case 1:
                                result = GetLineAndResetBuilder();
                                return result;
                        }

                        break;
                    case '\n':
                        switch (state)
                        {
                            case 0:
                            // fall through
                            case 1:
                                result = GetLineAndResetBuilder();
                                return result;
                        }

                        break;
                    default:
                        switch (state)
                        {
                            case 0:
                                appendToBuilder(readChar);
                                break;
                            case 1:
                                appendToBuilder(readChar);
                                state = 0;
                                break;
                        }

                        break;
                }

                //if (this.builder.Length > MAX_LINE_LEN)
                //  break;
            }

            //if (builder.Length == 0)
            //  return null;  // EOF
            result = GetLineAndResetBuilder();
            //if (result.Length == 0)
            //  return null;  // EOF
            if (readInt == -1 && result.Length == 0)
            {
                return null; // EOF
            }

            return result;
        }
    }
}