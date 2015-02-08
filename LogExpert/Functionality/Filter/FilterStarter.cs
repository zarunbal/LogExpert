using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
	public delegate void ProgressCallback(int lineCount);

	class FilterStarter
	{
		delegate Filter WorkerFx(FilterParams filterParams, int startLine, int maxCount, ProgressCallback callback);

		private List<Filter> filterReadyList;
		private List<Filter> filterWorkerList;
		private SortedDictionary<int, int> filterHitDict;
		private SortedDictionary<int, int> filterResultDict;
		private SortedDictionary<int, int> lastFilterLinesDict;

		private LogExpert.ColumnizerCallback callback;

		private ProgressCallback progressCallback;
		private int progressLineCount;
		private bool shouldStop;

		public FilterStarter(LogExpert.ColumnizerCallback callback, int minThreads)
		{
			this.callback = callback;
			this.FilterResultLines = new List<int>();
			this.LastFilterLinesList = new List<int>();
			this.FilterHitList = new List<int>();
			this.filterReadyList = new List<Filter>();
			this.filterWorkerList = new List<Filter>();
			this.filterHitDict = new SortedDictionary<int, int>();
			this.filterResultDict = new SortedDictionary<int, int>();
			this.lastFilterLinesDict = new SortedDictionary<int, int>();
			this.ThreadCount = Environment.ProcessorCount * 4;
			this.ThreadCount = minThreads;
			int worker, completion;
			ThreadPool.GetMinThreads(out worker, out completion);
			ThreadPool.SetMinThreads(minThreads, completion);
			ThreadPool.GetMaxThreads(out worker, out completion);
		}

		public List<int> FilterResultLines { get; set; }

		public List<int> LastFilterLinesList { get; set; }

		public List<int> FilterHitList { get; set; }

		public int ThreadCount { get; set; }

		public void DoFilter(FilterParams filterParams, int startLine, int maxCount, ProgressCallback progressCallback)
		{
			this.FilterResultLines.Clear();
			this.LastFilterLinesList.Clear();
			this.FilterHitList.Clear();
			this.filterHitDict.Clear();
			this.filterReadyList.Clear();
			this.filterResultDict.Clear();
			this.lastFilterLinesDict.Clear();
			this.filterReadyList.Clear();
			this.filterWorkerList.Clear();
			this.shouldStop = false;

			int interval = maxCount / this.ThreadCount;
			if (interval < 1)
			{
				interval = 1;
			}
			int workStartLine = startLine;
			List<WaitHandle> handleList = new List<WaitHandle>();
			progressLineCount = 0;
			this.progressCallback = progressCallback;
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
				WorkerFx workerFx = new WorkerFx(this.DoWork);
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
			this.shouldStop = true;
			lock (this.filterWorkerList)
			{
				Logger.logInfo("Filter cancel requested. Stopping all " + this.filterWorkerList.Count + " threads.");
				foreach (Filter filter in this.filterWorkerList)
				{
					filter.ShouldCancel = true;
				}
			}
		}

		private void ThreadProgressCallback(int lineCount)
		{
			int count = Interlocked.Add(ref this.progressLineCount, lineCount);
			this.progressCallback(count);
		}

		private Filter DoWork(FilterParams filterParams, int startLine, int maxCount, ProgressCallback progressCallback)
		{
			Logger.logInfo("Started Filter worker [" + Thread.CurrentThread.ManagedThreadId + "] for line " + startLine);

			// Give every thread own copies of ColumnizerCallback and FilterParams, because the state of the objects changes while filtering
			FilterParams threadFilterParams = filterParams.CreateCopy2();
			LogExpert.ColumnizerCallback threadColumnizerCallback = this.callback.createCopy();

			Filter filter = new Filter(threadColumnizerCallback);
			lock (this.filterWorkerList)
			{
				this.filterWorkerList.Add(filter);
			}
			if (this.shouldStop)
			{
				return filter;
			}
			int realCount = filter.DoFilter(threadFilterParams, startLine, maxCount, progressCallback);
			Logger.logInfo("Filter worker [" + Thread.CurrentThread.ManagedThreadId + "] for line " + startLine + " has completed.");
			lock (this.filterReadyList)
			{
				this.filterReadyList.Add(filter);
			}
			return filter;
		}

		private void FilterDoneCallback(IAsyncResult ar)
		{
			//if (ar.IsCompleted)
			//{
			//  Filter filter = ((WorkerFx)ar.AsyncState).EndInvoke(ar);
			//  lock (this.filterReadyList)
			//  {
			//    this.filterReadyList.Add(filter);
			//  }
			//}
			Filter filter = ((WorkerFx)ar.AsyncState).EndInvoke(ar);  // EndInvoke() has to be called mandatory.
		}

		private void MergeResults()
		{
			Logger.logInfo("Merging filter results.");
			foreach (Filter filter in this.filterReadyList)
			{
				foreach (int lineNum in filter.FilterHitList)
				{
					if (!this.filterHitDict.ContainsKey(lineNum))
					{
						this.filterHitDict.Add(lineNum, lineNum);
					}
				}
				foreach (int lineNum in filter.FilterResultLines)
				{
					if (!this.filterResultDict.ContainsKey(lineNum))
					{
						this.filterResultDict.Add(lineNum, lineNum);
					}
				}
				foreach (int lineNum in filter.LastFilterLinesList)
				{
					if (!this.lastFilterLinesDict.ContainsKey(lineNum))
					{
						this.lastFilterLinesDict.Add(lineNum, lineNum);
					}
				}
			}
			this.FilterHitList.AddRange(this.filterHitDict.Keys);
			this.FilterResultLines.AddRange(this.filterResultDict.Keys);
			this.LastFilterLinesList.AddRange(this.lastFilterLinesDict.Keys);
			Logger.logInfo("Merging done.");
		}
	}
}