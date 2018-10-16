using NLog;

namespace LogExpert
{
    internal class FileSystemCallback : IFileSystemCallback
    {
        #region Interface IFileSystemCallback

        public ILogExpertLogger GetLogger()
        {
            return new NLogLogExpertWrapper();
        }

        #endregion

        #region Nested type: NLogLogExpertWrapper

        private class NLogLogExpertWrapper : ILogExpertLogger
        {
            private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

            #region Interface ILogExpertLogger

            public void Debug(string msg)
            {
                _logger.Debug(msg);
            }

            public void Info(string msg)
            {
                _logger.Info(msg);
            }

            public void LogError(string msg)
            {
                _logger.Error(msg);
            }

            public void LogWarn(string msg)
            {
                _logger.Warn(msg);
            }

            #endregion
        }

        #endregion
    }
}
