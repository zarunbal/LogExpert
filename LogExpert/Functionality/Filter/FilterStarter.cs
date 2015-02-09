using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
	public delegate void ProgressCallback(int lineCount);

	public class FilterStarter
	{
		#region Fields

		private List<Filter> _filterReadyList = new List<Filter>();
		private List<Filter> _filterWorkerList = new List<Filter>();
		private SortedDictionary<int, int> _filterHitDict = new SortedDictionary<int, int>();
		private SortedDictionary<int, int> _filterResultDict = new SortedDictionary<int, int>();
		private SortedDictionary<int, int> _lastFilterLinesDict = new SortedDictionary<int, int>();
		
		private LogExpert.ColumnizerCallback _callback;
		
		private ProgressCallback _progressCallback;
		private int _progressLineCount;
		private bool _shouldStop; 
		
		#endregion
		
		#region cTor
		
		public FilterStarter(LogExpert.ColumnizerCallback callback, int minThreads)
		{
			_callback = callback;

			FilterResultLines = new List<int>();
			LastFilterLinesList = new List<int>();
			FilterHitList = new List<int>();

			ThreadCount = Environment.ProcessorCount * 4;
			ThreadCount = minThreads;

			int worker;
			int completion;

			ThreadPool.GetMinThreads(out worker, out completion);
			ThreadPool.SetMinThreads(minThreads, completion);
			ThreadPool.GetMaxThreads(out worker, out completion);
		}

		#endregion

		#region Delegates

		delegate Filter WorkerFx(FilterParams filterParams, int startLine, int maxCount, ProgressCallback callback);

		#endregion
		
		#region Properties
		
		public List<int> FilterResultLines { get; set; }
		
		public List<int> LastFilterLinesList { get; set; }
		
		public List<int> FilterHitList { get; set; }
		
		public int ThreadCount { get; set; }
		
		#endregion
		
		#region Public Methods
			
		public void DoFilter(FilterParams filterParams, int startLine, int maxCount, ProgressCallback progressCallback)
		{
			FilterResultLines.Clear();
			LastFilterLinesList.Clear();
			FilterHitList.Clear();
			_filterHitDict.Clear();
			_filterReadyList.Clear();
			_filterResultDict.Clear();
			_lastFilterLinesDict.Clear();
			_filterReadyList.Clear();
			_filterWorkerList.Clear();
			_shouldStop = false;
				
			int interval = maxCount / ThreadCount;
			if (interval < 1)
			{
				interval = 1;
			}
			int workStartLine = startLine;
			List<WaitHandle> handleList = new List<WaitHandle>();
			_progressLineCount = 0;
			_progressCallback = progressCallback;
			while (workStartLine < startLine + maxCount)
			{
				if (workStartLine + interval > maxCount)
				{
					interval = maxCount - workStartLine;
					if (interval == 0)
					{
						break;
					}
				}
				Logger.logInfo("FilterStarter starts worker for line " + workStartLine + ", lineCount " + interval);
				WorkerFx workerFx = new WorkerFx(DoWork);
				IAsyncResult ar = workerFx.BeginInvoke(filterParams, workStartLine, interval, ThreadProgressCallback, FilterDoneCallback, workerFx);
				workStartLine += interval;
				handleList.Add(ar.AsyncWaitHandle);
			}
			WaitHandle[] handles = handleList.ToArray();
			// wait for worker threads completion
			if (handles.Length > 0)
			{
				WaitHandle.WaitAll(handles);
			}
		
			MergeResults();
		}
		
		/// <summary>
		/// Requests the FilterStarter to stop all filter threads. Call this from another thread (e.g. GUI). The function returns
		/// immediately without waiting for filter end.
		/// </summary>
		public void CancelFilter()
		{
			_shouldStop = true;
			lock (_filterWorkerList)
			{
				Logger.logInfo("Filter cancel requested. Stopping all " + _filterWorkerList.Count + " threads.");
				foreach (Filter filter in _filterWorkerList)
				{
					filter.ShouldCancel = true;
				}
			}
		}
		
		#endregion
			
		#region Private Methods

		private void ThreadProgressCallback(int lineCount)
		{
			int count = Interlocked.Add(ref _progressLineCount, lineCount);
			_progressCallback(count);
		}
			
		private Filter DoWork(FilterParams filterParams, int startLine, int maxCount, ProgressCallback progressCallback)
		{
			Logger.logInfo(string.Format("Started Filter worker [{0}] for line {1}", Thread.CurrentThread.ManagedThreadId, startLine));
			
			// Give every thread own copies of ColumnizerCallback and FilterParams, because the state of the objects changes while filtering
			FilterParams threadFilterParams = filterParams.CreateCopy2();
			LogExpert.ColumnizerCallback threadColumnizerCallback = _callback.createCopy();
			
			Filter filter = new Filter(threadColumnizerCallback);
			lock (_filterWorkerList)
			{
				_filterWorkerList.Add(filter);
			}
			if (_shouldStop)
			{
				return filter;
			}
			int realCount = filter.DoFilter(threadFilterParams, startLine, maxCount, progressCallback);
			Logger.logInfo(string.Format("Filter worker [{0}] for line {1} has completed.", Thread.CurrentThread.ManagedThreadId, startLine));
			lock (_filterReadyList)
			{
				_filterReadyList.Add(filter);
			}
			return filter;
		}
			
		private void FilterDoneCallback(IAsyncResult ar)
		{
			Filter filter = ((WorkerFx)ar.AsyncState).EndInvoke(ar);  // EndInvoke() has to be called mandatory.
		}
				
		private void MergeResults()
		{
			Logger.logInfo("Merging filter results.");
			foreach (Filter filter in _filterReadyList)
			{
				foreach (int lineNum in filter.FilterHitList)
				{
					if (!_filterHitDict.ContainsKey(lineNum))
					{
						_filterHitDict.Add(lineNum, lineNum);
					}
				}
				foreach (int lineNum in filter.FilterResultLines)
				{
					if (!_filterResultDict.ContainsKey(lineNum))
					{
						_filterResultDict.Add(lineNum, lineNum);
					}
				}
				foreach (int lineNum in filter.LastFilterLinesList)
				{
					if (!_lastFilterLinesDict.ContainsKey(lineNum))
					{
						_lastFilterLinesDict.Add(lineNum, lineNum);
					}
				}
			}
			FilterHitList.AddRange(_filterHitDict.Keys);
			FilterResultLines.AddRange(_filterResultDict.Keys);
			LastFilterLinesList.AddRange(_lastFilterLinesDict.Keys);
			Logger.logInfo("Merging done.");
		}

		#endregion
	}
}