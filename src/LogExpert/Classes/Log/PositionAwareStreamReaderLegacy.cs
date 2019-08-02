using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogExpert.Classes.Log
{
    public class PositionAwareStreamReaderLegacy : PositionAwareStreamReaderBase
    {
        #region Fields

        private readonly char[] charBuffer = new char[MAX_LINE_LEN];

        private int _charBufferPos = 0;

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
            bool crDetect = false;

            while (-1 != (readInt = this.ReadChar()))
            {
                char readChar = (char)readInt;

                switch (readChar)
                {
                    case '\n':
                        return this.getLineAndResetCharBufferPos();
                    case '\r':
                        if (crDetect)
                        {
                            // double \r\r should return line ???
                            return this.getLineAndResetCharBufferPos();
                        }
                        else
                        {
                            crDetect = true;
                        }
                        break;
                    default:
                        this.appendToCharBuffer(readChar);
                        crDetect = false;
                        break;
                }
            }

            string result = this.getLineAndResetCharBufferPos();
            if (readInt == -1 && result.Length == 0)
            {
                return null; // EOF
            }
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