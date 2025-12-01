using System.Runtime.Versioning;

using ColumnizerLib;

using LogExpert.Core.Classes.Filter;

namespace LogExpert.UI.Dialogs;

[SupportedOSPlatform("windows")]
internal partial class FilterColumnChooser : Form
{
    #region Fields

    private readonly ILogLineColumnizer _columnizer;
    private readonly FilterParams _filterParams;

    #endregion

    #region cTor

    public FilterColumnChooser (FilterParams filterParams)
    {
        SuspendLayout();

        InitializeComponent();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ApplyResources();

        columnListBox.ItemHeight = columnListBox.Font.Height;

        _columnizer = filterParams.CurrentColumnizer;
        _filterParams = filterParams;

        ResumeLayout();

        Init();
    }

    private void ApplyResources ()
    {
        Text = Resources.FilterColumnChooser_UI_Title;

        groupBox1.Text = Resources.FilterColumnChooser_UI_GroupBox_OnEmptyColumns;

        checkBoxExactMatch.Text = Resources.FilterColumnChooser_UI_CheckBox_ExactMatch;

        emptyColumnNoHitRadioButton.Text = Resources.FilterColumnChooser_UI_RadioButton_NoHit;
        emptyColumnHitRadioButton.Text = Resources.FilterColumnChooser_UI_RadioButton_SearchHit;
        emptyColumnUsePrevRadioButton.Text = Resources.FilterColumnChooser_UI_RadioButton_UsePrevContent;
        buttonOk.Text = Resources.LogExpert_Common_UI_Button_OK;
        buttonCancel.Text = Resources.LogExpert_Common_UI_Button_Cancel;

        toolTipListBox.ToolTipTitle = Resources.FilterColumnChooser_UI_ToolTip_Title_Columns;
        toolTipListBox.SetToolTip(columnListBox, Resources.FilterColumnChooser_UI_ToolTip_ColumnListBox);
        toolTipEmptyColumnNoHit.SetToolTip(emptyColumnNoHitRadioButton, Resources.FilterColumnChooser_UI_ToolTip_NoHit);
        toolTipSearchHit.SetToolTip(emptyColumnHitRadioButton, Resources.FilterColumnChooser_UI_ToolTip_SearchHit);
        toolTipPrevContent.SetToolTip(emptyColumnUsePrevRadioButton, Resources.FilterColumnChooser_UI_ToolTip_UsePrevContent);
        toolTipExactMatch.SetToolTip(checkBoxExactMatch, Resources.FilterColumnChooser_UI_ToolTip_ExactMatch);
    }

    #endregion

    #region Private Methods

    private void Init ()
    {
        var count = _columnizer.GetColumnCount();
        var names = _columnizer.GetColumnNames();

        for (var i = 0; i < count; ++i)
        {
            _ = columnListBox.Items.Add(names[i], _filterParams.ColumnList.Contains(i));
        }

        emptyColumnUsePrevRadioButton.Checked = _filterParams.EmptyColumnUsePrev;
        emptyColumnHitRadioButton.Checked = _filterParams.EmptyColumnHit;
        emptyColumnNoHitRadioButton.Checked = !_filterParams.EmptyColumnHit && !_filterParams.EmptyColumnUsePrev;
        checkBoxExactMatch.Checked = _filterParams.ExactColumnMatch;
    }

    #endregion

    #region Events handler

    private void OnOkButtonClick (object sender, EventArgs e)
    {
        _filterParams.ColumnList.Clear();

        foreach (int colNum in columnListBox.CheckedIndices)
        {
            _filterParams.ColumnList.Add(colNum);
        }

        _filterParams.EmptyColumnUsePrev = emptyColumnUsePrevRadioButton.Checked;
        _filterParams.EmptyColumnHit = emptyColumnHitRadioButton.Checked;
        _filterParams.ExactColumnMatch = checkBoxExactMatch.Checked;
    }

    #endregion
}