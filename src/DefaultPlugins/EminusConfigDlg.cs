using System;
using System.Windows.Forms;

namespace LogExpert
{
    public partial class EminusConfigDlg : Form
    {
        #region Ctor

        public EminusConfigDlg(EminusConfig config)
        {
            InitializeComponent();
            TopLevel = false;
            Config = config;

            hostTextBox.Text = config.host;
            portTextBox.Text = string.Empty + config.port;
            passwordTextBox.Text = config.password;
        }

        #endregion

        #region Properties / Indexers

        public EminusConfig Config { get; set; }

        #endregion

        #region Public Methods

        public void ApplyChanges()
        {
            Config.host = hostTextBox.Text;
            try
            {
                Config.port = short.Parse(portTextBox.Text);
            }
            catch (FormatException fe)
            {
                Config.port = 0;
            }

            Config.password = passwordTextBox.Text;
        }

        #endregion
    }
}
