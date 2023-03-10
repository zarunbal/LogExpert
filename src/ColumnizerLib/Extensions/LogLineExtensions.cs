namespace LogExpert
{
    public static class LogLineExtensions
    {
        public static string ToClipBoardText(this ILogLine logLine)
        {
            return "\t" + (logLine.LineNumber + 1).ToString() + "\t" + logLine.FullLine;
        }
    }
}