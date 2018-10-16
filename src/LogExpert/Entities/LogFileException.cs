using System;

namespace LogExpert
{
    internal class LogFileException : ApplicationException
    {
        #region Ctor

        internal LogFileException(string msg)
            : base(msg)
        {
        }

        internal LogFileException(string msg, Exception inner)
            : base(msg, inner)
        {
        }

        #endregion
    }
}
