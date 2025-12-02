using System.IO.Pipelines;
using System.Text;

using LogExpert.Core.Entities;

namespace LogExpert.Core.Classes.Log;

public class PositionAwareStreamReaderPipeline : LogStreamReaderBase
{
    private const int DEFAULT_BYTE_BUFFER_SIZE = 64 * 1024; // 64 KB
    private const int MINIMUM_READ_AHEAD_SIZE = 4 * 1024; // 4 KB
    private const int DEFAULT_CHANNEL_CAPACITY = 128; // Number of line segments

    private static readonly Encoding[] _preambleEncodings =
    [
        Encoding.UTF8,
        Encoding.Unicode,
        Encoding.BigEndianUnicode,
        Encoding.UTF32
    ];

    private readonly StreamPipeReaderOptions _streamPipeReaderOptions = new(bufferSize: DEFAULT_BYTE_BUFFER_SIZE, minimumReadSize: MINIMUM_READ_AHEAD_SIZE, leaveOpen: true);
    private readonly int _maximumLineLength;
    private readonly Lock _reconfigureLock = new();
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private readonly int _byteBufferSize;
    private readonly int _charBufferSize;
    private readonly long _preambleLength;

    private readonly PipeReader _pipeReader;
    private readonly PipeWriter _pipeWriter;

    //private Channel<LineSegment> _channel;
    private CancellationTokenSource _cts;
    private Task _producerTask;
    private bool _isDisposed;
    private long _position;


    public PositionAwareStreamReaderPipeline (Stream stream, EncodingOptions encodingOptions, int maximumLineLength)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream must support reading.", nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must support seeking.", nameof(stream));
        }

        if (maximumLineLength <= 0)
        {
            maximumLineLength = 1024;
        }

        _maximumLineLength = maximumLineLength;
        _byteBufferSize = DEFAULT_BYTE_BUFFER_SIZE;
        var (length, detectedEncoding) = DetectPreambleLength(stream);
        _preambleLength = length;
        _encoding = DetermineEncoding(encodingOptions, detectedEncoding);


        _pipeReader = PipeReader.Create(stream, _streamPipeReaderOptions);


        _stream = stream;

        _charBufferSize = Math.Max(_encoding.GetMaxCharCount(_byteBufferSize), _maximumLineLength + 2);

        //RestartPipelineInternal(0);
    }

    public override long Position { get; set; }

    public override bool IsBufferComplete { get; }

    public override Encoding Encoding { get; }

    public override bool IsDisposed { get; protected set; }

    public override int ReadChar ()
    {
        throw new NotImplementedException();
    }

    public override string ReadLine ()
    {
        throw new NotImplementedException();
    }

    protected override void Dispose (bool disposing)
    {
        throw new NotImplementedException();
    }

    private static Encoding DetermineEncoding (EncodingOptions options, Encoding detectedEncoding)
    {
        return options?.Encoding != null
            ? options.Encoding
            : detectedEncoding ?? options?.DefaultEncoding ?? Encoding.Default;
    }

    private static (int length, Encoding? detectedEncoding) DetectPreambleLength (Stream stream)
    {
        if (!stream.CanSeek)
        {
            return (0, null);
        }

        var originalPos = stream.Position;
        var buffer = new byte[4];
        _ = stream.Seek(0, SeekOrigin.Begin);
        var readBytes = stream.Read(buffer, 0, buffer.Length);
        _ = stream.Seek(originalPos, SeekOrigin.Begin);

        if (readBytes >= 2)
        {
            foreach (var encoding in _preambleEncodings)
            {
                var preamble = encoding.GetPreamble();
                var fail = false;
                for (var i = 0; i < readBytes && i < preamble.Length; ++i)
                {
                    if (buffer[i] != preamble[i])
                    {
                        fail = true;
                        break;
                    }
                }

                if (!fail)
                {
                    return (preamble.Length, encoding);
                }
            }
        }

        return (0, null);
    }
}
