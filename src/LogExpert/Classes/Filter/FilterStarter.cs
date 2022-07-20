using System;
using System.Collections.Generic;
using System.Threading;
using LogExpert.Classes.ILogLineColumnizerCallback;
using NLog;

namespace LogExpert.Classes.Filter
{
    public delegate void ProgressCallback(int lineCount);

    internal class FilterStarter
    {
        #region Fields

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly ColumnizerCallback _callback;
        private readonly SortedDictionary<int, int> _filterHitDict;
        private readonly List<Filter> _filterReadyList;
        private readonly SortedDictionary<int, int> _filterResultDict;

        private readonly List<Filter> _filterWorkerList;
        private readonly SortedDictionary<int, int> _lastFilterLinesDict;

        private ProgressCallback _progressCallback;
        private int _progressLineCount;
        private bool _shouldStop;

        #endregion

        #region cTor

        public FilterStarter(ColumnizerCallback callback, int minThreads)
        {
            _callback = callback;
            FilterResultLines = new List<int>();
            LastFilterLinesList = new List<int>();
            FilterHitList = new List<int>();
            _filterReadyList = new List<Filter>();
            _filterWorkerList = new List<Filter>();
            _filterHitDict = new SortedDictionary<int, int>();
            _filterResultDict = new SortedDictionary<int, int>();
            _lastFilterLinesDict = new SortedDictionary<int, int>();
            ThreadCount = Environment.ProcessorCount * 4;
            ThreadCount = minThreads;
            ThreadPool.GetMinThreads(out var worker, out var completion);
            ThreadPool.SetMinThreads(minThreads, completion);
            ThreadPool.GetMaxThreads(out worker, out completion);
        }

        #endregion

        #region Properties

        public List<int> FilterResultLines { get; set; }

        public List<int> LastFilterLinesList { get; set; }

        public List<int> FilterHitList { get; set; }

        public int ThreadCount { get; set; }

        #endregion

        #region Public methods

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


        /// <summary>
        /// Requests the FilterStarter to stop all filter threads. Call this from another thread (e.g. GUI). The function returns
        /// immediately without waiting for filter end.
        /// </summary>
        public void CancelFilter()
        {
            _shouldStop = true;
            lock (_filterWorkerList)
            {
                _logger.Info("Filter cancel requested. Stopping all {0} threads.", _filterWorkerList.Count);
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
            _logger.Info("Started Filter worker [{0}] for line {1}", Thread.CurrentThread.ManagedThreadId, startLine);

            // Give every thread own copies of ColumnizerCallback and FilterParams, because the state of the objects changes while filtering
            FilterParams threadFilterParams = filterParams.CreateCopy2();
            ColumnizerCallback threadColumnizerCallback = _callback.CreateCopy();

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
            _logger.Info("Filter worker [{0}] for line {1} has completed.", Thread.CurrentThread.ManagedThreadId, startLine);
            lock (_filterReadyList)
            {
                _filterReadyList.Add(filter);
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
            Filter filter = ((WorkerFx) ar.AsyncState).EndInvoke(ar); // EndInvoke() has to be called mandatory.
        }

        private void MergeResults()
        {
            _logger.Info("Merging filter results.");
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
            _logger.Info("Merging done.");
        }

        #endregion

        private delegate Filter WorkerFx(FilterParams filterParams, int startLine, int maxCount,
            ProgressCallback callback);
    }
}