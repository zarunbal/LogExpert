using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class BookmarkCommentDlg : Form
    {
        #region cTor

        public BookmarkCommentDlg()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public string Comment
        {
            set { this.commentTextBox.Text = value; }
            get { return this.commentTextBox.Text; }
        }

        #endregion
    }
}