using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
	public class TimeSpreadCalculator
	{
		#region Fields
		
		private const int INACTIVITY_TIME = 2000;
		
		private DateTime _startTimestamp;
		private DateTime _endTimestamp;
		private int _lineCount = 0;
		private int _displayHeight = 0;
		private bool _enabled;
		private int _contrast = 400;
		private bool _shouldStop = false;
		
		// for DoCalc_via_Time
		private double _average;
		private int _maxDiff;
		
		// for DoCalc
		private int _timePerLine;
		private TimeSpan _maxSpan;
		
		private bool _timeMode = true;
		
		private const int MAX_CONTRAST = 1300;
		
		private readonly LogWindow _logWindow;
		private readonly LogWindow.ColumnizerCallback _callback;
		
		private readonly EventWaitHandle _calcEvent = new ManualResetEvent(false);
		private readonly EventWaitHandle _lineCountEvent = new ManualResetEvent(false);
		private readonly Thread _calcThread = null;
		
		private readonly Object _diffListLock = new Object();
		
		#endregion

		#region ctor

		public TimeSpreadCalculator()
		{
			DiffList = new List<SpreadEntry>();
		}
		#endregion
		
		#region Properties
		
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
				if (_enabled)
				{
					this._calcEvent.Set();
					this._lineCountEvent.Set();
				}
				else
				{
				}
			}
		}
		
		public bool TimeMode
		{
			get
			{
				return _timeMode;
			}
			set
			{
				_timeMode = value;
				if (_enabled)
				{
					this._calcEvent.Set();
					this._lineCountEvent.Set();
				}
			}
		}
		
		public int Contrast
		{
			set
			{
				this._contrast = value;
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
					CalcValuesViaTime(this._maxDiff, this._average);
				}
				else
				{
					CalcValuesViaLines(this._timePerLine, this._maxSpan);
				}
				OnCalcDone(new EventArgs());
			}
			
			get
			{
				return this._contrast;
			}
		}
		
		public List<SpreadEntry> DiffList { get; set; }

		#endregion

		#region Events

		public delegate void CalcDoneEventHandler(object sender, EventArgs e);

		public event CalcDoneEventHandler CalcDone;
		
		private void OnCalcDone(EventArgs e)
		{
			if (CalcDone != null)
			{
				CalcDone(this, e);
			}
		}

		public delegate void StartCalcEventHandler(object sender, EventArgs e);

		public event StartCalcEventHandler StartCalc;
		
		private void OnStartCalc(EventArgs e)
		{
			if (StartCalc != null)
			{
				StartCalc(this, e);
			}
		}

		#endregion
		
		#region Public Methods
			
		public TimeSpreadCalculator(LogWindow logWindow)
		{
			_logWindow = logWindow;
			_callback = new LogWindow.ColumnizerCallback(this._logWindow);
			_calcThread = new Thread(new ThreadStart(WorkerFx));
			_calcThread.IsBackground = true;
			_calcThread.Name = "TimeSpreadCalculator Worker";
			_calcThread.Priority = ThreadPriority.BelowNormal;
			_calcThread.Start();
		}
			
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
		
		#region Methods
			
		private void WorkerFx()
		{				
			while (!_shouldStop)
			{
				// wait for wakeup
				_lineCountEvent.WaitOne();
					
				while (!_shouldStop)
				{
					// wait for unbusy moments
					Logger.logDebug("TimeSpreadCalculator: wait for unbusy moments");
					bool signalled = _calcEvent.WaitOne(INACTIVITY_TIME, false);
					if (!signalled)
					{
						Logger.logDebug("TimeSpreadCalculator: unbusy. starting calc.");
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
					else
					{
						Logger.logDebug("TimeSpreadCalculator: signalled. no calc.");
					}
					_calcEvent.Reset();
				}
				_lineCountEvent.Reset();
			}
		}
			
		private void DoCalc()
		{
			OnStartCalc(new EventArgs());
			Logger.logDebug("TimeSpreadCalculator.DoCalc() begin");
			if (_callback.GetLineCount() < 1)
			{
				OnCalcDone(new EventArgs());
				Logger.logDebug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
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
				int overallSpanMillis = (int)(overallSpan.Ticks / TimeSpan.TicksPerMillisecond);
				_timePerLine = (int)Math.Round((double)overallSpanMillis / (double)_lineCount);
				DateTime oldTime = _logWindow.GetTimestampForLineForward(ref lineNum, false);
				int step;
				if (_lineCount > _displayHeight)
				{
					step = (int)Math.Round((double)_lineCount / (double)_displayHeight);
				}
				else
				{
					step = 1;
				}
				
				Logger.logDebug(string.Format("TimeSpreadCalculator.DoCalc() collecting data for {0} lines with step size {1}", lastLineNum, step));
				
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
						timePerLineSum += (int)(span.Ticks / TimeSpan.TicksPerMillisecond);
						newDiffList.Add(new SpreadEntry(i, 0, time));
						oldTime = time;
						Logger.logDebug(string.Format("TimeSpreadCalculator.DoCalc() time diff {0}", span));
					}
				}
				if (maxList.Count > 3)
				{
					maxList.Sort();
					_maxSpan = maxList[maxList.Count - 3];
				}
				lock (this._diffListLock)
				{
					DiffList = newDiffList;
					_timePerLine = (int)Math.Round((double)timePerLineSum / ((double)(lastLineNum + 1) / step));
					CalcValuesViaLines(_timePerLine, _maxSpan);
					OnCalcDone(new EventArgs());
					Logger.logDebug("TimeSpreadCalculator.DoCalc() end");
				}
			}
		}
			
		private void DoCalcViaTime()
		{
			OnStartCalc(new EventArgs());
			Logger.logDebug("TimeSpreadCalculator.DoCalc_via_Time() begin");
			if (_callback.GetLineCount() < 1)
			{
				OnCalcDone(new EventArgs());
				Logger.logDebug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
				return;
			}
			int lineNum = 0;
			int lastLineNum = _callback.GetLineCount() - 1;
			_startTimestamp = _logWindow.GetTimestampForLineForward(ref lineNum, false);
			_endTimestamp = _logWindow.GetTimestampForLine(ref lastLineNum, false);
				
			if (this._startTimestamp != DateTime.MinValue && this._endTimestamp != DateTime.MinValue)
			{
				TimeSpan overallSpan = _endTimestamp - _startTimestamp;
				long overallSpanMillis = (long)(overallSpan.Ticks / TimeSpan.TicksPerMillisecond);
				//int timePerLine = (int)Math.Round((double)overallSpanMillis / (double)this.lineCount);
				DateTime oldTime = this._logWindow.GetTimestampForLineForward(ref lineNum, false);
				long step;
				if (overallSpanMillis > this._displayHeight)
				{
					step = (long)Math.Round((double)overallSpanMillis / (double)this._displayHeight);
				}
				else
				{
					step = 1;
				}
				
				Logger.logDebug(string.Format("TimeSpreadCalculator.DoCalc_via_Time() time range is {0} ms", overallSpanMillis));
				
				lineNum = 0;
				DateTime searchTimeStamp = this._startTimestamp;
				int oldLineNum = lineNum;
				int loopCount = 0;
				int lineDiffSum = 0;
				int minDiff = Int32.MaxValue;
				this._maxDiff = 0;
				List<int> maxList = new List<int>();
				List<SpreadEntry> newDiffList = new List<SpreadEntry>();
				while (searchTimeStamp.CompareTo(_endTimestamp) <= 0)
				{
					lineNum = this._logWindow.FindTimestampLine_Internal(lineNum, lineNum, lastLineNum, searchTimeStamp, false);
					if (lineNum < 0)
					{
						lineNum = -lineNum;
					}
					int lineDiff = lineNum - oldLineNum;
					#if DEBUG
					Logger.logDebug(string.Format("TimeSpreadCalculator.DoCalc_via_Time() test time {0} line diff={1}", searchTimeStamp.ToString("HH:mm:ss.fff"), lineDiff));
					#endif
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
					//LineNum++;
				}
				if (maxList.Count > 3)
				{
					maxList.Sort();
					_maxDiff = maxList[maxList.Count - 3];
				}
				_average = (double)lineDiffSum / (double)loopCount;
				//double average = maxList[maxList.Count / 2];
				Logger.logDebug(string.Format("Average diff={0} minDiff={1} maxDiff={2}", _average, minDiff, _maxDiff));
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
				oldTime = DiffList[0].Timestamp;
				foreach (SpreadEntry entry in DiffList)
				{
					TimeSpan span = entry.Timestamp - oldTime;
					double diffFromAverage = (int)(span.Ticks / TimeSpan.TicksPerMillisecond) - timePerLine;
					if (diffFromAverage < 0)
					{
						diffFromAverage = 0;
					}
					int value = (int)(diffFromAverage / (double)(timePerLine / TimeSpan.TicksPerMillisecond) *
									  (double)_contrast);
					entry.Value = 255 - value;
					oldTime = entry.Timestamp;
				}
			}
			return oldTime;
		}
			
		private void CalcValuesViaTime(int maxDiff, double average)
		{
			foreach (SpreadEntry entry in DiffList)
			{
				int lineDiff = entry.Diff;
				double diffFromAverage = (double)entry.Diff - average;
				if (diffFromAverage < 0)
				{
					diffFromAverage = 0;
				}
				int value = (int)(diffFromAverage / (double)maxDiff * (double)_contrast);
				entry.Value = 255 - value;
				Logger.logDebug(string.Format("TimeSpreadCalculator.DoCalc() test time {0} line diff={1} value={2}", entry.Timestamp.ToString("HH:mm:ss.fff"), lineDiff, value));
			}
		}

		#endregion
	}
}