using System.Drawing;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class TabRenameDlg : Form
    {
        #region cTor

        public TabRenameDlg()
        {
            InitializeComponent();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        #endregion

        #region Properties

        public string TabName
        {
            get => textBoxTabName.Text;
            set => textBoxTabName.Text = value;
        }

        #endregion

        #region Events handler

        private void OnTabRenameDlgKeyDown(object sender, KeyEventArgs e)
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