using System;
using System.Windows.Forms;

namespace LogExpert
{
    public partial class Log4jXmlColumnizerConfigDlg : Form
    {
        #region Private Fields

        private readonly Log4jXmlColumnizerConfig config;

        #endregion

        #region Ctor

        public Log4jXmlColumnizerConfigDlg(Log4jXmlColumnizerConfig config)
        {
            this.config = config;
            InitializeComponent();
            FillListBox();
            localTimeCheckBox.Checked = this.config.localTimestamps;
        }

        #endregion

        #region Private Methods

        private void FillListBox()
        {
            DataGridViewCheckBoxColumn checkColumn = (DataGridViewCheckBoxColumn)columnGridView.Columns[0];
            DataGridViewTextBoxColumn nameColumn = (DataGridViewTextBoxColumn)columnGridView.Columns[1];
            DataGridViewTextBoxColumn lenColumn = (DataGridViewTextBoxColumn)columnGridView.Columns[2];

            foreach (Log4jColumnEntry entry in config.columnList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewCheckBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells[0].Value = entry.visible;
                row.Cells[1].Value = entry.columnName;
                row.Cells[2].Value = entry.maxLen > 0 ? string.Empty + entry.maxLen : string.Empty;
                columnGridView.Rows.Add(row);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // for (int i = 0; i < this.config.columnList.Count; ++i)
            // {
            // this.config.columnList[i]. visible = this.columnListBox.GetItemChecked(i);
            // }
            for (int i = 0; i < columnGridView.Rows.Count; ++i)
            {
                config.columnList[i].visible = (bool)columnGridView.Rows[i].Cells[0].Value;
                string sLen = (string)columnGridView.Rows[i].Cells[2].Value;
                int len;
                if (int.TryParse(sLen, out len))
                {
                    config.columnList[i].maxLen = len;
                }
                else
                {
                    config.columnList[i].maxLen = 0;
                }
            }

            config.localTimestamps = localTimeCheckBox.Checked;
        }

        #endregion
    }
}
