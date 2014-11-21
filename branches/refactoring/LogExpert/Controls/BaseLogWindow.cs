using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WeifenLuo.WinFormsUI.Docking;

namespace LogExpert.Controls
{
	//Zarunbal: only for refactoring and cleanup
	public abstract class BaseLogWindow : DockContent
	{
		#region Fields
		
		private readonly Thread _logEventHandlerThread = null;
		private readonly EventWaitHandle _logEventArgsEvent = new ManualResetEvent(false);
		private readonly List<LogEventArgs> _logEventArgsList = new List<LogEventArgs>();
		
		private readonly BookmarkDataProvider _bookmarkProvider = new BookmarkDataProvider();
		
		private SortedList<int, RowHeightEntry> _rowHeightList = new SortedList<int, RowHeightEntry>();
		
		private readonly IList<FilterPipe> _filterPipeList = new List<FilterPipe>();
		
		protected TimeSpreadCalculator _timeSpreadCalc;
		
		#endregion
		
		#region cTor
		
		public BaseLogWindow()
		{
			_logEventHandlerThread = new Thread(new ThreadStart(LogEventWorker));
			_logEventHandlerThread.IsBackground = true;
			_logEventHandlerThread.Start();
		}
		
		#endregion
		
		#region Methods
		
		private void LogEventWorker()
		{
			Thread.CurrentThread.Name = "LogEventWorker";
			while (true)
			{
				Logger.logDebug("Waiting for signal");
				_logEventArgsEvent.WaitOne();
				Logger.logDebug("Wakeup signal received.");
				while (true)
				{
					LogEventArgs e;
					int lastLineCount = 0;
					lock (_logEventArgsList)
					{
						Logger.logInfo("" + _logEventArgsList.Count + " events in queue");
						if (_logEventArgsList.Count == 0)
						{
							_logEventArgsEvent.Reset();
							break;
						}
						e = _logEventArgsList[0];
						_logEventArgsList.RemoveAt(0);
					}
					if (e.IsRollover)
					{
						ShiftBookmarks(e.RolloverOffset);
						ShiftRowHeightList(e.RolloverOffset);
						ShiftFilterPipes(e.RolloverOffset);
						lastLineCount = 0;
					}
					else
					{
						if (e.LineCount < lastLineCount)
						{
							Logger.logError("Line count of event is: " + e.LineCount + ", should be greater than last line count: " + lastLineCount);
						}
					}
					Action<LogEventArgs> callback = new Action<LogEventArgs>(UpdateGrid);
					Invoke(callback, new object[] { e });
					CheckFilterAndHighlight(e);
					_timeSpreadCalc.SetLineCount(e.LineCount);
				}
			}
		}

		#region Bookmarks
		 
		/**
		 * Shift bookmarks after a logfile rollover
		 */
		private void ShiftBookmarks(int offset)
		{
			_bookmarkProvider.ShiftBookmarks(offset);
			OnBookmarkRemoved();
		}

		#endregion
		
		private void ShiftRowHeightList(int offset)
		{
			SortedList<int, RowHeightEntry> newList = new SortedList<int, RowHeightEntry>();
			foreach (RowHeightEntry entry in _rowHeightList.Values)
			{
				int line = entry.LineNum - offset;
				if (line >= 0)
				{
					entry.LineNum = line;
					newList.Add(line, entry);
				}
			}
			_rowHeightList = newList;
		}
		
		private void ShiftFilterPipes(int offset)
		{
			lock (_filterPipeList)
			{
				foreach (FilterPipe pipe in _filterPipeList)
				{
					pipe.ShiftLineNums(offset);
				}
			}
		}

		#endregion

		#region Abstract methods

		protected abstract void UpdateGrid(LogEventArgs e);

		protected abstract void CheckFilterAndHighlight(LogEventArgs e);

		#endregion

		#region Event delegate and methods

		public delegate void BookmarkRemovedEventHandler(object sender, EventArgs e);

		public event BookmarkRemovedEventHandler BookmarkRemoved;
		
		private void OnBookmarkRemoved()
		{
			if (BookmarkRemoved != null)
			{
				BookmarkRemoved(this, new EventArgs());
			}
		}

		#endregion
	}
}