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

        internal static ILogStreamReader CreateStreamReader(Stream stream, EncodingOptions encodingOptions, PositionAwareStreamReaderImplementation implementation)
        {
            ILogStreamReader output = null;
            switch (implementation)
            {
                case PositionAwareStreamReaderImplementation.Legacy:
                    output = new PositionAwareStreamReaderLegacy(stream, encodingOptions);

                    break;
                case PositionAwareStreamReaderImplementation.NoLimit:
                    output = new PositionAwareStreamReaderNoLimit(stream, encodingOptions);
                    break;
                case PositionAwareStreamReaderImplementation.Default:
                default:
                    output = new PositionAwareStreamReader(stream, encodingOptions);
                    break;
            }

            return output;
        }

        #endregion
    }
}