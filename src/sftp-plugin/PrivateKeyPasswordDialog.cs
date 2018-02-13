using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SftpFileSystem
{
  public partial class PrivateKeyPasswordDialog : Form
  {
    private string password;

    public PrivateKeyPasswordDialog()
    {
      InitializeComponent();
    }

    public string Password
    {
      get { return password; }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      this.password = this.passwordTextBox.Text;
    }

    private void LoginDialog_Load(object sender, EventArgs e)
    {
        this.passwordTextBox.Focus();
    }
  }
}
