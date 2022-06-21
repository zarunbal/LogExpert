using System;

namespace LogExpert.Entities
{
    internal class LogFileException : ApplicationException
    {
        #region cTor

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