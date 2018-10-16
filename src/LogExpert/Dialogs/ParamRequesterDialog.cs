using System;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class ParamRequesterDialog : Form
    {
        #region Ctor

        public ParamRequesterDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties / Indexers

        public string ParamName { get; set; }

        public string ParamValue { get; set; }

        public string[] Values { get; set; }

        #endregion

        #region Private Methods

        private void okButton_Click(object sender, EventArgs e)
        {
            ParamValue = valueComboBox.Text;
        }

        private void ParamRequesterDialog_Shown(object sender, EventArgs e)
        {
            paramLabel.Text = ParamName;
            if (Values != null)
            {
                foreach (string value in Values)
                {
                    valueComboBox.Items.Add(value);
                }

                valueComboBox.SelectedIndex = 0;
            }
        }

        #endregion
    }
}
