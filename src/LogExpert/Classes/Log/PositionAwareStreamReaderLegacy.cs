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

        #endregion

        #region cTor

        public PositionAwareStreamReaderLegacy(Stream stream, EncodingOptions encodingOptions) : base(stream, encodingOptions)
        {
        }

        #endregion

        #region Public methods

        public override string ReadLine()
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
                        switch (_state)
                        {
                            case 0:
                                _state = 1;
                                break;
                            case 1:
                                result = GetLineAndResetBuilder();
                                return result;
                        }

                        break;
                    case '\n':
                        switch (_state)
                        {
                            case 0:
                            // fall through
                            case 1:
                                result = GetLineAndResetBuilder();
                                return result;
                        }

                        break;
                    default:
                        switch (_state)
                        {
                            case 0:
                                appendToBuilder(readChar);
                                break;
                            case 1:
                                appendToBuilder(readChar);
                                _state = 0;
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

        #endregion

        #region Private Methods

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
            string result = new string(charBuffer, 0, _charBufferPos);
            NewBuilder();
            _state = 0;
            return result;
        }

        private void appendToBuilder(char readChar)
        {
            if (_charBufferPos >= MAX_LINE_LEN)
            {
                return;
            }

            charBuffer[_charBufferPos++] = readChar;
        }

        #endregion
    }
}