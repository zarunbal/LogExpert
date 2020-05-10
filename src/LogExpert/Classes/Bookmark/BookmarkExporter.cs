using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace LogExpert
{
    internal static class BookmarkExporter
    {
        #region Fields

        private const string replacementForNewLine = @"\n";

        #endregion

        #region Public methods

        public static void ExportBookmarkList(SortedList<int, Bookmark> bookmarkList, string logfileName,
            string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            writer.WriteLine("Log file name;Line number;Comment");
            foreach (Bookmark bookmark in bookmarkList.Values)
            {
                string line = logfileName + ";" + bookmark.LineNum + ";" +
                              bookmark.Text.Replace(replacementForNewLine, @"\" + replacementForNewLine).Replace("\r\n",
                                  replacementForNewLine);
                writer.WriteLine(line);
            }
            writer.Close();
            fs.Close();
        }

        public static void ImportBookmarkList(string logfileName, string fileName,
            SortedList<int, Bookmark> bookmarkList)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    if (!reader.EndOfStream)
                    {
                        reader.ReadLine(); // skip "Log file name;Line number;Comment"
                    }

                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            string line = reader.ReadLine();
                            line = line.Replace(replacementForNewLine, "\r\n").Replace("\\\r\n", replacementForNewLine);

                            // Line is formatted: logfileName ";" bookmark.LineNum ";" bookmark.Text;
                            int firstSeparator = line.IndexOf(';');
                            int secondSeparator = line.IndexOf(';', firstSeparator + 1);

                            string fileStr = line.Substring(0, firstSeparator);
                            string lineStr = line.Substring(firstSeparator + 1, secondSeparator - firstSeparator - 1);
                            string comment = line.Substring(secondSeparator + 1);

                            int lineNum;
                            if (int.TryParse(lineStr, out lineNum))
                            {
                                Bookmark bookmark = new Bookmark(lineNum, comment);
                                bookmarkList.Add(lineNum, bookmark);
                            }
                            else
                            {
                                //!!!log error: skipping a line entry
                            }
                        }
                        catch
                        {
                            //!!!
                        }
                    }
                }
            }
        }

        #endregion
    }
}