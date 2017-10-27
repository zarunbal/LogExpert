using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CsvColumnizer
{
    public partial class CsvColumnizerConfigDlg : Form
    {
        #region Fields

        private readonly CsvColumnizerConfig config;

        #endregion

        #region cTor

        public CsvColumnizerConfigDlg(CsvColumnizerConfig config)
        {
            this.config = config;
            InitializeComponent();
            fillValues();
        }

        #endregion

        #region Private Methods

        private void fillValues()
        {
            this.delimiterTextBox.Text = "" + this.config.delimiterChar;
            this.quoteCharTextBox.Text = "" + this.config.quoteChar;
            this.escapeCharTextBox.Text = "" + this.config.escapeChar;
            this.escapeCheckBox.Checked = this.config.escapeChar != '\0';
            this.commentCharTextBox.Text = "" + this.config.commentChar;
            this.fieldNamesCheckBox.Checked = this.config.hasFieldNames;
            this.escapeCharTextBox.Enabled = this.escapeCheckBox.Checked;
            this.minColumnsNumericUpDown.Value = this.config.minColumns;
        }

        private void retrieveValues()
        {
            this.config.delimiterChar = this.delimiterTextBox.Text[0];
            this.config.quoteChar = this.quoteCharTextBox.Text[0];
            this.config.escapeChar = this.escapeCheckBox.Checked ? this.escapeCharTextBox.Text[0] : '\0';
            this.config.commentChar = this.commentCharTextBox.Text[0];
            this.config.hasFieldNames = this.fieldNamesCheckBox.Checked;
            this.config.minColumns = (int) this.minColumnsNumericUpDown.Value;
        }

        #endregion

        #region Events handler

        private void okButton_Click(object sender, EventArgs e)
        {
            retrieveValues();
        }

        private void escapeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.escapeCharTextBox.Enabled = this.escapeCheckBox.Checked;
        }

        #endregion
    }
}