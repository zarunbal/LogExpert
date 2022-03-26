using System;
using System.Collections.Generic;
using System.Threading;
using LogExpert.Classes.ILogLineColumnizerCallback;
using NLog;

namespace LogExpert
{
    internal class TimeSpreadCalculator
    {
        #region Fields

        private const int INACTIVITY_TIME = 2000;

        private const int MAX_CONTRAST = 1300;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly EventWaitHandle _calcEvent = new ManualResetEvent(false);
        private readonly Thread _calcThread;
        private readonly ColumnizerCallback _callback;

        private readonly object _diffListLock = new object();
        private readonly EventWaitHandle _lineCountEvent = new ManualResetEvent(false);

        private readonly LogWindow _logWindow;

        // for DoCalc_via_Time
        private double _average;

        private int _contrast = 400;
        private int _displayHeight = 0;
        private bool _enabled;
        private DateTime _endTimestamp;
        private int _lineCount = 0;
        private int _maxDiff;
        private TimeSpan _maxSpan;
        private bool _shouldStop;

        private DateTime _startTimestamp;

        private bool _timeMode = true;

        // for DoCalc
        private int _timePerLine;

        #endregion

        #region cTor

        public TimeSpreadCalculator(LogWindow logWindow)
        {
            _logWindow = logWindow;
            _callback = new ColumnizerCallback(_logWindow);
            _calcThread = new Thread(WorkerFx);
            _calcThread.IsBackground = true;
            _calcThread.Start();
        }

        #endregion

        #region Delegates

        public delegate void CalcDoneEventHandler(object sender, EventArgs e);

        public delegate void StartCalcEventHandler(object sender, EventArgs e);

        #endregion

        #region Events

