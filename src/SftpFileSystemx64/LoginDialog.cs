using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace SftpFileSystem;

public partial class LoginDialog : Form
{
    #region Private Fields

    private string _username;

    #endregion

    #region Ctor

    public LoginDialog (string host, IList<string> userNames, bool hidePasswordField)
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        serverNameLabel.Text = host;

        if (userNames != null)
        {
            foreach (var name in userNames)
            {
                if (name != null)
                {
                    _ = cmbUsername.Items.Add(name);
                }
            }
        }

        if (hidePasswordField)
        {
            txtBoxPassword.Enabled = false;
            lblPassword.Enabled = false;
        }

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.LoginDialog_UI_Title;
        label3.Text = Resources.LoginDialog_UI_Label_Server;
        label1.Text = Resources.LoginDialog_UI_Label_Username;
        lblPassword.Text = Resources.LoginDialog_UI_Label_Password;
        btnOk.Text = Resources.LoginDialog_UI_Button_OK;
        btnCancel.Text = Resources.LoginDialog_UI_Button_Cancel;
    }

    #endregion

    #region Properties / Indexers

    public string Password { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Username
    {
        get => _username;
        set
        {
            _username = value ?? string.Empty;
            cmbUsername.Text = value;
        }
    }

    #endregion

    #region Event handling Methods

    private void OnBtnOKClick (object sender, EventArgs e)
    {
        Password = txtBoxPassword.Text;
        _username = cmbUsername.Text;
    }

    private void OnLoginDialogLoad (object sender, EventArgs e)
    {
        if (cmbUsername.Text.Length > 0)
        {
            _ = txtBoxPassword.Focus();
        }
    }

    #endregion
}
