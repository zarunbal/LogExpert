using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LogExpert.Classes.Filter;

namespace LogExpert.Dialogs
{
    public partial class FilterColumnChooser : Form
    {
        #region Fields

        private readonly ILogLineColumnizer columnizer;
        private readonly FilterParams filterParams;

        #endregion

        #region cTor

        public FilterColumnChooser(FilterParams filterParams)
        {
            InitializeComponent();

            this.columnizer = filterParams.currentColumnizer;
            this.filterParams = filterParams;
            Init();
        }

        #endregion

        #region Private Methods

        private void Init()
        {
            int count = this.columnizer.GetColumnCount();
            string[] names = this.columnizer.GetColumnNames();
            for (int i = 0; i < count; ++i)
            {
                this.columnListBox.Items.Add(names[i], filterParams.columnList.Contains(i));
            }
            this.emptyColumnUsePrevRadioButton.Checked = this.filterParams.emptyColumnUsePrev;
            this.emptyColumnHitRadioButton.Checked = this.filterParams.emptyColumnHit;
            this.emptyColumnNoHitRadioButton.Checked =
                !this.filterParams.emptyColumnHit && !this.filterParams.emptyColumnUsePrev;
            this.exactMatchCheckBox.Checked = this.filterParams.exactColumnMatch;
        }

        #endregion

        #region Events handler

        private void okButton_Click(object sender, EventArgs e)
        {
            this.filterParams.columnList.Clear();
            foreach (int colNum in this.columnListBox.CheckedIndices)
            {
                this.filterParams.columnList.Add(colNum);
            }
            this.filterParams.emptyColumnUsePrev = this.emptyColumnUsePrevRadioButton.Checked;
            this.filterParams.emptyColumnHit = this.emptyColumnHitRadioButton.Checked;
            this.filterParams.exactColumnMatch = this.exactMatchCheckBox.Checked;
        }

        #endregion
    }
}