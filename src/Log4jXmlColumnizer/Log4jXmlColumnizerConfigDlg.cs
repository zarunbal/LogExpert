using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert
{
  public partial class Log4jXmlColumnizerConfigDlg : Form
  {
    Log4jXmlColumnizerConfig config;

    public Log4jXmlColumnizerConfigDlg(Log4jXmlColumnizerConfig config)
    {
      this.config = config;
      InitializeComponent();
      FillListBox();
      this.localTimeCheckBox.Checked = this.config.localTimestamps;
    }

    private void FillListBox()
    {
      DataGridViewCheckBoxColumn checkColumn = (DataGridViewCheckBoxColumn) this.columnGridView.Columns[0];
      DataGridViewTextBoxColumn nameColumn = (DataGridViewTextBoxColumn) this.columnGridView.Columns[1];
      DataGridViewTextBoxColumn lenColumn = (DataGridViewTextBoxColumn) this.columnGridView.Columns[2];

      foreach (Log4jColumnEntry entry in config.columnList)
      {
        DataGridViewRow row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewCheckBoxCell());
        row.Cells.Add(new DataGridViewTextBoxCell());
        row.Cells.Add(new DataGridViewTextBoxCell());
        row.Cells[0].Value = entry.visible;
        row.Cells[1].Value = entry.columnName;
        row.Cells[2].Value = entry.maxLen > 0 ? ("" + entry.maxLen) : ""; 
        this.columnGridView.Rows.Add(row);
      }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
    //  for (int i = 0; i < this.config.columnList.Count; ++i)
    //  {
    //    this.config.columnList[i]. visible = this.columnListBox.GetItemChecked(i);
    //  }
      for (int i = 0; i < this.columnGridView.Rows.Count; ++i)
      {
        this.config.columnList[i].visible = (bool)this.columnGridView.Rows[i].Cells[0].Value;
        string sLen = (string)this.columnGridView.Rows[i].Cells[2].Value;
        int len;
        if (Int32.TryParse(sLen, out len))
        {
          this.config.columnList[i].maxLen = len;
        }
        else
        {
          this.config.columnList[i].maxLen = 0;
        }
      }
      this.config.localTimestamps = this.localTimeCheckBox.Checked;
    }


  }
}
