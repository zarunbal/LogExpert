using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogExpert
{
  public class HighlightThread
  {
    private const int FILTER_COUNT = 300;
    private LogExpert.LogWindow.ColumnizerCallback callback;
    private bool shouldStop = true;
    private bool newList = false;
    private Thread  thread;
    private IList<HilightEntry> highlightEntryList;
    private Dictionary<int, HighlightResults> highlightDict = new Dictionary<int, HighlightResults>();
    private int startLine = 0;
    private Object listLock = new Object();

    // triggered when a new highlight list is set
    readonly EventWaitHandle newListEvent = new ManualResetEvent(false);

    public HighlightThread(LogExpert.LogWindow.ColumnizerCallback callback)
    {
      this.callback = callback;
    }

    public HighlightResults GetHighlightsResult(int lineNum)
    {
      lock (this.highlightDict)
      {
        if (this.highlightDict.ContainsKey(lineNum))
        {
          return this.highlightDict[lineNum];
        }
        else
        {
          return null;
        }
      }
    }


    public void Start()
    {
      lock (this)
      {
        this.shouldStop = false;
        this.thread = new Thread(new ThreadStart(this.Run));
        this.thread.Name = "Highlight Thread";
        this.thread.Priority = ThreadPriority.BelowNormal;
        this.thread.Start();
      }
    }


    public void Stop()
    {
      lock (this)
      {
        if (this.thread != null)
        {
          this.shouldStop = true;
        }
      }
      this.newListEvent.Set();
      this.thread.Abort();
      this.thread.Join();
    }

    public void SetHighlightEntryList(IList<HilightEntry> list)
    {
      this.startLine = 0;
      this.newList = true;
      lock (this.listLock)
      {
        this.highlightEntryList = new List<HilightEntry>();
        foreach (HilightEntry entry in list)
        {
          this.highlightEntryList.Add(entry);
        }
        this.newList = false;
      }
      this.newListEvent.Set();
    }

    public void CheckHighlightsFromLine(int startLine)
    {
      this.startLine = startLine;
      this.newListEvent.Set();
    }


    private void Run()
    {
      while (!this.shouldStop)
      {
        this.newListEvent.WaitOne();
        this.newListEvent.Reset();
        if (this.shouldStop)
        {
          break;
        }
        lock (this.highlightDict)
        {
          this.highlightDict.Clear();
        }
        int lineCount = this.callback.GetLineCount();

        lock (this.listLock)
        {
          foreach (HilightEntry searchEntry in this.highlightEntryList)
          {
            if (this.shouldStop || newList)
            {
              break;
            }
            if (searchEntry.FilterParams != null)
            {
              int startLine = 0;
              Filter filter = new Filter(callback);
              bool doFilter = true;
              while (doFilter && !newList)
              {
                //Thread.Sleep(5);
                doFilter = filter.DoFilter(searchEntry.FilterParams, startLine, FILTER_COUNT);
                IList<int> resultLines = filter.FilterResultLines;
                bool hasChanged = false;
                foreach (int line in resultLines)
                {
                  hasChanged = true;
                  if (this.shouldStop)
                  {
                    break;
                  }
                  HighlightResults results;
                  if (!this.highlightDict.ContainsKey(line))
                  {
                    lock (this.highlightDict)
                    {
                      this.highlightDict[line] = results = new HighlightResults();
                    }
                  }
                  else
                  {
                    results = this.highlightDict[line];
                  }
                  lock (results)
                  {
                    results.HighlightEntryList.Add(searchEntry);
                  }
                }
                if (hasChanged)
                {
                  OnHighlightDone(new HighlightEventArgs(startLine, FILTER_COUNT));
                }
                startLine += FILTER_COUNT;
              }
            }
          }
        }
      }
    }

    public delegate void HighlightDoneEventHandler(object sender, HighlightEventArgs e);
    public event HighlightDoneEventHandler HighlightDoneEvent;
    protected void OnHighlightDone(HighlightEventArgs e)
    {
      HighlightDoneEventHandler handler = HighlightDoneEvent;
      if (handler != null)
      {
        handler(this, e);
      }
    }


  }
}
