using LogExpert.Core.Callback;
using LogExpert.Core.Classes;
using LogExpert.Core.Interface;

using NLog;

namespace LogExpert.UI.Controls.LogWindow;

internal class TimeSpreadCalculator
{
    #region Fields

    private const int INACTIVITY_TIME = 2000;

    private const int MAX_CONTRAST = 1300;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly EventWaitHandle _calcEvent = new ManualResetEvent(false);
    private readonly ColumnizerCallback _callback;

    private readonly object _diffListLock = new();
    private readonly EventWaitHandle _lineCountEvent = new ManualResetEvent(false);

    //TODO Refactor that it does not need LogWindow
    private readonly ILogWindow _logWindow;

    // for DoCalc_via_Time
    private double _average;

    private int _contrast = 400;

    private int _displayHeight;

    private bool _enabled;

    private DateTime _endTimestamp;
    private int _lineCount;
    private int _maxDiff;
    private bool _shouldStop;
    private readonly CancellationTokenSource _cts = new();
    private DateTime _startTimestamp;

    private bool _timeMode = true;

    // for DoCalc
    private int _timePerLine;

    #endregion

    #region cTor

    public TimeSpreadCalculator (ILogWindow logWindow)
    {
        _logWindow = logWindow;
        _callback = new ColumnizerCallback(_logWindow);

        _ = Task.Run(WorkerFx, _cts.Token);
    }

    #endregion

    #region Events

    public EventHandler<EventArgs> CalcDone;
    public EventHandler<EventArgs> StartCalc;

    #endregion

