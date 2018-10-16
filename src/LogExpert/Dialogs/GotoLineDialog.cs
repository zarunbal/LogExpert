using System;
using System.Windows.Forms;

// using System.Linq;
namespace LogExpert.Dialogs
{
    public partial class GotoLineDialog : Form
    {
        #region Ctor

        public GotoLineDialog(Form parent)
        {
            InitializeComponent();
            Owner = parent;
        }

        #endregion

        #region Properties / Indexers

        public int Line { get; private set; }

        #endregion

        #region Private Methods

        private void GotoLineDialog_Load(object sender, EventArgs e)
        {
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                Line = int.Parse(lineNumberTextBox.Text);
            }
            catch (Exception)
            {
                Line = -1;
            }
        }

        #endregion
    }
}
