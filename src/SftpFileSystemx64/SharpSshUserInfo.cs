using System;
using System.Collections.Generic;
using System.Text;
using Tamir.SharpSsh.jsch;
using System.Windows.Forms;

namespace SftpFileSystem
{
  internal class SharpSshUserInfo : UserInfo
  {
    private String password;
    private String userName;

    internal SharpSshUserInfo(string userName, string password)
    {  
      this.userName = userName;
      this.password = password;
    }


    public String getPassword() { return password; }

    public bool promptYesNo(String str)
    {
      DialogResult returnVal = MessageBox.Show(
        str,
        "LogExpert",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning);
      return (returnVal == DialogResult.Yes);
    }

    public String getPassphrase() { return null; }

    public bool promptPassphrase(String message) { return true; }

    public bool promptPassword(String message)
    {
      return true;
    }

    public void showMessage(String message)
    {
      MessageBox.Show(
        message,
        "LogExpert",
        MessageBoxButtons.OK,
        MessageBoxIcon.Asterisk);
    }
  }
}
