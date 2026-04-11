namespace SftpFileSystem;

public partial class FailedKeyDialog : Form
{
    #region Ctor

    public FailedKeyDialog ()
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
        Text = Resources.FailedKeyDialog_UI_Title;
        label1.Text = Resources.FailedKeyDialog_UI_Label_Message;
        btnRetry.Text = Resources.FailedKeyDialog_UI_Button_Retry;
        btnUsePasswordAuthentication.Text = Resources.FailedKeyDialog_UI_Button_UsePassword;
        btnCancel.Text = Resources.FailedKeyDialog_UI_Button_Cancel;
    }

    #endregion

    #region Event handling Methods

    private void OnBtnCancelClick (object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void OnBtnRetryClick (object sender, EventArgs e)
    {
        DialogResult = DialogResult.Retry;
        Close();
    }

    private void OnBtnUsePasswordAuthenticationClick (object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    #endregion
}
