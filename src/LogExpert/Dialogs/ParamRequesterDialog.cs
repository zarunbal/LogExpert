using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert.Dialogs
{
    public partial class ParamRequesterDialog : Form
    {
        #region Fields

        #endregion

        #region cTor

        public ParamRequesterDialog()
        {
            InitializeComponent();

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        #endregion

        #region Properties

        public string ParamName { get; set; }

        public string ParamValue { get; set; }

        public string[] Values { get; set; }

        #endregion

        #region Events handler

        private void ParamRequesterDialog_Shown(object sender, EventArgs e)
        {
            labelValueForParameter.Text = ParamName;

            if (Values != null)
            {
                foreach (string value in Values)
                {
                    comboBoxValue.Items.Add(value);
                }
                comboBoxValue.SelectedIndex = 0;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            ParamValue = comboBoxValue.Text;
        }

        #endregion
    }
}