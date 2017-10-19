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
    CsvColumnizerConfig config;

    public CsvColumnizerConfigDlg(CsvColumnizerConfig config)
    {
      this.config = config;
      InitializeComponent();
      fillValues();
    }

    private void fillValues()
    {
      this.delimiterTextBox.Text = "" + this.config.delimiterChar;
      this.quoteCharTextBox.Text = "" + this.config.quoteChar;
      this.escapeCharTextBox.Text = "" + this.config.escapeChar;
      this.escapeCheckBox.Checked = this.config.escapeChar != '\0';
      this.commentCharTextBox.Text = "" + this.config.commentChar;
      this.fieldNamesCheckBox.Checked = this.config.hasFieldNames;
      this.escapeCharTextBox.Enabled = this.escapeCheckBox.Checked;
    }

    private void retrieveValues()
    {
      this.config.delimiterChar = this.delimiterTextBox.Text[0];
      this.config.quoteChar = this.quoteCharTextBox.Text[0];
      this.config.escapeChar = this.escapeCheckBox.Checked ? this.escapeCharTextBox.Text[0] : '\0';
      this.config.commentChar = this.commentCharTextBox.Text[0];
      this.config.hasFieldNames = this.fieldNamesCheckBox.Checked;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      retrieveValues();
    }

    private void escapeCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      this.escapeCharTextBox.Enabled = this.escapeCheckBox.Checked;
    }

  }
}