    #region Properties

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (_enabled)
            {
                _ = _calcEvent.Set();
                _ = _lineCountEvent.Set();
            }
        }
    }

    public bool TimeMode
    {
        get => _timeMode;
        set
        {
            _timeMode = value;
            if (_enabled)
            {
                _ = _calcEvent.Set();
                _ = _lineCountEvent.Set();
            }
        }
    }

    public int Contrast
    {
        set
        {
            _contrast = value;
            if (_contrast < 0)
            {
                _contrast = 0;
            }
            else if (_contrast > MAX_CONTRAST)
            {
                _contrast = MAX_CONTRAST;
            }

            if (TimeMode)
            {
                CalcValuesViaTime(_maxDiff, _average);
            }
            else
            {
                _ = CalcValuesViaLines(_timePerLine);
            }

            OnCalcDone(EventArgs.Empty);
        }

        get => _contrast;
    }

    public List<SpreadEntry> DiffList { get; set; } = [];

    #endregion

    #region Public methods

    public void Stop ()
    {
        _shouldStop = true;
        _ = _lineCountEvent.Set();

        _cts.Cancel();
    }

    public void SetLineCount (int count)
    {
        _lineCount = count;
        if (Enabled)
        {
            _ = _calcEvent.Set();
            _ = _lineCountEvent.Set();
        }
    }

    public void SetDisplayHeight (int height)
    {
        _displayHeight = height;
        if (Enabled)
        {
            _ = _calcEvent.Set();
            _ = _lineCountEvent.Set();
        }
    }

    #endregion

    #region Private Methods

    private void WorkerFx ()
    {
        //Thread.CurrentThread.Name = "TimeSpreadCalculator Worker";
        //Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

        while (!_shouldStop)
        {
            // wait for wakeup
            _ = _lineCountEvent.WaitOne();

            while (!_shouldStop)
            {
                // wait for unbusy moments
                var signaled = _calcEvent.WaitOne(INACTIVITY_TIME, false);
                if (!signaled)
                {
                    if (TimeMode)
                    {
                        DoCalcViaTime();
                    }
                    else
                    {
                        DoCalc();
                    }

                    break;
                }

                _ = _calcEvent.Reset();
            }

            _ = _lineCountEvent.Reset();
        }
    }

    private void DoCalc ()
    {
        OnStartCalc(EventArgs.Empty);

        if (_callback.GetLineCount() < 1)
        {
            OnCalcDone(EventArgs.Empty);
            return;
        }

        var lineNum = 0;
        var lastLineNum = _callback.GetLineCount() - 1;
        _startTimestamp = _logWindow.GetTimestampForLineForward(ref lineNum, false);
        _endTimestamp = _logWindow.GetTimestampForLine(ref lastLineNum, false);

        var timePerLineSum = 0;

        if (_startTimestamp != DateTime.MinValue && _endTimestamp != DateTime.MinValue)
        {
            var overallSpan = _endTimestamp - _startTimestamp;
            var overallSpanMillis = (int)(overallSpan.Ticks / TimeSpan.TicksPerMillisecond);
            _timePerLine = (int)Math.Round(overallSpanMillis / (double)_lineCount);
            var oldTime = _logWindow.GetTimestampForLineForward(ref lineNum, false);
            var step = _lineCount > _displayHeight
                ? (int)Math.Round(_lineCount / (double)_displayHeight)
                : 1;

            //_logger.Debug($"Collecting data for {lastLineNum} lines with step size {step}"));

            List<SpreadEntry> newDiffList = [];
            List<TimeSpan> maxList = [];
            lineNum++;

            for (var i = lineNum; i < lastLineNum; i += step)
            {
                var currLineNum = i;
                var time = _logWindow.GetTimestampForLineForward(ref currLineNum, false);
                if (time != DateTime.MinValue)
                {
                    var span = time - oldTime;
                    maxList.Add(span);
                    timePerLineSum += (int)(span.Ticks / TimeSpan.TicksPerMillisecond);
                    newDiffList.Add(new SpreadEntry(i, 0, time));
                    oldTime = time;
                }
            }

            if (maxList.Count > 3)
            {
                maxList.Sort();
            }

            lock (_diffListLock)
            {
                DiffList = newDiffList;
                _timePerLine = (int)Math.Round(timePerLineSum / ((double)(lastLineNum + 1) / step));
                _ = CalcValuesViaLines(_timePerLine);
                OnCalcDone(EventArgs.Empty);
            }
        }
    }

    //TODO Refactor this method
    private void DoCalcViaTime ()
    {
        OnStartCalc(EventArgs.Empty);

        if (_callback.GetLineCount() < 1)
        {
            OnCalcDone(EventArgs.Empty);
            //_logger.Debug($"End because of line count < 1");
            return;
        }

        var lineNum = 0;
        var lastLineNum = _callback.GetLineCount() - 1;
        _startTimestamp = _logWindow.GetTimestampForLineForward(ref lineNum, false);
        _endTimestamp = _logWindow.GetTimestampForLine(ref lastLineNum, false);

        if (_startTimestamp != DateTime.MinValue && _endTimestamp != DateTime.MinValue)
        {
            var overallSpan = _endTimestamp - _startTimestamp;
            var overallSpanMillis = overallSpan.Ticks / TimeSpan.TicksPerMillisecond;
            //int timePerLine = (int)Math.Round((double)overallSpanMillis / (double)this.lineCount);

            var step = overallSpanMillis > _displayHeight ? (long)Math.Round(overallSpanMillis / (double)_displayHeight) : 1;

            //_logger.Debug($"Time range is {overallSpanMillis} ms");

            lineNum = 0;
            var searchTimeStamp = _startTimestamp;
            var oldLineNum = lineNum;
            var loopCount = 0;
            var lineDiffSum = 0;
            var minDiff = int.MaxValue;
            _maxDiff = 0;
            List<int> maxList = [];
            List<SpreadEntry> newDiffList = [];

            while (searchTimeStamp.CompareTo(_endTimestamp) <= 0)
            {
                lineNum = _logWindow.FindTimestampLineInternal(lineNum, lineNum, lastLineNum, searchTimeStamp, false);
                if (lineNum < 0)
                {
                    lineNum = -lineNum;
                }

                var lineDiff = lineNum - oldLineNum;

                //var timestamp = $"{searchTimeStamp:HH:mm:ss.fff}";
                //_logger.Debug($"Test time {timestamp} line diff={lineDiff}"));

                if (lineDiff >= 0)
                {
                    lineDiffSum += lineDiff;
                    newDiffList.Add(new SpreadEntry(lineNum, lineDiff, searchTimeStamp));

                    if (lineDiff < minDiff)
                    {
                        minDiff = lineDiff;
                    }

                    if (lineDiff > _maxDiff)
                    {
                        _maxDiff = lineDiff;
                    }

                    maxList.Add(lineDiff);
                    loopCount++;
                }

                searchTimeStamp = searchTimeStamp.AddMilliseconds(step);
                oldLineNum = lineNum;
                //lineNum++;
            }

            if (maxList.Count > 3)
            {
                maxList.Sort();
                _maxDiff = maxList[^3];
            }

            _average = lineDiffSum / (double)loopCount;
            //double average = maxList[maxList.Count / 2];
            //_logger.Debug($"Average diff={_average} minDiff={minDiff} maxDiff={_maxDiff}");

            lock (_diffListLock)
            {
                if (newDiffList.Count > 0)
                {
                    newDiffList.RemoveAt(0);
                }

                if (newDiffList.Count > 0)
                {
                    newDiffList.RemoveAt(0);
                }

                DiffList = newDiffList;
                CalcValuesViaTime(_maxDiff, _average);
                OnCalcDone(EventArgs.Empty);
            }
        }
    }

    private DateTime CalcValuesViaLines (int timePerLine)
    {
        var oldTime = DateTime.MinValue;

        if (DiffList.Count > 0)
        {
            oldTime = DiffList[0].Timestamp;

            foreach (var entry in DiffList)
            {
                var span = entry.Timestamp - oldTime;
                double diffFromAverage = (int)(span.Ticks / TimeSpan.TicksPerMillisecond) - timePerLine;

                if (diffFromAverage < 0)
                {
                    diffFromAverage = 0;
                }

                var value = (int)(diffFromAverage / (timePerLine / TimeSpan.TicksPerMillisecond) * _contrast);
                entry.Value = 255 - value;
                oldTime = entry.Timestamp;
            }
        }

        return oldTime;
    }

    private void CalcValuesViaTime (int maxDiff, double average)
    {
        foreach (var entry in DiffList)
        {
            //var lineDiff = entry.Diff;
            var diffFromAverage = entry.Diff - average;

            if (diffFromAverage < 0)
            {
                diffFromAverage = 0;
            }

            var value = (int)(diffFromAverage / maxDiff * _contrast);
            entry.Value = 255 - value;

            //var timestamp = $"{entry.Timestamp:HH:mm:ss.fff}";
            //_logger.Debug($"Test time {timestamp} line diff={lineDiff} value={value}"));
        }
    }

    private void OnCalcDone (EventArgs e)
    {
        CalcDone?.Invoke(this, e);
    }

    private void OnStartCalc (EventArgs e)
    {
        StartCalc?.Invoke(this, e);
    }

    #endregion
}