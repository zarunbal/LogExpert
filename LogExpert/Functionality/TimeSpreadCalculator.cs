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
					_calcEvent.Set();
					_lineCountEvent.Set();
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
					_calcEvent.Set();
					_lineCountEvent.Set();
				}
			}
		}
		
		public int Contrast
		{
			get
			{
				return _contrast;
			}
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
					CalcValuesViaLines(_timePerLine);
				}
				OnCalcDone();
			}
		}
		
		public List<SpreadEntry> DiffList { get; set; }
		
		#endregion
		
		#region Events
		
		public event Action CalcDone;
		
		private void OnCalcDone()
		{
			if (CalcDone != null)
			{
				CalcDone();
			}
		}
		
		public event Action StartCalc;
		
		private void OnStartCalc()
		{
			if (StartCalc != null)
			{
				StartCalc();
			}
		}
		
		public int DisplayHeight 
		{ 
			get
			{
				return _displayHeight;
			}
			set
			{
				_displayHeight = value;
				if (Enabled)
				{
					_calcEvent.Set();
					_lineCountEvent.Set();
				}
			}
		}
		
		#endregion
		
		#region Public Methods
		
		public TimeSpreadCalculator(LogWindow logWindow)
		{
			_logWindow = logWindow;
			_callback = new LogWindow.ColumnizerCallback(_logWindow);
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
			if (!_calcThread.Join(300))
			{
				_calcThread.Abort();
			}
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
						Calc();
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
		
		private void Calc()
		{
			OnStartCalc();
			Logger.logDebug("TimeSpreadCalculator.DoCalc_via_Time() begin");
			if (_callback.GetLineCount() > 1)
			{
				int lineNum = 0;
				int lastLineNum = _callback.GetLineCount() - 1;
				_startTimestamp = _logWindow.GetTimestampForLineForward(ref lineNum, false);
				_endTimestamp = _logWindow.GetTimestampForLine(ref lastLineNum, false);
				
				int timePerLineSum = 0;
				
				if (_startTimestamp != DateTime.MinValue && _endTimestamp != DateTime.MinValue)
				{
					TimeSpan overallSpan = _endTimestamp - _startTimestamp;
					long overallSpanMillis = overallSpan.Ticks / TimeSpan.TicksPerMillisecond;
					DateTime oldTime = _logWindow.GetTimestampForLineForward(ref lineNum, false);
					
					long step = 1;
					if (overallSpanMillis > DisplayHeight)
					{
						step = (long)Math.Round((double)overallSpanMillis / (double)DisplayHeight);
					}
					
					Logger.logDebug(string.Format("TimeSpreadCalculator.DoCalc_via_Time() time range is {0} ms", overallSpanMillis));
					
					List<SpreadEntry> newDiffList = new List<SpreadEntry>();
					
					int maxDiff = 0;
					
					if (TimeMode)
					{
						DateTime searchTimeStamp = _startTimestamp;
						int oldLineNum = lineNum;
						int loopCount = 0;
						int lineDiffSum = 0;
						int minDiff = Int32.MaxValue;
						_maxDiff = 0;
						List<int> maxList = new List<int>();
						while (searchTimeStamp.CompareTo(_endTimestamp) <= 0)
						{
							lineNum = _logWindow.FindTimestampLine_Internal(lineNum, lineNum, lastLineNum, searchTimeStamp, false);
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
								if (lineDiff > maxDiff)
								{
									maxDiff = lineDiff;
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
							maxDiff = maxList[maxList.Count - 3];
						}
						_average = (double)lineDiffSum / (double)loopCount;
						
						Logger.logDebug(string.Format("Average diff={0} minDiff={1} maxDiff={2}", _average, minDiff, maxDiff));
						
						if (newDiffList.Count > 0)
						{
							newDiffList.RemoveAt(0);
						}
						if (newDiffList.Count > 0)
						{
							newDiffList.RemoveAt(0);
						}
						
						_maxDiff = maxDiff;
					}
					else
					{
						maxDiff = (int)Math.Round((double)overallSpanMillis / (double)_lineCount);
						List<TimeSpan> maxList = new List<TimeSpan>();
						lineNum++;
						for (int i = lineNum; i < lastLineNum; i += (int)step)
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
						
						maxDiff = (int)Math.Round((double)timePerLineSum / ((double)(lastLineNum + 1) / (double)step));
						
						if (maxList.Count > 3)
						{
							maxList.Sort();
							_maxSpan = maxList[maxList.Count - 3];
						}
						
						_timePerLine = maxDiff;
					}
					lock (_diffListLock)
					{
						DiffList = newDiffList;
						if (TimeMode)
						{
							CalcValuesViaTime(maxDiff, _average);
						}
						else
						{
							CalcValuesViaLines(maxDiff); 
						}
						
						Logger.logDebug("TimeSpreadCalculator.DoCalc() end");
						OnCalcDone();
					}
				}
			}
			else
			{
				Logger.logDebug("TimeSpreadCalculator.DoCalc() end because of line count < 1");
				OnCalcDone();
			}
		}
		
		private DateTime CalcValuesViaLines(int timePerLine)
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