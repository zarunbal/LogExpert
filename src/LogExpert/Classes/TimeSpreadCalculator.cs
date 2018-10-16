using System;
using System.Collections.Generic;
using System.Threading;
using NLog;

namespace LogExpert
{
    internal class TimeSpreadCalculator
    {
        #region Delegates

        public delegate void CalcDoneEventHandler(object sender, EventArgs e);

        public delegate void StartCalcEventHandler(object sender, EventArgs e);

        #endregion

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Static/Constants

        private const int INACTIVITY_TIME = 2000;

        private const int MAX_CONTRAST = 1300;

        #endregion

        #region Private Fields

        private readonly EventWaitHandle calcEvent = new ManualResetEvent(false);
        private readonly Thread calcThread;
        private readonly LogWindow.ColumnizerCallback callback;

        private readonly object diffListLock = new object();
        private readonly EventWaitHandle lineCountEvent = new ManualResetEvent(false);

        private readonly LogWindow logWindow;

        // for DoCalc_via_Time
        private double average;

        private int contrast = 400;
        private int displayHeight;
        private bool enabled;
        private DateTime endTimestamp;
        private int lineCount;
        private int maxDiff;
        private TimeSpan maxSpan;
        private bool shouldStop;

        private DateTime startTimestamp;

        private bool timeMode = true;

        // for DoCalc
        private int timePerLine;

        #endregion

        #region Public Events

        public event CalcDoneEventHandler CalcDone;
        public event StartCalcEventHandler StartCalc;

        #endregion

        #region Ctor

        public TimeSpreadCalculator(LogWindow logWindow)
        {
            this.logWindow = logWindow;
            callback = new LogWindow.ColumnizerCallback(this.logWindow);
            calcThread = new Thread(WorkerFx);
            calcThread.IsBackground = true;
            calcThread.Start();
        }

        #endregion

        #region Properties / Indexers

        public int Contrast
        {
            get => contrast;
            set
            {
                contrast = value;
                if (contrast < 0)
                {
                    contrast = 0;
                }
                else if (contrast > MAX_CONTRAST)
                {
                    contrast = MAX_CONTRAST;
                }

                if (TimeMode)
                {
                    CalcValuesViaTime(maxDiff, average);
                }
                else
                {
                    CalcValuesViaLines(timePerLine, maxSpan);
                }

                OnCalcDone(new EventArgs());
            }
        }

