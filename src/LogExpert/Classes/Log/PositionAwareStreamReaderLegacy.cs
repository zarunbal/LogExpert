using System.IO;

namespace LogExpert.Classes.Log
{
    public class PositionAwareStreamReaderLegacy : PositionAwareStreamReaderBase
    {
        #region Fields

        private readonly char[] charBuffer = new char[MAX_LINE_LEN];

        private int _charBufferPos = 0;
        private bool _crDetect = false;

        #endregion

        #region cTor

        public PositionAwareStreamReaderLegacy(Stream stream, EncodingOptions encodingOptions) 
            : base(stream, encodingOptions)
        {

        }

        #endregion

        #region Public methods

        public override string ReadLine()
        {
            int readInt;

            while (-1 != (readInt = ReadChar()))
            {
                char readChar = (char)readInt;

                switch (readChar)
                {
                    case '\n':
                        _crDetect = false;
                        return getLineAndResetCharBufferPos();
                    case '\r':
                        if (_crDetect)
                        {
                            return getLineAndResetCharBufferPos();
                        }
                        else
                        {
                            _crDetect = true;
                        }
                        break;
                    default:
                        if (_crDetect)
                        {
                            _crDetect = false;
                            string line = getLineAndResetCharBufferPos();
                            appendToCharBuffer(readChar);
                            return line;
                        }
                        else
                        {
                            appendToCharBuffer(readChar);
                        }
                        break;
                }
            }

            string result = getLineAndResetCharBufferPos();
            if (readInt == -1 && result.Length == 0 && !_crDetect)
            {
                return null; // EOF
            }
            _crDetect = false;
            return result;
        }

        protected override void ResetReader()
        {
            resetCharBufferPos();

            base.ResetReader();
        }

        #endregion

        #region Private Methods

        private string getLineAndResetCharBufferPos()
        {
            string result = new string(charBuffer, 0, _charBufferPos);
            resetCharBufferPos();
            return result;
        }

        private void appendToCharBuffer(char readChar)
        {
            if (_charBufferPos < MAX_LINE_LEN)
            {
                charBuffer[_charBufferPos++] = readChar;
            }
        }

        private void resetCharBufferPos()
        {
            _charBufferPos = 0;
        }

        #endregion
    }
}