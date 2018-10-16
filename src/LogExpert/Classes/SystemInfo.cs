using System;
using System.Text;

namespace LogExpert
{
    /// <summary>
    ///     Collects some system information (OS, Runtime etc.)
    /// </summary>
    internal class SystemInfo
    {
        #region Private Fields

        private readonly StringBuilder info = new StringBuilder();

        #endregion

        #region Ctor

        internal SystemInfo()
        {
            info.Append("OS:  ").AppendLine(Environment.OSVersion.ToString());
            info.Append("CLR: ").AppendLine(Environment.Version.ToString());
        }

        #endregion

        #region Properties / Indexers

        public string Info => info.ToString();

        #endregion
    }
}
