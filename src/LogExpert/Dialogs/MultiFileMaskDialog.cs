using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class MultiFileMaskDialog : Form
    {
        #region Ctor

        public MultiFileMaskDialog(Form parent, string fileName)
        {
            InitializeComponent();
            syntaxHelpLabel.Text = string.Empty +
                                   "Pattern syntax:\n\n" +
                                   "* = any characters (wildcard)\n" +
                                   "$D(<date>) = Date pattern\n" +
                                   "$I = File index number\n" +
                                   "$J = File index number, hidden when zero\n" +
                                   "$J(<prefix>) = Like $J, but adding <prefix> when non-zero\n" +
                                   "\n" +
                                   "<date>:\n" +
                                   "DD = day\n" +
                                   "MM = month\n" +
                                   "YY[YY] = year\n" +
                                   "all other chars will be used as given"
                ;
            fileNameLabel.Text = fileName;
        }

        #endregion

        #region Properties / Indexers

        public string FileNamePattern { get; set; }

        public int MaxDays { get; set; }

        #endregion

        #region Private Methods

        private void buttonOK_Click(object sender, EventArgs e)
        {
            FileNamePattern = fileNamePatternTextBox.Text;
            MaxDays = (int)maxDaysUpDown.Value;
        }

        private void MultiFileMaskDialog_Load(object sender, EventArgs e)
        {
            fileNamePatternTextBox.Text = FileNamePattern;
            maxDaysUpDown.Value = MaxDays;
        }

        #endregion
    }
}
