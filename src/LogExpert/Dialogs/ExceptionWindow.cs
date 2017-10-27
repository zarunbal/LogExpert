using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class ExceptionWindow : Form
    {
        #region Fields

        private readonly string errorText;
        private readonly string stackTrace;

        #endregion

        #region cTor

        public ExceptionWindow(string errorText, string stackTrace)
        {
            InitializeComponent();
            this.errorText = errorText;
            this.stackTrace = stackTrace;
            this.stackTraceTextBox.Text = this.errorText + "\n\n" + this.stackTrace;
            this.stackTraceTextBox.Select(0, 0);
        }

        #endregion

        #region Private Methods

        private void CopyToClipboard()
        {
            Clipboard.SetText(this.errorText + "\n\n" + this.stackTrace);
        }

        #endregion

        #region Events handler

        private void copyButton_Click(object sender, EventArgs e)
        {
            CopyToClipboard();
        }

        #endregion
    }
}