using System;
using System.Collections.Generic;
using System.Text;
using LogExpert;
using Tamir.SharpSsh.jsch;
using System.Windows.Forms;

namespace SftpFileSystem
{
  class SftpLogFileInfoSharpSSH : ILogFileInfo
  {
    private long originalFileLength = -1;

    private Uri uri;
    private string remoteFileName;

    private ChannelSftp sftpChannel;

    internal SftpLogFileInfoSharpSSH(Uri uri)
    {
      this.uri = uri;
      this.remoteFileName = uri.PathAndQuery;

      string userName = null;
      string password = null;
      if (uri.UserInfo != null && uri.UserInfo.Length > 0)
      {
        string[] split = uri.UserInfo.Split(new char[] { ':' });
        if (split.Length > 0)
          userName = split[0];
        if (split.Length > 1)
          password = split[1];
      }
      if (userName == null || password == null)
      {
        IList<string> userNames = new List<string>();
        LoginDialog dlg = new LoginDialog(uri.Host, userNames);
        dlg.UserName = userName;
        if (DialogResult.OK == dlg.ShowDialog())
        {
          password = dlg.Password;
          userName = dlg.UserName;
        }
      }

      UserInfo userInfo = new SharpSshUserInfo(userName, password);
      JSch jsch = new JSch();
      int port = uri.Port != -1 ? uri.Port : 22;
      Session session = jsch.getSession(userName, this.uri.Host, port);
      session.setUserInfo(userInfo);
      session.connect();
      Channel channel = session.openChannel("sftp");
      channel.connect();
      this.sftpChannel = (ChannelSftp)channel;
      SftpATTRS sftpAttrs = this.sftpChannel.lstat(this.remoteFileName);
      this.originalFileLength = sftpAttrs.getSize();

    }

    #region ILogFileInfo Member

    public System.IO.Stream OpenStream()
    {
      throw new NotImplementedException();
    }

    public string FileName
    {
      get { return this.remoteFileName; }
    }

    public long Length
    {
      get
      {
        SftpATTRS sftpAttrs = this.sftpChannel.lstat(this.remoteFileName);
        return sftpAttrs.getSize();
      }
    }

    public long OriginalLength
    {
      get { return this.originalFileLength; }
    }

    public bool FileExists
    {
      get
      {
        try
        {
          SftpATTRS sftpAttrs = this.sftpChannel.lstat(this.remoteFileName);
          return true;
        }
        catch (SftpException e)
        {
          return false;
        }
      }
    }

    #endregion

    #region ILogFileInfo Member


    public string FullName
    {
      get { throw new NotImplementedException(); }
    }

    public string DirectoryName
    {
      get { throw new NotImplementedException(); }
    }

    public char DirectorySeparatorChar
    {
      get { throw new NotImplementedException(); }
    }

    public Uri Uri
    {
      get { throw new NotImplementedException(); }
    }

    public int PollInterval
    {
      get { throw new NotImplementedException(); }
    }

    public bool FileHasChanged()
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
