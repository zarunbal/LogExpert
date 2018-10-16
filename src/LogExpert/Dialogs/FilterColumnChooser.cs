using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class FilterColumnChooser : Form
    {
        #region Private Fields

        private readonly ILogLineColumnizer columnizer;
        private readonly FilterParams filterParams;

        #endregion

        #region Ctor

        public FilterColumnChooser(FilterParams filterParams)
        {
            InitializeComponent();

            columnizer = filterParams.currentColumnizer;
            this.filterParams = filterParams;
            Init();
        }

        #endregion

        #region Private Methods

        private void Init()
        {
            int count = columnizer.GetColumnCount();
            string[] names = columnizer.GetColumnNames();
            for (int i = 0; i < count; ++i)
            {
                columnListBox.Items.Add(names[i], filterParams.columnList.Contains(i));
            }

            emptyColumnUsePrevRadioButton.Checked = filterParams.emptyColumnUsePrev;
            emptyColumnHitRadioButton.Checked = filterParams.emptyColumnHit;
            emptyColumnNoHitRadioButton.Checked =
                !filterParams.emptyColumnHit && !filterParams.emptyColumnUsePrev;
            exactMatchCheckBox.Checked = filterParams.exactColumnMatch;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            filterParams.columnList.Clear();
            foreach (int colNum in columnListBox.CheckedIndices)
            {
                filterParams.columnList.Add(colNum);
            }

            filterParams.emptyColumnUsePrev = emptyColumnUsePrevRadioButton.Checked;
            filterParams.emptyColumnHit = emptyColumnHitRadioButton.Checked;
            filterParams.exactColumnMatch = exactMatchCheckBox.Checked;
        }

        #endregion
    }
}
