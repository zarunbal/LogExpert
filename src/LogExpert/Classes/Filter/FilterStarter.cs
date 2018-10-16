using System;
using System.Collections.Generic;
using System.Threading;
using NLog;

namespace LogExpert
{
    public delegate void ProgressCallback(int lineCount);

    internal class FilterStarter
    {
        #region Delegates

        private delegate Filter WorkerFx(FilterParams filterParams, int startLine, int maxCount,
                                         ProgressCallback callback);

        #endregion

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        #region Private Fields

        private readonly LogWindow.ColumnizerCallback callback;
        private readonly SortedDictionary<int, int> filterHitDict;
        private readonly List<Filter> filterReadyList;
        private readonly SortedDictionary<int, int> filterResultDict;

        private readonly List<Filter> filterWorkerList;
        private readonly SortedDictionary<int, int> lastFilterLinesDict;

        private ProgressCallback progressCallback;
        private int progressLineCount;
        private bool shouldStop;

        #endregion

        #region Ctor

        public FilterStarter(LogWindow.ColumnizerCallback callback, int minThreads)
        {
            this.callback = callback;
            FilterResultLines = new List<int>();
            LastFilterLinesList = new List<int>();
            FilterHitList = new List<int>();
            filterReadyList = new List<Filter>();
            filterWorkerList = new List<Filter>();
            filterHitDict = new SortedDictionary<int, int>();
            filterResultDict = new SortedDictionary<int, int>();
            lastFilterLinesDict = new SortedDictionary<int, int>();
            ThreadCount = Environment.ProcessorCount * 4;
            ThreadCount = minThreads;
            int worker, completion;
            ThreadPool.GetMinThreads(out worker, out completion);
            ThreadPool.SetMinThreads(minThreads, completion);
            ThreadPool.GetMaxThreads(out worker, out completion);
        }

        #endregion

        #region Properties / Indexers

        public List<int> FilterHitList { get; set; }

        public List<int> FilterResultLines { get; set; }

        public List<int> LastFilterLinesList { get; set; }

        public int ThreadCount { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Requests the FilterStarter to stop all filter threads. Call this from another thread (e.g. GUI). The function
        ///     returns
        ///     immediately without waiting for filter end.
        /// </summary>
        public void CancelFilter()
        {
            shouldStop = true;
            lock (filterWorkerList)
            {
                _logger.Info("Filter cancel requested. Stopping all {0} threads.", filterWorkerList.Count);
                foreach (Filter filter in filterWorkerList)
                {
                    filter.ShouldCancel = true;
                }
            }
        }

        public void DoFilter(FilterParams filterParams, int startLine, int maxCount, ProgressCallback progressCallback)
        {
            FilterResultLines.Clear();
            LastFilterLinesList.Clear();
            FilterHitList.Clear();
            filterHitDict.Clear();
            filterReadyList.Clear();
            filterResultDict.Clear();
            lastFilterLinesDict.Clear();
            filterReadyList.Clear();
            filterWorkerList.Clear();
            shouldStop = false;

            int interval = maxCount / ThreadCount;
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

                _logger.Info("FilterStarter starts worker for line {0}, lineCount {1}", workStartLine, interval);
                WorkerFx workerFx = DoWork;
                IAsyncResult ar = workerFx.BeginInvoke(filterParams, workStartLine, interval, ThreadProgressCallback,
                    FilterDoneCallback, workerFx);
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

        #endregion

        #region Event raising Methods

        private Filter DoWork(FilterParams filterParams, int startLine, int maxCount, ProgressCallback progressCallback)
        {
            _logger.Info("Started Filter worker [{0}] for line {1}", Thread.CurrentThread.ManagedThreadId, startLine);

            // Give every thread own copies of ColumnizerCallback and FilterParams, because the state of the objects changes while filtering
            FilterParams threadFilterParams = filterParams.CreateCopy2();
            LogWindow.ColumnizerCallback threadColumnizerCallback = callback.createCopy();

            Filter filter = new Filter(threadColumnizerCallback);
            lock (filterWorkerList)
            {
                filterWorkerList.Add(filter);
            }

            if (shouldStop)
            {
                return filter;
            }

            int realCount = filter.DoFilter(threadFilterParams, startLine, maxCount, progressCallback);
            _logger.Info("Filter worker [{0}] for line {1} has completed.", Thread.CurrentThread.ManagedThreadId, startLine);
            lock (filterReadyList)
            {
                filterReadyList.Add(filter);
            }

            return filter;
        }

        private void FilterDoneCallback(IAsyncResult ar)
        {
            // if (ar.IsCompleted)
            // {
            // Filter filter = ((WorkerFx)ar.AsyncState).EndInvoke(ar);
            // lock (this.filterReadyList)
            // {
            // this.filterReadyList.Add(filter);
            // }
            // }
            Filter filter = ((WorkerFx)ar.AsyncState).EndInvoke(ar); // EndInvoke() has to be called mandatory.
        }

        #endregion

        #region Private Methods

        private void MergeResults()
        {
            _logger.Info("Merging filter results.");
            foreach (Filter filter in filterReadyList)
            {
                foreach (int lineNum in filter.FilterHitList)
                {
                    if (!filterHitDict.ContainsKey(lineNum))
                    {
                        filterHitDict.Add(lineNum, lineNum);
                    }
                }

                foreach (int lineNum in filter.FilterResultLines)
                {
                    if (!filterResultDict.ContainsKey(lineNum))
                    {
                        filterResultDict.Add(lineNum, lineNum);
                    }
                }

                foreach (int lineNum in filter.LastFilterLinesList)
                {
                    if (!lastFilterLinesDict.ContainsKey(lineNum))
                    {
                        lastFilterLinesDict.Add(lineNum, lineNum);
                    }
                }
            }

            FilterHitList.AddRange(filterHitDict.Keys);
            FilterResultLines.AddRange(filterResultDict.Keys);
            LastFilterLinesList.AddRange(lastFilterLinesDict.Keys);
            _logger.Info("Merging done.");
        }

        private void ThreadProgressCallback(int lineCount)
        {
            int count = Interlocked.Add(ref progressLineCount, lineCount);
            progressCallback(count);
        }

        #endregion
    }
}
