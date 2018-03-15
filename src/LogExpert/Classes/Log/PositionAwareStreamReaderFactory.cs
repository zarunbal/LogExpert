using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogExpert.Classes.Log
{
    internal static class PositionAwareStreamReaderFactory
    {
        #region Internals

        internal static ILogStreamReader CreateStreamReader(Stream stream, EncodingOptions encodingOptions, bool useSystemReader)
        {
            if (useSystemReader)
            {
                return new PositionAwareStreamReader(stream, encodingOptions);
            }
            else
            {
                return new PositionAwareStreamReaderLegacy(stream, encodingOptions);
            }
        }

        #endregion
    }
}