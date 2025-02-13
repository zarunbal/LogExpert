using System;
using System.Drawing;
using System.Windows.Forms;
using LogExpert.Classes.Filter;

namespace LogExpert.Dialogs
{
    public partial class FilterColumnChooser : Form
    {
        #region Fields

        private readonly ILogLineColumnizer _columnizer;
        private readonly FilterParams _filterParams;

        #endregion

        #region cTor

        public FilterColumnChooser(FilterParams filterParams)
        {
            InitializeComponent();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            columnListBox.ItemHeight = columnListBox.Font.Height;

            _columnizer = filterParams.currentColumnizer;
            _filterParams = filterParams;

            Init();
        }

        #endregion

        #region Private Methods

        private void Init()
        {
            int count = _columnizer.GetColumnCount();
            string[] names = _columnizer.GetColumnNames();

            for (int i = 0; i < count; ++i)
            {
                columnListBox.Items.Add(names[i], _filterParams.columnList.Contains(i));
            }

            emptyColumnUsePrevRadioButton.Checked = _filterParams.emptyColumnUsePrev;
            emptyColumnHitRadioButton.Checked = _filterParams.emptyColumnHit;
            emptyColumnNoHitRadioButton.Checked = _filterParams.emptyColumnHit == false && _filterParams.emptyColumnUsePrev == false;
            checkBoxExactMatch.Checked = _filterParams.exactColumnMatch;
        }

        #endregion

        #region Events handler

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            _filterParams.columnList.Clear();
            foreach (int colNum in columnListBox.CheckedIndices)
            {
                _filterParams.columnList.Add(colNum);
            }
            _filterParams.emptyColumnUsePrev = emptyColumnUsePrevRadioButton.Checked;
            _filterParams.emptyColumnHit = emptyColumnHitRadioButton.Checked;
            _filterParams.exactColumnMatch = checkBoxExactMatch.Checked;
        }

        #endregion
    }
}