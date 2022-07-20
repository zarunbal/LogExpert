using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LogExpert.Classes;

namespace LogExpert.Dialogs
{
    public partial class ChooseIconDlg : Form
    {
        #region Fields

        #endregion

        #region cTor

        public ChooseIconDlg(string fileName)
        {
            InitializeComponent();
            this.FileName = fileName;
        }

        #endregion

        #region Properties

        public string FileName { get; set; }

        public int IconIndex { get; set; }

        #endregion

        #region Private Methods

        private void FillIconList()
        {
            this.iconListView.Items.Clear();
            Icon[,] icons = Win32.ExtractIcons(this.FileName);
            if (icons == null)
            {
                return;
            }

            ImageList imageList = new ImageList();
            if (icons.GetLength(0) > 0)
            {
                imageList.ImageSize = icons[1, 0].Size;
                this.iconListView.LargeImageList = imageList;
                for (int i = 0; i < icons.GetLength(1); ++i)
                {
                    imageList.Images.Add(icons[1, i]);
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = i;
                    this.iconListView.Items.Add(item);
                }
            }
        }

        private void DisposeIcons()
        {
            ImageList imageList = this.iconListView.LargeImageList;
            this.iconListView.LargeImageList = null;
            foreach (Image image in imageList.Images)
            {
                image.Dispose();
            }
        }

        #endregion

        #region Events handler

        private void ChooseIconDlg_Load(object sender, EventArgs e)
        {
            FillIconList();
            this.iconFileLabel.Text = this.FileName;
        }

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
                this.FileName = dlg.FileName;
                FillIconList();
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.IconIndex = this.iconListView.SelectedIndices.Count > 0 ? this.iconListView.SelectedIndices[0] : -1;
            ;
            DisposeIcons();
        }

        #endregion
    }
}