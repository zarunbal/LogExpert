using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class BookmarkCommentDlg : Form
    {
        #region Ctor

        public BookmarkCommentDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public string Comment
        {
            get => commentTextBox.Text;
            set => commentTextBox.Text = value;
        }

        #endregion
    }
}
