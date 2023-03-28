using System;
using System.Windows.Forms;

namespace CsvColumnizer
{
    public partial class CsvColumnizerConfigDlg : Form
    {
        #region Fields

        private readonly CsvColumnizerConfig _config;

        #endregion

        #region cTor

        public CsvColumnizerConfigDlg(CsvColumnizerConfig config)
        {
            _config = config;
            InitializeComponent();
            FillValues();
        }

        #endregion

        #region Private Methods

        private void FillValues()
        {
            delimiterTextBox.Text = _config.DelimiterChar;
            quoteCharTextBox.Text = _config.QuoteChar.ToString();
            escapeCharTextBox.Text = _config.EscapeChar.ToString();
            escapeCheckBox.Checked = _config.EscapeChar != '\0';
            commentCharTextBox.Text = _config.CommentChar.ToString();
            fieldNamesCheckBox.Checked = _config.HasFieldNames;
            escapeCharTextBox.Enabled = escapeCheckBox.Checked;
            minColumnsNumericUpDown.Value = _config.MinColumns;
        }

        private void RetrieveValues()
        {
            _config.DelimiterChar = delimiterTextBox.Text;
            _config.QuoteChar = quoteCharTextBox.Text[0];
            _config.EscapeChar = escapeCheckBox.Checked ? escapeCharTextBox.Text[0] : '\0';
            _config.CommentChar = commentCharTextBox.Text[0];
            _config.HasFieldNames = fieldNamesCheckBox.Checked;
            _config.MinColumns = (int) minColumnsNumericUpDown.Value;
        }

        #endregion

        #region Events handler

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            RetrieveValues();
        }

        private void OnEscapeCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            escapeCharTextBox.Enabled = escapeCheckBox.Checked;
        }

        #endregion
    }
}