using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class ExceptionWindow : Form
    {
        #region Private Fields

        private readonly string errorText;
        private readonly string stackTrace;

        #endregion

        #region Ctor

        public ExceptionWindow(string errorText, string stackTrace)
        {
            InitializeComponent();
            this.errorText = errorText;
            this.stackTrace = stackTrace;
            stackTraceTextBox.Text = this.errorText + "\n\n" + this.stackTrace;
            stackTraceTextBox.Select(0, 0);
        }

        #endregion

        #region Private Methods

        private void copyButton_Click(object sender, EventArgs e)
        {
            CopyToClipboard();
        }

        private void CopyToClipboard()
        {
            Clipboard.SetText(errorText + "\n\n" + stackTrace);
        }

        #endregion
    }
}
