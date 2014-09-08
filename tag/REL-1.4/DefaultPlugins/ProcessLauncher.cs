using System.Diagnostics;
namespace LogExpert
{
  public class ProcessLauncher : IKeywordAction
  {
    #region IKeywordAction Member

    public void Execute(string keyword, string param, ILogExpertCallback callback, ILogLineColumnizer columnizer)
    {
      int start = 0;
      int end = 0;
      if (param.StartsWith("\""))
      {
        start = 1;
        end = param.IndexOf("\"", start);
      }
      else
      {
        end = param.IndexOf(" ");

      }
      if (end == -1)
        end = param.Length;
      string procName = param.Substring(start, end - start);
      string parameters = param.Substring(end).Trim();
      parameters = parameters.Replace("%F", callback.GetFileName());
      parameters = parameters.Replace("%K", keyword);
      parameters = parameters.Replace("%L", "" + callback.GetLineNum());
      parameters = parameters.Replace("%T", callback.GetTabTitle());
      parameters = parameters.Replace("%C", callback.GetLogLine(callback.GetLineNum()));
      Process explorer = new Process();
      explorer.StartInfo.FileName = procName;
      explorer.StartInfo.Arguments = parameters;
      explorer.StartInfo.UseShellExecute = false;
      explorer.Start();
    }

    public string GetName()
    {
      return "ProcessLauncher keyword plugin";
    }


    public string GetDescription()
    {
      return "Launches an external process. The plugin's parameter is the process name " +
        "and its (optional) command line.\r\n" +
        "Use the following variables for command line replacements:\r\n" +
        "%F = Log file name (full path)\r\n" +
        "%T = Tab title\r\n" +
        "%L = Line number of keyword hit\r\n" +
        "%K = Keyword\r\n" +
        "%C = Complete line content";
    }

    #endregion

    public string Text
    {
      get { return GetName(); }
    }
  }
}