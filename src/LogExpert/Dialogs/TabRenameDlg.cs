using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class TabRenameDlg : Form
    {
        #region Ctor

        public TabRenameDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public string TabName
        {
            get => tabNameTextBox.Text;
            set => tabNameTextBox.Text = value;
        }

        #endregion

        #region Event raising Methods

        private void TabRenameDlg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        #endregion
    }
}
