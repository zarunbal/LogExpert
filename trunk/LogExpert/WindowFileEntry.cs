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

    public WindowFileEntry(LogWindow logWindow)
    {
      this.logWindow = logWindow;
    }

    public String Title 
    {
      get 
      {
        String title = this.LogWindow.Text;
        if (title.Length > 40)
        {
          title = "..." + title.Substring(title.Length - 50);
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
