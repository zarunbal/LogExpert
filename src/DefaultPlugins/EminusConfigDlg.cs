using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogExpert
{
    public partial class EminusConfigDlg : Form
    {
        #region Fields

        #endregion

        #region cTor

        public EminusConfigDlg(EminusConfig config)
        {
            InitializeComponent();
            TopLevel = false;
            Config = config;

            hostTextBox.Text = config.host;
            portTextBox.Text = "" + config.port;
            passwordTextBox.Text = config.password;
        }

        #endregion

        #region Properties

        public EminusConfig Config { get; set; }

        #endregion

        #region Public methods

        public void ApplyChanges()
        {
            Config.host = hostTextBox.Text;
            try
            {
                Config.port = short.Parse(portTextBox.Text);
            }
            catch (FormatException)
            {
                Config.port = 0;
            }
            Config.password = passwordTextBox.Text;
        }

        #endregion
    }
}