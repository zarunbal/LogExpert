using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  public class FileViewContext
  {
    private ILogPaintContext logPaintContext;
    private ILogView logView;

    internal FileViewContext(ILogPaintContext logPaintContext, ILogView logView)
    {
      this.logPaintContext = logPaintContext;
      this.logView = logView;
    }

    public ILogPaintContext LogPaintContext
    {
      get { return logPaintContext; }
    }

    public ILogView LogView
    {
      get { return logView; }
    }
  }
}
