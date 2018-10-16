using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class ChooseIconDlg : Form
    {
        #region Ctor

        public ChooseIconDlg(string fileName)
        {
            InitializeComponent();
            FileName = fileName;
        }

        #endregion

        #region Properties / Indexers

        public string FileName { get; set; }

        public int IconIndex { get; set; }

        #endregion

        #region Private Methods

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (iconFileLabel.Text != null && iconFileLabel.Text.Length > 0)
            {
                FileInfo info = new FileInfo(iconFileLabel.Text);
                if (info.Directory.Exists)
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

        private void ChooseIconDlg_Load(object sender, EventArgs e)
        {
            FillIconList();
            iconFileLabel.Text = FileName;
        }

        private void DisposeIcons()
        {
            ImageList imageList = iconListView.LargeImageList;
            iconListView.LargeImageList = null;
            foreach (Image image in imageList.Images)
            {
                image.Dispose();
            }
        }

        private void FillIconList()
        {
            iconListView.Items.Clear();
            Icon[,] icons = Win32.ExtractIcons(FileName);
            if (icons == null)
            {
                return;
            }

            ImageList imageList = new ImageList();
            if (icons.GetLength(0) > 0)
            {
                imageList.ImageSize = icons[1, 0].Size;
                iconListView.LargeImageList = imageList;
                for (int i = 0; i < icons.GetLength(1); ++i)
                {
                    imageList.Images.Add(icons[1, i]);
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = i;
                    iconListView.Items.Add(item);
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            IconIndex = iconListView.SelectedIndices.Count > 0 ? iconListView.SelectedIndices[0] : -1;
            
            DisposeIcons();
        }

        #endregion
    }
}
