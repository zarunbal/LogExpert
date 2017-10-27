using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class GotoLineDialog : Form
    {
        #region Fields

        #endregion

        #region cTor

        public GotoLineDialog(Form parent)
        {
            InitializeComponent();
            this.Owner = parent;
        }

        #endregion

        #region Properties

        public int Line { get; private set; }

        #endregion

        #region Events handler

        private void GotoLineDialog_Load(object sender, EventArgs e)
        {
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.Line = int.Parse(this.lineNumberTextBox.Text);
            }
            catch (Exception)
            {
                this.Line = -1;
            }
        }

        #endregion
    }
}