        public event CalcDoneEventHandler CalcDone;
        public event StartCalcEventHandler StartCalc;

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
                    _calcEvent.Set();
                    _lineCountEvent.Set();
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
                    _calcEvent.Set();
                    _lineCountEvent.Set();
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
                    CalcValuesViaLines(_timePerLine, _maxSpan);
                }
                OnCalcDone(EventArgs.Empty);
            }

            get => _contrast;
        }

        public List<SpreadEntry> DiffList { get; set; } = new List<SpreadEntry>();

        #endregion

        #region Public methods

        public void Stop()
        {
            _shouldStop = true;
            _lineCountEvent.Set();
            _calcThread.Join(300);
            _calcThread.Abort();
            _calcThread.Join();
        }

        public void SetLineCount(int count)
        {
            _lineCount = count;
            if (Enabled)
            {
                _calcEvent.Set();
                _lineCountEvent.Set();
            }
        }

        public void SetDisplayHeight(int height)
        {
            _displayHeight = height;
            if (Enabled)
            {
                _calcEvent.Set();
                _lineCountEvent.Set();
            }
        }

        #endregion

        #region Private Methods

        private void WorkerFx()
        {
            Thread.CurrentThread.Name = "TimeSpreadCalculator Worker";
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (!_shouldStop)
            {
                // wait for wakeup
                _lineCountEvent.WaitOne();

                while (!_shouldStop)
                {
                    // wait for unbusy moments
                    _logger.Debug("TimeSpreadCalculator: wait for unbusy moments");
                    bool signaled = _calcEvent.WaitOne(INACTIVITY_TIME, false);
                    if (signaled == false)
                    {
                        _logger.Debug("TimeSpreadCalculator: unbusy. starting calc.");
                        if (TimeMode)
                        {
                            DoCalc_via_Time();
                        }
                        else
                        {
                            DoCalc();
                        }
                        break;
                    }

                    _logger.Debug("TimeSpreadCalculator: signalled. no calc.");
                    _calcEvent.Reset();
                }
                _lineCountEvent.Reset();
            }
        }

        private void DoCalc()
        {
            OnStartCalc(EventArgs.Empty);
            _logger.Debug("TimeSpreadCalculator.DoCalc() begin");
            if (_callback.GetLineCount() < 1)
            {
                OnCalcDone(EventArgs.Empty);
                _logger.Debug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
                return;
            }
            int lineNum = 0;
            int lastLineNum = _callback.GetLineCount() - 1;
            _startTimestamp = _logWindow.GetTimestampForLineForward(ref lineNum, false);
            _endTimestamp = _logWindow.GetTimestampForLine(ref lastLineNum, false);

            int timePerLineSum = 0;

            if (_startTimestamp != DateTime.MinValue && _endTimestamp != DateTime.MinValue)
            {
                TimeSpan overallSpan = _endTimestamp - _startTimestamp;
                int overallSpanMillis = (int) (overallSpan.Ticks / TimeSpan.TicksPerMillisecond);
                _timePerLine = (int) Math.Round(overallSpanMillis / (double) _lineCount);
                DateTime oldTime = _logWindow.GetTimestampForLineForward(ref lineNum, false);
                int step;
                if (_lineCount > _displayHeight)
                {
                    step = (int) Math.Round(_lineCount / (double) _displayHeight);
                }
                else
                {
                    step = 1;
                }

                _logger.Debug("TimeSpreadCalculator.DoCalc() collecting data for {0} lines with step size {1}", lastLineNum, step);

                List<SpreadEntry> newDiffList = new List<SpreadEntry>();
                List<TimeSpan> maxList = new List<TimeSpan>();
                lineNum++;
                for (int i = lineNum; i < lastLineNum; i += step)
                {
                    int currLineNum = i;
                    DateTime time = _logWindow.GetTimestampForLineForward(ref currLineNum, false);
                    if (time != DateTime.MinValue)
                    {
                        TimeSpan span = time - oldTime;
                        maxList.Add(span);
                        timePerLineSum += (int) (span.Ticks / TimeSpan.TicksPerMillisecond);
                        newDiffList.Add(new SpreadEntry(i, 0, time));
                        oldTime = time;
                        _logger.Debug("TimeSpreadCalculator.DoCalc() time diff {0}", span);
                    }
                }
                if (maxList.Count > 3)
                {
                    maxList.Sort();
                    _maxSpan = maxList[maxList.Count - 3];
                }
                lock (_diffListLock)
                {
                    DiffList = newDiffList;
                    _timePerLine = (int) Math.Round(timePerLineSum / ((double) (lastLineNum + 1) / step));
                    CalcValuesViaLines(_timePerLine, _maxSpan);
                    OnCalcDone(EventArgs.Empty);
                    _logger.Debug("TimeSpreadCalculator.DoCalc() end");
                }
            }
        }

        private void DoCalc_via_Time()
        {
            OnStartCalc(EventArgs.Empty);
            _logger.Debug("TimeSpreadCalculator.DoCalc_via_Time() begin");
            if (_callback.GetLineCount() < 1)
            {
                OnCalcDone(EventArgs.Empty);
                _logger.Debug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
                return;
            }
            int lineNum = 0;
            int lastLineNum = _callback.GetLineCount() - 1;
            _startTimestamp = _logWindow.GetTimestampForLineForward(ref lineNum, false);
            _endTimestamp = _logWindow.GetTimestampForLine(ref lastLineNum, false);

            if (_startTimestamp != DateTime.MinValue && _endTimestamp != DateTime.MinValue)
            {
                TimeSpan overallSpan = _endTimestamp - _startTimestamp;
                long overallSpanMillis = overallSpan.Ticks / TimeSpan.TicksPerMillisecond;
                //int timePerLine = (int)Math.Round((double)overallSpanMillis / (double)this.lineCount);
                DateTime oldTime = _logWindow.GetTimestampForLineForward(ref lineNum, false);
                long step;
                if (overallSpanMillis > _displayHeight)
                {
                    step = (long) Math.Round(overallSpanMillis / (double) _displayHeight);
                }
                else
                {
                    step = 1;
                }

                _logger.Debug("TimeSpreadCalculator.DoCalc_via_Time() time range is {0} ms", overallSpanMillis);

                lineNum = 0;
                DateTime searchTimeStamp = _startTimestamp;
                int oldLineNum = lineNum;
                int loopCount = 0;
                int lineDiffSum = 0;
                int minDiff = int.MaxValue;
                _maxDiff = 0;
                List<int> maxList = new List<int>();
                List<SpreadEntry> newDiffList = new List<SpreadEntry>();
                while (searchTimeStamp.CompareTo(_endTimestamp) <= 0)
                {
                    lineNum = _logWindow.FindTimestampLine_Internal(lineNum, lineNum, lastLineNum, searchTimeStamp, false);
                    if (lineNum < 0)
                    {
                        lineNum = -lineNum;
                    }
                    int lineDiff = lineNum - oldLineNum;

                    _logger.Debug("TimeSpreadCalculator.DoCalc_via_Time() test time {0:HH:mm:ss.fff} line diff={1}", searchTimeStamp, lineDiff);

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
                    _maxDiff = maxList[maxList.Count - 3];
                }
                _average = lineDiffSum / (double) loopCount;
                //double average = maxList[maxList.Count / 2];
                _logger.Debug("Average diff={0} minDiff={1} maxDiff={2}", _average, minDiff, _maxDiff);
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
                    _logger.Debug("TimeSpreadCalculator.DoCalc_via_Time() end");
                }
            }
        }
        
        private DateTime CalcValuesViaLines(int timePerLine, TimeSpan maxSpan)
        {
            DateTime oldTime = DateTime.MinValue;
            if (DiffList.Count > 0)
            {
                oldTime = DiffList[0].timestamp;
                foreach (SpreadEntry entry in DiffList)
                {
                    TimeSpan span = entry.timestamp - oldTime;
                    double diffFromAverage = (int) (span.Ticks / TimeSpan.TicksPerMillisecond) - timePerLine;
                    if (diffFromAverage < 0)
                    {
                        diffFromAverage = 0;
                    }
                    int value = (int) (diffFromAverage / (timePerLine / TimeSpan.TicksPerMillisecond)
                                       * _contrast);
                    entry.value = 255 - value;
                    oldTime = entry.timestamp;
                }
            }
            return oldTime;
        }

        private void CalcValuesViaTime(int maxDiff, double average)
        {
            foreach (SpreadEntry entry in DiffList)
            {
                int lineDiff = entry.diff;
                double diffFromAverage = entry.diff - average;
                if (diffFromAverage < 0)
                {
                    diffFromAverage = 0;
                }
                int value = (int) (diffFromAverage / maxDiff * _contrast);
                entry.value = 255 - value;
                _logger.Debug("TimeSpreadCalculator.DoCalc() test time {0:HH:mm:ss.fff} line diff={1} value={2}", entry.timestamp, lineDiff, value);
            }
        }

        private void OnCalcDone(EventArgs e)
        {
            CalcDone?.Invoke(this, e);
        }

        private void OnStartCalc(EventArgs e)
        {
            StartCalc?.Invoke(this, e);
        }

        #endregion

        public class SpreadEntry
        {
            #region Fields

            public int diff;
            public int lineNum;
            public DateTime timestamp;
            public int value;

            #endregion

            #region cTor

            public SpreadEntry(int lineNum, int diff, DateTime timestamp)
            {
                this.lineNum = lineNum;
                this.diff = diff;
                this.timestamp = timestamp;
            }

            #endregion
        }
    }
}