using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LogExpert.Classes
{
    internal class ParamParser
    {
        #region Fields

        private readonly string argLine;

        #endregion

        #region cTor

        public ParamParser(string argTemplate)
        {
            this.argLine = argTemplate;
        }

        #endregion

        #region Public methods

        public string ReplaceParams(ILogLine logLine, int lineNum, string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            StringBuilder builder = new StringBuilder(this.argLine);
            builder.Replace("%L", "" + lineNum);
            builder.Replace("%P",
                fileInfo.DirectoryName.Contains(" ") ? "\"" + fileInfo.DirectoryName + "\"" : fileInfo.DirectoryName);
            builder.Replace("%N", fileInfo.Name.Contains(" ") ? "\"" + fileInfo.Name + "\"" : fileInfo.Name);
            builder.Replace("%F",
                fileInfo.FullName.Contains(" ") ? "\"" + fileInfo.FullName + "\"" : fileInfo.FullName);
            builder.Replace("%E",
                fileInfo.Extension.Contains(" ") ? "\"" + fileInfo.Extension + "\"" : fileInfo.Extension);
            string stripped = StripExtension(fileInfo.Name);
            builder.Replace("%M", stripped.Contains(" ") ? "\"" + stripped + "\"" : stripped);
            int sPos = 0;
            string reg;
            string replace;
            do
            {
                reg = GetNextGroup(builder, ref sPos);
                replace = GetNextGroup(builder, ref sPos);
                if (reg != null && replace != null)
                {
                    string result = Regex.Replace(logLine.FullLine, reg, replace);
                    builder.Insert(sPos, result);
                }
            } while (replace != null);
            return builder.ToString();
        }

        public static string StripExtension(string fileName)
        {
            int i = fileName.LastIndexOf('.');
            if (i < 0)
            {
                i = fileName.Length - 1;
            }
            return fileName.Substring(0, i);
        }

        #endregion

        #region Private Methods

        private string GetNextGroup(StringBuilder builder, ref int sPos)
        {
            int count = 0;
            int ePos;
            while (sPos < builder.Length)
            {
                if (builder[sPos] == '{')
                {
                    ePos = sPos + 1;
                    count = 1;
                    while (ePos < builder.Length)
                    {
                        if (builder[ePos] == '{')
                        {
                            count++;
                        }
                        if (builder[ePos] == '}')
                        {
                            count--;
                        }
                        if (count == 0)
                        {
                            string reg = builder.ToString(sPos + 1, ePos - sPos - 1);
                            builder.Remove(sPos, ePos - sPos + 1);
                            return reg;
                        }
                        ePos++;
                    }
                }
                sPos++;
            }
            return null;
        }

        #endregion
    }
}