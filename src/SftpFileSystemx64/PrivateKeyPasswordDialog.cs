namespace SftpFileSystem;

public partial class PrivateKeyPasswordDialog : Form
{
    #region Ctor

    public PrivateKeyPasswordDialog ()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        ResumeLayout();
    }

    private void ApplyResources ()
    {
        Text = Resources.PrivateKeyPasswordDialog_UI_Title;
        label2.Text = Resources.PrivateKeyPasswordDialog_UI_Label_Password;
        btnOk.Text = Resources.PrivateKeyPasswordDialog_UI_Button_OK;
        btnCancel.Text = Resources.PrivateKeyPasswordDialog_UI_Button_Cancel;
    }

    #endregion

    #region Properties / Indexers

    public string Password { get; private set; }

    #endregion

    #region Event handling Methods

    private void OnLoginDialogLoad (object sender, EventArgs e)
    {
        _ = passwordTextBox.Focus();
    }

    private void OnBtnOkClick (object sender, EventArgs e)
    {
        Password = passwordTextBox.Text;
    }

    #endregion
}
