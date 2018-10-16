using System.IO;

namespace LogExpert.Classes.Log
{
    internal static class PositionAwareStreamReaderFactory
    {
        internal static ILogStreamReader CreateStreamReader(Stream stream, EncodingOptions encodingOptions, bool useSystemReader)
        {
            if (useSystemReader)
            {
                return new PositionAwareStreamReader(stream, encodingOptions);
            }

            return new PositionAwareStreamReaderLegacy(stream, encodingOptions);
        }
    }
}
