using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
    internal class TimeSpreadCalculator
    {
        #region Fields

        private const int INACTIVITY_TIME = 2000;

        private const int MAX_CONTRAST = 1300;

        // for DoCalc_via_Time
        private double average;

        private readonly EventWaitHandle calcEvent = new ManualResetEvent(false);
        private readonly Thread calcThread = null;
        private readonly LogWindow.ColumnizerCallback callback;
        private int contrast = 400;

        private readonly object diffListLock = new object();
        private int displayHeight = 0;
        private bool enabled;
        private DateTime endTimestamp;
        private int lineCount = 0;
        private readonly EventWaitHandle lineCountEvent = new ManualResetEvent(false);

        private readonly LogWindow logWindow;
        private int maxDiff;
        private TimeSpan maxSpan;
        private bool shouldStop = false;

        private DateTime startTimestamp;

        private bool timeMode = true;

        // for DoCalc
        private int timePerLine;

        #endregion

        #region cTor

        public TimeSpreadCalculator(LogWindow logWindow)
        {
            this.logWindow = logWindow;
            this.callback = new LogWindow.ColumnizerCallback(this.logWindow);
            calcThread = new Thread(new ThreadStart(WorkerFx));
            calcThread.IsBackground = true;
            calcThread.Start();
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
            get { return enabled; }
            set
            {
                enabled = value;
                if (enabled)
                {
                    this.calcEvent.Set();
                    this.lineCountEvent.Set();
                }
                else
                {
                }
            }
        }

        public bool TimeMode
        {
            get { return timeMode; }
            set
            {
                timeMode = value;
                if (enabled)
                {
                    this.calcEvent.Set();
                    this.lineCountEvent.Set();
                }
            }
        }

        public int Contrast
        {
            set
            {
                this.contrast = value;
                if (this.contrast < 0)
                {
                    this.contrast = 0;
                }
                else if (this.contrast > MAX_CONTRAST)
                {
                    this.contrast = MAX_CONTRAST;
                }

                if (TimeMode)
                {
                    CalcValuesViaTime(this.maxDiff, this.average);
                }
                else
                {
                    CalcValuesViaLines(this.timePerLine, this.maxSpan);
                }
                OnCalcDone(new EventArgs());
            }

            get { return this.contrast; }
        }

        public List<SpreadEntry> DiffList { get; set; } = new List<SpreadEntry>();

        #endregion

        #region Public methods

        public void Stop()
        {
            this.shouldStop = true;
            this.lineCountEvent.Set();
            this.calcThread.Join(300);
            this.calcThread.Abort();
            this.calcThread.Join();
        }

        public void SetLineCount(int count)
        {
            this.lineCount = count;
            if (Enabled)
            {
                this.calcEvent.Set();
                this.lineCountEvent.Set();
            }
        }

        public void SetDisplayHeight(int height)
        {
            this.displayHeight = height;
            if (Enabled)
            {
                this.calcEvent.Set();
                this.lineCountEvent.Set();
            }
        }

        #endregion

        #region Private Methods

        private void WorkerFx()
        {
            Thread.CurrentThread.Name = "TimeSpreadCalculator Worker";
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            while (!this.shouldStop)
            {
                // wait for wakeup
                this.lineCountEvent.WaitOne();

                while (!this.shouldStop)
                {
                    // wait for unbusy moments
                    Logger.logDebug("TimeSpreadCalculator: wait for unbusy moments");
                    bool signalled = this.calcEvent.WaitOne(INACTIVITY_TIME, false);
                    if (!signalled)
                    {
                        Logger.logDebug("TimeSpreadCalculator: unbusy. starting calc.");
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
                    else
                    {
                        Logger.logDebug("TimeSpreadCalculator: signalled. no calc.");
                    }
                    this.calcEvent.Reset();
                }
                this.lineCountEvent.Reset();
            }
        }

        private void DoCalc()
        {
            OnStartCalc(new EventArgs());
            Logger.logDebug("TimeSpreadCalculator.DoCalc() begin");
            if (this.callback.GetLineCount() < 1)
            {
                OnCalcDone(new EventArgs());
                Logger.logDebug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
                return;
            }
            int lineNum = 0;
            int lastLineNum = this.callback.GetLineCount() - 1;
            this.startTimestamp = this.logWindow.GetTimestampForLineForward(ref lineNum, false);
            this.endTimestamp = this.logWindow.GetTimestampForLine(ref lastLineNum, false);

            int timePerLineSum = 0;

            if (this.startTimestamp != DateTime.MinValue && this.endTimestamp != DateTime.MinValue)
            {
                TimeSpan overallSpan = this.endTimestamp - this.startTimestamp;
                int overallSpanMillis = (int) (overallSpan.Ticks / TimeSpan.TicksPerMillisecond);
                this.timePerLine = (int) Math.Round((double) overallSpanMillis / (double) this.lineCount);
                DateTime oldTime = this.logWindow.GetTimestampForLineForward(ref lineNum, false);
                int step;
                if (this.lineCount > this.displayHeight)
                {
                    step = (int) Math.Round((double) this.lineCount / (double) this.displayHeight);
                }
                else
                {
                    step = 1;
                }

                Logger.logDebug("TimeSpreadCalculator.DoCalc() collecting data for " + lastLineNum +
                                " lines with step size " + step);

                List<SpreadEntry> newDiffList = new List<SpreadEntry>();
                List<TimeSpan> maxList = new List<TimeSpan>();
                lineNum++;
                for (int i = lineNum; i < lastLineNum; i += step)
                {
                    int currLineNum = i;
                    DateTime time = this.logWindow.GetTimestampForLineForward(ref currLineNum, false);
                    if (time != DateTime.MinValue)
                    {
                        TimeSpan span = time - oldTime;
                        maxList.Add(span);
                        timePerLineSum += (int) (span.Ticks / TimeSpan.TicksPerMillisecond);
                        newDiffList.Add(new SpreadEntry(i, 0, time));
                        oldTime = time;
                        Logger.logDebug("TimeSpreadCalculator.DoCalc() time diff " + span.ToString());
                    }
                }
                if (maxList.Count > 3)
                {
                    maxList.Sort();
                    this.maxSpan = maxList[maxList.Count - 3];
                }
                lock (this.diffListLock)
                {
                    this.DiffList = newDiffList;
                    timePerLine = (int) Math.Round((double) timePerLineSum / ((double) (lastLineNum + 1) / step));
                    CalcValuesViaLines(timePerLine, this.maxSpan);
                    OnCalcDone(new EventArgs());
                    Logger.logDebug("TimeSpreadCalculator.DoCalc() end");
                }
            }
        }

        private void DoCalc_via_Time()
        {
            OnStartCalc(new EventArgs());
            Logger.logDebug("TimeSpreadCalculator.DoCalc_via_Time() begin");
            if (this.callback.GetLineCount() < 1)
            {
                OnCalcDone(new EventArgs());
                Logger.logDebug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
                return;
            }
            int lineNum = 0;
            int lastLineNum = this.callback.GetLineCount() - 1;
            this.startTimestamp = this.logWindow.GetTimestampForLineForward(ref lineNum, false);
            this.endTimestamp = this.logWindow.GetTimestampForLine(ref lastLineNum, false);

            if (this.startTimestamp != DateTime.MinValue && this.endTimestamp != DateTime.MinValue)
            {
                TimeSpan overallSpan = this.endTimestamp - this.startTimestamp;
                long overallSpanMillis = (long) (overallSpan.Ticks / TimeSpan.TicksPerMillisecond);
                //int timePerLine = (int)Math.Round((double)overallSpanMillis / (double)this.lineCount);
                DateTime oldTime = this.logWindow.GetTimestampForLineForward(ref lineNum, false);
                long step;
                if (overallSpanMillis > this.displayHeight)
                {
                    step = (long) Math.Round((double) overallSpanMillis / (double) this.displayHeight);
                }
                else
                {
                    step = 1;
                }

                Logger.logDebug("TimeSpreadCalculator.DoCalc_via_Time() time range is " + overallSpanMillis + " ms");

                lineNum = 0;
                DateTime searchTimeStamp = this.startTimestamp;
                int oldLineNum = lineNum;
                int loopCount = 0;
                int lineDiffSum = 0;
                int minDiff = int.MaxValue;
                this.maxDiff = 0;
                List<int> maxList = new List<int>();
                List<SpreadEntry> newDiffList = new List<SpreadEntry>();
                while (searchTimeStamp.CompareTo(this.endTimestamp) <= 0)
                {
                    lineNum = this.logWindow.FindTimestampLine_Internal(lineNum, lineNum, lastLineNum, searchTimeStamp,
                        false);
                    if (lineNum < 0)
                    {
                        lineNum = -lineNum;
                    }
                    int lineDiff = lineNum - oldLineNum;
#if DEBUG
					Logger.logDebug("TimeSpreadCalculator.DoCalc_via_Time() test time " + searchTimeStamp.ToString("HH:mm:ss.fff") + " line diff=" + lineDiff);
#endif
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
                    //lineNum++;
                }
                if (maxList.Count > 3)
                {
                    maxList.Sort();
                    maxDiff = maxList[maxList.Count - 3];
                }
                this.average = (double) lineDiffSum / (double) loopCount;
                //double average = maxList[maxList.Count / 2];
                Logger.logDebug("Average diff=" + average + " minDiff=" + minDiff + " maxDiff=" + maxDiff);
                lock (this.diffListLock)
                {
                    if (newDiffList.Count > 0)
                    {
                        newDiffList.RemoveAt(0);
                    }
                    if (newDiffList.Count > 0)
                    {
                        newDiffList.RemoveAt(0);
                    }
                    this.DiffList = newDiffList;
                    CalcValuesViaTime(maxDiff, average);
                    OnCalcDone(new EventArgs());
                    Logger.logDebug("TimeSpreadCalculator.DoCalc_via_Time() end");
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
                    int value = (int) (diffFromAverage / (double) (timePerLine / TimeSpan.TicksPerMillisecond)
                                       * (double) this.contrast);
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
                double diffFromAverage = (double) entry.diff - average;
                if (diffFromAverage < 0)
                {
                    diffFromAverage = 0;
                }
                int value = (int) (diffFromAverage / (double) maxDiff * (double) this.contrast);
                entry.value = 255 - value;
                Logger.logDebug("TimeSpreadCalculator.DoCalc() test time " + entry.timestamp.ToString("HH:mm:ss.fff") +
                                " line diff=" + lineDiff + " value=" + value);
            }
        }

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