        public List<SpreadEntry> DiffList { get; set; } = new List<SpreadEntry>();

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                if (enabled)
                {
                    calcEvent.Set();
                    lineCountEvent.Set();
                }
            }
        }

        public bool TimeMode
        {
            get => timeMode;
            set
            {
                timeMode = value;
                if (enabled)
                {
                    calcEvent.Set();
                    lineCountEvent.Set();
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetDisplayHeight(int height)
        {
            displayHeight = height;
            if (Enabled)
            {
                calcEvent.Set();
                lineCountEvent.Set();
            }
        }

        public void SetLineCount(int count)
        {
            lineCount = count;
            if (Enabled)
            {
                calcEvent.Set();
                lineCountEvent.Set();
            }
        }

        public void Stop()
        {
            shouldStop = true;
            lineCountEvent.Set();
            calcThread.Join(300);
            calcThread.Abort();
            calcThread.Join();
        }

        #endregion

        #region Event handling Methods

        private void OnCalcDone(EventArgs e)
        {
            if (CalcDone != null)
            {
                CalcDone(this, e);
            }
        }

        private void OnStartCalc(EventArgs e)
        {
            if (StartCalc != null)
            {
                StartCalc(this, e);
            }
        }

        #endregion

        #region Event raising Methods

        private void DoCalc()
        {
            OnStartCalc(new EventArgs());
            _logger.Debug("TimeSpreadCalculator.DoCalc() begin");
            if (callback.GetLineCount() < 1)
            {
                OnCalcDone(new EventArgs());
                _logger.Debug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
                return;
            }

            int lineNum = 0;
            int lastLineNum = callback.GetLineCount() - 1;
            startTimestamp = logWindow.GetTimestampForLineForward(ref lineNum, false);
            endTimestamp = logWindow.GetTimestampForLine(ref lastLineNum, false);

            int timePerLineSum = 0;

            if (startTimestamp != DateTime.MinValue && endTimestamp != DateTime.MinValue)
            {
                TimeSpan overallSpan = endTimestamp - startTimestamp;
                int overallSpanMillis = (int)(overallSpan.Ticks / TimeSpan.TicksPerMillisecond);
                timePerLine = (int)Math.Round(overallSpanMillis / (double)lineCount);
                DateTime oldTime = logWindow.GetTimestampForLineForward(ref lineNum, false);
                int step;
                if (lineCount > displayHeight)
                {
                    step = (int)Math.Round(lineCount / (double)displayHeight);
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
                    DateTime time = logWindow.GetTimestampForLineForward(ref currLineNum, false);
                    if (time != DateTime.MinValue)
                    {
                        TimeSpan span = time - oldTime;
                        maxList.Add(span);
                        timePerLineSum += (int)(span.Ticks / TimeSpan.TicksPerMillisecond);
                        newDiffList.Add(new SpreadEntry(i, 0, time));
                        oldTime = time;
                        _logger.Debug("TimeSpreadCalculator.DoCalc() time diff {0}", span);
                    }
                }

                if (maxList.Count > 3)
                {
                    maxList.Sort();
                    maxSpan = maxList[maxList.Count - 3];
                }

                lock (diffListLock)
                {
                    DiffList = newDiffList;
                    timePerLine = (int)Math.Round(timePerLineSum / ((double)(lastLineNum + 1) / step));
                    CalcValuesViaLines(timePerLine, maxSpan);
                    OnCalcDone(new EventArgs());
                    _logger.Debug("TimeSpreadCalculator.DoCalc() end");
                }
            }
        }

        private void DoCalc_via_Time()
        {
            OnStartCalc(new EventArgs());
            _logger.Debug("TimeSpreadCalculator.DoCalc_via_Time() begin");
            if (callback.GetLineCount() < 1)
            {
                OnCalcDone(new EventArgs());
                _logger.Debug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
                return;
            }

            int lineNum = 0;
            int lastLineNum = callback.GetLineCount() - 1;
            startTimestamp = logWindow.GetTimestampForLineForward(ref lineNum, false);
            endTimestamp = logWindow.GetTimestampForLine(ref lastLineNum, false);

            if (startTimestamp != DateTime.MinValue && endTimestamp != DateTime.MinValue)
            {
                TimeSpan overallSpan = endTimestamp - startTimestamp;
                long overallSpanMillis = overallSpan.Ticks / TimeSpan.TicksPerMillisecond;


// int timePerLine = (int)Math.Round((double)overallSpanMillis / (double)this.lineCount);
                DateTime oldTime = logWindow.GetTimestampForLineForward(ref lineNum, false);
                long step;
                if (overallSpanMillis > displayHeight)
                {
                    step = (long)Math.Round(overallSpanMillis / (double)displayHeight);
                }
                else
                {
                    step = 1;
                }

                _logger.Debug("TimeSpreadCalculator.DoCalc_via_Time() time range is {0} ms", overallSpanMillis);

                lineNum = 0;
                DateTime searchTimeStamp = startTimestamp;
                int oldLineNum = lineNum;
                int loopCount = 0;
                int lineDiffSum = 0;
                int minDiff = int.MaxValue;
                maxDiff = 0;
                List<int> maxList = new List<int>();
                List<SpreadEntry> newDiffList = new List<SpreadEntry>();
                while (searchTimeStamp.CompareTo(endTimestamp) <= 0)
                {
                    lineNum = logWindow.FindTimestampLine_Internal(lineNum, lineNum, lastLineNum, searchTimeStamp,
                        false);
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

                        if (lineDiff > maxDiff)
                        {
                            maxDiff = lineDiff;
                        }

                        maxList.Add(lineDiff);
                        loopCount++;
                    }

                    searchTimeStamp = searchTimeStamp.AddMilliseconds(step);
                    oldLineNum = lineNum;


// lineNum++;
                }

                if (maxList.Count > 3)
                {
                    maxList.Sort();
                    maxDiff = maxList[maxList.Count - 3];
                }

                average = lineDiffSum / (double)loopCount;


// double average = maxList[maxList.Count / 2];
                _logger.Debug("Average diff={0} minDiff={1} maxDiff={2}", average, minDiff, maxDiff);
                lock (diffListLock)
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
                    CalcValuesViaTime(maxDiff, average);
                    OnCalcDone(new EventArgs());
                    _logger.Debug("TimeSpreadCalculator.DoCalc_via_Time() end");
                }
            }
        }

        #endregion

        #region Private Methods

        private DateTime CalcValuesViaLines(int timePerLine, TimeSpan maxSpan)
        {
            DateTime oldTime = DateTime.MinValue;
            if (DiffList.Count > 0)
            {
                oldTime = DiffList[0].timestamp;
                foreach (SpreadEntry entry in DiffList)
                {
                    TimeSpan span = entry.timestamp - oldTime;
                    double diffFromAverage = (int)(span.Ticks / TimeSpan.TicksPerMillisecond) - timePerLine;
                    if (diffFromAverage < 0)
                    {
                        diffFromAverage = 0;
                    }

                    int value = (int)(diffFromAverage / (timePerLine / TimeSpan.TicksPerMillisecond)
                                      * contrast);
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

                int value = (int)(diffFromAverage / maxDiff * contrast);
                entry.value = 255 - value;
                _logger.Debug("TimeSpreadCalculator.DoCalc() test time {0:HH:mm:ss.fff} line diff={1} value={2}", entry.timestamp, lineDiff, value);
            }
        }

        private void WorkerFx()
        {
            Thread.CurrentThread.Name = "TimeSpreadCalculator Worker";
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (!shouldStop)
            {
                // wait for wakeup
                lineCountEvent.WaitOne();

                while (!shouldStop)
                {
                    // wait for unbusy moments
                    _logger.Debug("TimeSpreadCalculator: wait for unbusy moments");
                    bool signalled = calcEvent.WaitOne(INACTIVITY_TIME, false);
                    if (!signalled)
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
                    calcEvent.Reset();
                }

                lineCountEvent.Reset();
            }
        }

        #endregion

        #region Nested type: SpreadEntry

        public class SpreadEntry
        {
            #region Private Fields

            public int diff;
            public int lineNum;
            public DateTime timestamp;
            public int value;

            #endregion

            #region Ctor

            public SpreadEntry(int lineNum, int diff, DateTime timestamp)
            {
                this.lineNum = lineNum;
                this.diff = diff;
                this.timestamp = timestamp;
            }

            #endregion
        }

        #endregion
    }
}
