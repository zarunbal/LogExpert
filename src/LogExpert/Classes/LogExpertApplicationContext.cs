using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LogExpert
{
    internal class LogExpertApplicationContext : ApplicationContext
    {
        #region Fields

        private readonly LogExpertProxy proxy;

        #endregion

        #region cTor

        public LogExpertApplicationContext(LogExpertProxy proxy, LogTabWindow firstLogWin)
        {
            this.proxy = proxy;
            this.proxy.LastWindowClosed += new LogExpertProxy.LastWindowClosedEventHandler(proxy_LastWindowClosed);
            firstLogWin.Show();
        }

        #endregion

        #region Events handler

        private void proxy_LastWindowClosed(object sender, EventArgs e)
        {
            ExitThread();
            Application.Exit();
        }

        #endregion
    }
}