using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class OpenUriDialog : Form
    {
        #region Fields

        #endregion

        #region cTor

        public OpenUriDialog()
        {
            InitializeComponent();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        #endregion

        #region Properties

        public string Uri => comboBoxUri.Text;

        public IList<string> UriHistory { get; set; }

        #endregion

        #region Events handler

        private void OpenUriDialog_Load(object sender, EventArgs e)
        {
            if (UriHistory != null)
            {
                comboBoxUri.Items.Clear();
                foreach (string uri in UriHistory)
                {
                    comboBoxUri.Items.Add(uri);
                }
            }
        }

        private void OnButtonOkClick(object sender, EventArgs e)
        {
            UriHistory = new List<string>();

            foreach (object item in comboBoxUri.Items)
            {
                UriHistory.Add(item.ToString());
            }

            if (UriHistory.Contains(comboBoxUri.Text))
            {
                UriHistory.Remove(comboBoxUri.Text);
            }

            UriHistory.Insert(0, comboBoxUri.Text);

            while (UriHistory.Count > 20)
            {
                UriHistory.RemoveAt(UriHistory.Count - 1);
            }
        }

        #endregion
    }
}