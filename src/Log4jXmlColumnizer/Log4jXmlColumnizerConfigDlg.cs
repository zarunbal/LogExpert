using System;
using System.Windows.Forms;

namespace LogExpert
{
    public partial class Log4jXmlColumnizerConfigDlg : Form
    {
        #region Fields

        private readonly Log4jXmlColumnizerConfig _config;

        #endregion

        #region cTor

        public Log4jXmlColumnizerConfigDlg(Log4jXmlColumnizerConfig config)
        {
            _config = config;
            InitializeComponent();
            FillListBox();
            localTimeCheckBox.Checked = _config.localTimestamps;
        }

        #endregion

        #region Private Methods

        private void FillListBox()
        {
            DataGridViewCheckBoxColumn checkColumn = (DataGridViewCheckBoxColumn)columnGridView.Columns[0];
            DataGridViewTextBoxColumn nameColumn = (DataGridViewTextBoxColumn)columnGridView.Columns[1];
            DataGridViewTextBoxColumn lenColumn = (DataGridViewTextBoxColumn)columnGridView.Columns[2];

            foreach (Log4jColumnEntry entry in _config.columnList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewCheckBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells[0].Value = entry.visible;
                row.Cells[1].Value = entry.columnName;
                row.Cells[2].Value = entry.maxLen > 0 ? "" + entry.maxLen : "";
                columnGridView.Rows.Add(row);
            }
        }

        #endregion

        #region Events handler

        private void OkButton_Click(object sender, EventArgs e)
        {
            //  for (int i = 0; i < this.config.columnList.Count; ++i)
            //  {
            //    this.config.columnList[i]. visible = this.columnListBox.GetItemChecked(i);
            //  }
            for (int i = 0; i < columnGridView.Rows.Count; ++i)
            {
                _config.columnList[i].visible = (bool)columnGridView.Rows[i].Cells[0].Value;
                string sLen = (string)columnGridView.Rows[i].Cells[2].Value;
                int len;
                if (int.TryParse(sLen, out len))
                {
                    _config.columnList[i].maxLen = len;
                }
                else
                {
                    _config.columnList[i].maxLen = 0;
                }
            }
            _config.localTimestamps = localTimeCheckBox.Checked;
        }

        #endregion
    }
}