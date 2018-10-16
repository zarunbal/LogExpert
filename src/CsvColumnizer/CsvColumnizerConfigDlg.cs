using System;
using System.Windows.Forms;

namespace CsvColumnizer
{
    public partial class CsvColumnizerConfigDlg : Form
    {
        #region Private Fields

        private readonly CsvColumnizerConfig config;

        #endregion

        #region Ctor

        public CsvColumnizerConfigDlg(CsvColumnizerConfig config)
        {
            this.config = config;
            InitializeComponent();
            fillValues();
        }

        #endregion

        #region Private Methods

        private void escapeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            escapeCharTextBox.Enabled = escapeCheckBox.Checked;
        }

        private void fillValues()
        {
            delimiterTextBox.Text = string.Empty + config.delimiterChar;
            quoteCharTextBox.Text = string.Empty + config.quoteChar;
            escapeCharTextBox.Text = string.Empty + config.escapeChar;
            escapeCheckBox.Checked = config.escapeChar != '\0';
            commentCharTextBox.Text = string.Empty + config.commentChar;
            fieldNamesCheckBox.Checked = config.hasFieldNames;
            escapeCharTextBox.Enabled = escapeCheckBox.Checked;
            minColumnsNumericUpDown.Value = config.minColumns;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            retrieveValues();
        }

        private void retrieveValues()
        {
            config.delimiterChar = delimiterTextBox.Text[0];
            config.quoteChar = quoteCharTextBox.Text[0];
            config.escapeChar = escapeCheckBox.Checked ? escapeCharTextBox.Text[0] : '\0';
            config.commentChar = commentCharTextBox.Text[0];
            config.hasFieldNames = fieldNamesCheckBox.Checked;
            config.minColumns = (int)minColumnsNumericUpDown.Value;
        }

        #endregion
    }
}
