using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;



namespace LogExpert
{
  class BookmarkExporter
  {

    public static void ExportBookmarkList(SortedList<int, Bookmark> bookmarkList, string logfileName, string fileName)
    {
      FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
      StreamWriter writer = new StreamWriter(fs);
      writer.WriteLine("Log file name;Line number;Comment");
      foreach (Bookmark bookmark in bookmarkList.Values)
      {
        string line = logfileName + ";" + bookmark.LineNum + ";" + bookmark.Text.Replace('\r', ' ').Replace('\n', ' ');
        writer.WriteLine(line);
      }
      writer.Close();
      fs.Close();
    }

  }
}