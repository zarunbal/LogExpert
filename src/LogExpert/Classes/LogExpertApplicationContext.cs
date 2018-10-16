using System;
using System.Windows.Forms;

namespace LogExpert
{
    internal class LogExpertApplicationContext : ApplicationContext
    {
        #region Private Fields

        private readonly LogExpertProxy proxy;

        #endregion

        #region Ctor

        public LogExpertApplicationContext(LogExpertProxy proxy, LogTabWindow firstLogWin)
        {
            this.proxy = proxy;
            this.proxy.LastWindowClosed += proxy_LastWindowClosed;
            firstLogWin.Show();
        }

        #endregion

        #region Private Methods

        private void proxy_LastWindowClosed(object sender, EventArgs e)
        {
            ExitThread();
            Application.Exit();
        }

        #endregion
    }
}
