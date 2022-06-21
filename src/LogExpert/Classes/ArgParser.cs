using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LogExpert.Dialogs;

namespace LogExpert.Classes
{
    internal class ArgParser
    {
        #region Fields

        private readonly string argLine;

        #endregion

        #region cTor

        public ArgParser(string argTemplate)
        {
            this.argLine = argTemplate;
        }

        #endregion

        #region Public methods

        public string BuildArgs(ILogLine logLine, int lineNum, ILogFileInfo logFileInfo, Form parent)
        {
            StringBuilder builder = new StringBuilder(this.argLine);
            builder.Replace("%L", "" + lineNum);
            builder.Replace("%P", logFileInfo.DirectoryName);
            builder.Replace("%N", logFileInfo.FileName);
            builder.Replace("%F", logFileInfo.FullName);
            builder.Replace("%E", Util.GetExtension(logFileInfo.FileName));
            string stripped = Util.StripExtension(logFileInfo.FileName);
            builder.Replace("%M", stripped);

            builder.Replace("%URI", logFileInfo.Uri.AbsoluteUri);
            string user = logFileInfo.Uri.UserInfo;
            if (user.Contains(":"))
            {
                user = user.Substring(0, user.IndexOf(':'));
            }
            builder.Replace("%S", user);
            builder.Replace("%R", logFileInfo.Uri.PathAndQuery);
            builder.Replace("%H", logFileInfo.Uri.Host);
            builder.Replace("%T", logFileInfo.Uri.Port.ToString());

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

            int i = 0;
            while (i < builder.Length)
            {
                // ?"Pinpad-type?"(thales,dione)
                if (builder[i] == '?')
                {
                    int end = i;
                    string ask = "Parameter";
                    if (builder[i + 1] == '"')
                    {
                        end = builder.ToString().IndexOf('"', i + 2);
                        if (end == -1)
                        {
                            end = builder.Length - 1;
                        }
                        ask = builder.ToString().Substring(i + 2, end - i - 2);
                    }
                    string[] values = null;
                    if (builder[end + 1] == '(')
                    {
                        int end2 = builder.ToString().IndexOf(')');
                        if (end2 == -1)
                        {
                            end2 = builder.Length - 1;
                        }
                        string valueStr = builder.ToString().Substring(end + 2, end2 - end - 2);
                        values = valueStr.Split(new char[] {','}, StringSplitOptions.None);
                        end = end2;
                    }

                    ParamRequesterDialog dlg = new ParamRequesterDialog();
                    dlg.ParamName = ask;
                    dlg.Values = values;
                    DialogResult res = dlg.ShowDialog(parent);
                    if (res == DialogResult.OK)
                    {
                        builder.Remove(i, end - i + 1);
                        builder.Insert(i, dlg.ParamValue);
                    }
                    else if (res == DialogResult.Cancel || res == DialogResult.Abort)
                    {
                        return null;
                    }
                }
                ++i;
            }

            return builder.ToString();
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