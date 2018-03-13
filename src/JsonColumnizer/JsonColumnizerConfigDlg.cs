using System;
using System.Windows.Forms;

namespace JsonColumnizer
{
    public partial class JsonColumnizerConfigDlg : Form
    {
        private readonly JsonColumnizerConfig _config;

        public JsonColumnizerConfigDlg(JsonColumnizerConfig config)
        {
            _config = config;
            InitializeComponent();
            fillValues();
        }

        private void fillValues()
        {
        }

        private void retrieveValues()
        {
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            retrieveValues();
        }
    }
}