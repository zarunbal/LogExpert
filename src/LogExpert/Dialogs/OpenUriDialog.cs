using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class OpenUriDialog : Form
    {
        #region Ctor

        public OpenUriDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public string Uri => uriComboBox.Text;

        public IList<string> UriHistory { get; set; }

        #endregion

        #region Private Methods

        private void okButton_Click(object sender, EventArgs e)
        {
            UriHistory = new List<string>();
            foreach (object item in uriComboBox.Items)
            {
                UriHistory.Add(item.ToString());
            }

            if (UriHistory.Contains(uriComboBox.Text))
            {
                UriHistory.Remove(uriComboBox.Text);
            }

            UriHistory.Insert(0, uriComboBox.Text);
            while (UriHistory.Count > 20)
            {
                UriHistory.RemoveAt(UriHistory.Count - 1);
            }
        }

        private void OpenUriDialog_Load(object sender, EventArgs e)
        {
            if (UriHistory != null)
            {
                uriComboBox.Items.Clear();
                foreach (string uri in UriHistory)
                {
                    uriComboBox.Items.Add(uri);
                }
            }
        }

        #endregion
    }
}
