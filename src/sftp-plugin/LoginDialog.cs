using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SftpFileSystem
{
  public partial class LoginDialog : Form
  {
    private String userName;
    private String password;


    public LoginDialog(string host, IList<string> userNames, bool hidePasswordField)
    {
      InitializeComponent();
      this.serverNameLabel.Text = host;
      if (userNames != null)
      {
        foreach (string name in userNames)
        {
          if (name != null)
          {
            this.userNameComboBox.Items.Add(name);
          }
        }
      }
      if (hidePasswordField)
      {
        this.passwordTextBox.Enabled = false;
        this.passwordLabel.Enabled = false;
      }
    }

    public string UserName
    {
      get { return userName; }
      set
      {
        userName = value != null ? value : "";
        this.userNameComboBox.Text = value;
      }
    }

    public string Password
    {
      get { return password; }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      this.password = this.passwordTextBox.Text;
      this.userName = this.userNameComboBox.Text;
    }

    private void LoginDialog_Load(object sender, EventArgs e)
    {
      if (this.userNameComboBox.Text.Length > 0)
      {
        this.passwordTextBox.Focus();
      }
    }
  }
}
