using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
  public partial class ParamRequesterDialog : Form
  {
    string paramName;

    public string ParamName
    {
      get { return paramName; }
      set { paramName = value; }
    }
    string paramValue;

    public string ParamValue
    {
      get { return paramValue; }
      set { paramValue = value; }
    }

    string[] values;

    public string[] Values
    {
      get { return values; }
      set { values = value; }
    }


    public ParamRequesterDialog()
    {
      InitializeComponent();
    }

    private void ParamRequesterDialog_Shown(object sender, EventArgs e)
    {
      this.paramLabel.Text = ParamName;
      if (this.values != null)
      {
        foreach (string value in this.values)
        {
          this.valueComboBox.Items.Add(value);
        }
        this.valueComboBox.SelectedIndex = 0;
      }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      this.ParamValue = this.valueComboBox.Text;
    }

  }
}
