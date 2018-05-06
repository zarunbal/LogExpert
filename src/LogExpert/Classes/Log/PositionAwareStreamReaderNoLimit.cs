using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogExpert.Classes.Log
{
    public class PositionAwareStreamReaderNoLimit : PositionAwareStreamReader
    {
        #region cTor

        public PositionAwareStreamReaderNoLimit(Stream stream, EncodingOptions encodingOptions) : base(stream, encodingOptions)
        {
        }

        #endregion

        #region Public methods

        public override string ReadLine()
        {
            if (_newLineSequenceLength == 0)
            {
                _newLineSequenceLength = GuessNewLineSequenceLength();
            }

            string line = _reader.ReadLine();
            if (line != null)
            {
                _pos += Encoding.GetByteCount(line);

                _pos += _newLineSequenceLength;
            }

            return line;
        }

        #endregion
    }
}