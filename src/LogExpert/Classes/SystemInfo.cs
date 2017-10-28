using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    /// <summary>
    /// Collects some system information (OS, Runtime etc.)
    /// </summary>
    internal class SystemInfo
    {
        #region Fields

        private readonly StringBuilder info = new StringBuilder();

        #endregion

        #region cTor

        internal SystemInfo()
        {
            info.Append("OS:  ").AppendLine(System.Environment.OSVersion.ToString());
            info.Append("CLR: ").AppendLine(System.Environment.Version.ToString());
        }

        #endregion

        #region Properties

        public string Info
        {
            get { return this.info.ToString(); }
        }

        #endregion
    }
}