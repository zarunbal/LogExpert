using System;
using System.Collections.Generic;
using System.Drawing;
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

        public string Uri => cmbUri.Text;

        public IList<string> UriHistory { get; set; }

        #endregion

        #region Events handler

        private void OnOpenUriDialogLoad(object sender, EventArgs e)
        {
            if (UriHistory != null)
            {
                cmbUri.Items.Clear();
                foreach (string uri in UriHistory)
                {
                    cmbUri.Items.Add(uri);
                }
            }
        }

        private void OnBtnOkClick(object sender, EventArgs e)
        {
            UriHistory = new List<string>();
            foreach (object item in cmbUri.Items)
            {
                UriHistory.Add(item.ToString());
            }
            if (UriHistory.Contains(cmbUri.Text))
            {
                UriHistory.Remove(cmbUri.Text);
            }
            UriHistory.Insert(0, cmbUri.Text);

            while (UriHistory.Count > 20)
            {
                UriHistory.RemoveAt(UriHistory.Count - 1);
            }
        }

        #endregion
    }
}