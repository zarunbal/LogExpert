using System;
using System.Collections.Generic;
using LogExpert;

namespace PipeExample
{
  /// <summary>
  /// This is a (rather useless) example implementaion of the IContextMenuEntry interface.<br></br>
  /// This plugin will add a context menu entry to LogExpert. If you select the menu entry
  /// all currently selected line will be written into a new tab. Additionally the lines will
  /// be uppercased (just for fun). All lines will keep a reference to its original location in
  /// the log file. You you can use the "locate line in original file" from the context menu on 
  /// the new tab to navigate to the appropriate line in the log file.<br></br><br></br>
  /// Usage:<br></br>
  /// Compile the project and then copy the resulting 'ContextMenu.dll' to LogExpert's
  /// plugin folder.
  /// </summary>
  public class PipeExample : IContextMenuEntry
  {

    #region IContextMenuEntry Member
    
    /// <summary>
    /// The function which gets called to give us a chance to decide whether to show a context menu entry.
    /// </summary>
    public string GetMenuText(IList<int> logLines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
    {
      // no additional checks. always show the menu entry
      return "(Example) Write lines to tab";
    }

    /// <summary>
    /// The worker function which is called when the entry is choosen by the user.
    /// What we do here: Take all lines, uppercase the content, keep track of original line numbers, 
    /// write it to a new tab.
    /// </summary>
    public void MenuSelected(IList<int> logLines, ILogLineColumnizer columnizer, ILogExpertCallback callback)
    {
      List<LineEntry> lineEntryList = new List<LineEntry>();
      foreach(int lineNum in logLines)
      {
        LineEntry newEntry = new LineEntry();
        newEntry.logLine = callback.GetLogLine(lineNum).ToUpper();
        newEntry.lineNum = lineNum;
        lineEntryList.Add(newEntry);
      }
      callback.AddPipedTab(lineEntryList, callback.GetTabTitle() + " (uppercased)");
    }

    #endregion

    /// <summary>
    /// Let LogExpert show a nice name in the plugin settings
    /// </summary>
    public string Text
    {
      get { return "Example uppercaser"; }
    }
  }
}