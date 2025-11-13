using System.Runtime.Versioning;

using LogExpert.UI.Extensions;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class ChooseIconDlg : Form
{
    #region Fields

    #endregion

    #region cTor

    public ChooseIconDlg (string fileName)
    {
        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        FileName = fileName;

        ApplyResources();
    }

    private void ApplyResources ()
    {
        Text = Resources.ChooseIconDialog_UI_Text;
        buttonChooseIconFile.Text = Resources.ChooseIconDialog_UI_Button_ChooseIconFile;
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;
    }

    #endregion

    #region Properties

    public string FileName { get; set; }

    public int IconIndex { get; set; }

    #endregion

    #region Private Methods

    private void FillIconList ()
    {
        iconListView.Items.Clear();

        var icons = NativeMethods.ExtractIcons(FileName);

        if (icons == null)
        {
            return;
        }

        ImageList imageList = new();

        if (icons.GetLength(0) > 0)
        {
            imageList.ImageSize = icons[1, 0].Size;
            iconListView.LargeImageList = imageList;

            for (var i = 0; i < icons.GetLength(1); ++i)
            {
                imageList.Images.Add(icons[1, i]);
                ListViewItem item = new()
                {
                    ImageIndex = i
                };

                _ = iconListView.Items.Add(item);
            }
        }
    }

    private void DisposeIcons ()
    {
        var imageList = iconListView.LargeImageList;
        iconListView.LargeImageList = null;
        foreach (Image image in imageList.Images)
        {
            image.Dispose();
        }
    }

    #endregion

    #region Events handler

    private void ChooseIconDlg_Load (object sender, EventArgs e)
    {
        FillIconList();
        iconFileLabel.Text = FileName;
    }

    private void OnButtonChooseIconFileClick (object sender, EventArgs e)
    {
        OpenFileDialog dlg = new()
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        };

        if (string.IsNullOrEmpty(iconFileLabel.Text) == false)
        {
            FileInfo info = new(iconFileLabel.Text);
            if (info.Directory != null && info.Directory.Exists)
            {
                dlg.InitialDirectory = info.DirectoryName;
            }
        }

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            iconFileLabel.Text = dlg.FileName;
            FileName = dlg.FileName;
            FillIconList();
        }
    }

    private void OnOkButtonClick (object sender, EventArgs e)
    {
        IconIndex = iconListView.SelectedIndices.Count > 0 ? iconListView.SelectedIndices[0] : -1;

        DisposeIcons();
    }

    #endregion
}