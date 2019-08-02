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

            while (-1 != (readInt = this.ReadChar()))
            {
                char readChar = (char)readInt;

                switch (readChar)
                {
                    case '\n':
                        this._crDetect = false;
                        return this.getLineAndResetCharBufferPos();
                    case '\r':
                        if (this._crDetect)
                        {
                            return this.getLineAndResetCharBufferPos();
                        }
                        else
                        {
                            this._crDetect = true;
                        }
                        break;
                    default:
                        if (this._crDetect)
                        {
                            this._crDetect = false;
                            string line = this.getLineAndResetCharBufferPos();
                            this.appendToCharBuffer(readChar);
                            return line;
                        }
                        else
                        {
                            this.appendToCharBuffer(readChar);
                        }
                        break;
                }
            }

            string result = this.getLineAndResetCharBufferPos();
            if (readInt == -1 && result.Length == 0 && !this._crDetect)
            {
                return null; // EOF
            }
            this._crDetect = false;
            return result;
        }

        protected override void ResetReader()
        {
            this.resetCharBufferPos();

            base.ResetReader();
        }

        #endregion

        #region Private Methods

        private string getLineAndResetCharBufferPos()
        {
            string result = new string(this.charBuffer, 0, this._charBufferPos);
            this.resetCharBufferPos();
            return result;
        }

        private void appendToCharBuffer(char readChar)
        {
            if (this._charBufferPos < MAX_LINE_LEN)
            {
                this.charBuffer[this._charBufferPos++] = readChar;
            }
        }

        private void resetCharBufferPos()
        {
            this._charBufferPos = 0;
        }

        #endregion
    }
}