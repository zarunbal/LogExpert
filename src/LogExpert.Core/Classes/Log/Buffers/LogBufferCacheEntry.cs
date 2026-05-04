namespace LogExpert.Core.Classes.Log.Buffers;

public class LogBufferCacheEntry
{
    private long _lastUseTimeStamp;

    #region cTor

    public LogBufferCacheEntry ()
    {
        Touch();
    }

    #endregion

    #region Properties

    public LogBuffer LogBuffer { get; set; }

    public long LastUseTimeStamp => Interlocked.Read(ref _lastUseTimeStamp);

    #endregion

    #region Public methods

    public void Touch ()
    {
        _ = Interlocked.Exchange(ref _lastUseTimeStamp, Environment.TickCount64);
    }

    #endregion
}