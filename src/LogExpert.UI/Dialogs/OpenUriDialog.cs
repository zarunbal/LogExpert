using System.ComponentModel;
using System.Runtime.Versioning;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class OpenUriDialog : Form
{
    #region Fields

    #endregion

    #region cTor

    public OpenUriDialog ()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        ResumeLayout();
    }

    #endregion

    #region Properties

    //TODO Convert to System.Uri
    public string Uri => cmbUri.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public IList<string> UriHistory { get; set; }

    #endregion

    #region Events handler

    private void ApplyResources ()
    {
        Text = Resources.OpenUriDialog_UI_Dialog_Text;
        labelUrl.Text = Resources.OpenUriDialog_UI_Label_URL;
        okButton.Text = Resources.LogExpert_Common_UI_Button_OK;
        cancelButton.Text = Resources.LogExpert_Common_UI_Button_Cancel;
        labelExplaination.Text = Resources.OpenUriDialog_UI_Label_Explaination;

    }

    private void OnOpenUriDialogLoad (object sender, EventArgs e)
    {
        if (UriHistory != null)
        {
            cmbUri.Items.Clear();
            foreach (var uri in UriHistory)
            {
                _ = cmbUri.Items.Add(uri);
            }
        }
    }

    private void OnBtnOkClick (object sender, EventArgs e)
    {
        UriHistory = [];

        foreach (var item in cmbUri.Items)
        {
            UriHistory.Add(item.ToString());
        }

        if (UriHistory.Contains(cmbUri.Text))
        {
            _ = UriHistory.Remove(cmbUri.Text);

        }

        UriHistory.Insert(0, cmbUri.Text);

        while (UriHistory.Count > 20)
        {
            UriHistory.RemoveAt(UriHistory.Count - 1);
        }
    }

    #endregion
}