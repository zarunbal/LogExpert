using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
  /// <summary>
  /// Represents a log file and its window. Used as a kind of handle for menus or list of open files.
  /// </summary>
  public class WindowFileEntry
  {
    private LogWindow logWindow;
    private const int MAX_LEN = 40;

    public WindowFileEntry(LogWindow logWindow)
    {
      this.logWindow = logWindow;
    }

    public String Title 
    {
      get 
      {
        String title = this.LogWindow.Text;
        if (title.Length > MAX_LEN)
        {
          title = "..." + title.Substring(title.Length - MAX_LEN);
        }
        return title;
      }
    }

    public String FileName
    {
      get { return this.logWindow.FileName; }
    }


    public LogWindow LogWindow
    {
      get { return this.logWindow; }
    }
  }
}
