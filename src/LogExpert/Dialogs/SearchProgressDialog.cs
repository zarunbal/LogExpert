using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class SearchProgressDialog : Form
    {
        #region Ctor

        public SearchProgressDialog()
        {
            InitializeComponent();
            ShouldStop = false;
        }

        #endregion

        #region Properties / Indexers

        public bool ShouldStop { get; private set; }

        #endregion

        #region Private Methods

        private void cancelButton_Click(object sender, EventArgs e)
        {
            ShouldStop = true;
        }

        #endregion
    }
}
