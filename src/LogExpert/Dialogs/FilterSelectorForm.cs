using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
//using System.Linq;
using System.Windows.Forms;

namespace LogExpert
{
    public partial class FilterSelectorForm : Form
    {
        #region Fields

        private readonly ILogLineColumnizerCallback callback = null;
        private readonly IList<ILogLineColumnizer> columnizerList;

        #endregion

        #region cTor

        public FilterSelectorForm(IList<ILogLineColumnizer> existingColumnizerList,
            ILogLineColumnizer currentColumnizer, ILogLineColumnizerCallback callback)
        {
            this.SelectedColumnizer = currentColumnizer;
            this.callback = callback;
            InitializeComponent();
            this.filterComboBox.SelectedIndexChanged += new EventHandler(filterComboBox_SelectedIndexChanged);

            // for the currently selected columnizer use the current instance and not the template instance from
            // columnizer registry. This ensures that changes made in columnizer config dialogs
            // will apply to the current instance
            this.columnizerList = new List<ILogLineColumnizer>();
            foreach (ILogLineColumnizer col in existingColumnizerList)
            {
                if (col.GetType().Equals(this.SelectedColumnizer.GetType()))
                {
                    this.columnizerList.Add(this.SelectedColumnizer);
                }
                else
                {
                    this.columnizerList.Add(col);
                }
            }
            foreach (ILogLineColumnizer col in this.columnizerList)
            {
                this.filterComboBox.Items.Add(col);
            }

            foreach (ILogLineColumnizer columnizer in this.columnizerList)
            {
                if (columnizer.GetType().Equals(this.SelectedColumnizer.GetType()))
                {
                    this.filterComboBox.SelectedItem = columnizer;
                    break;
                }
            }
        }

        #endregion

        #region Properties

        public ILogLineColumnizer SelectedColumnizer { get; private set; } = null;

        public bool ApplyToAll
        {
            get { return this.applyToAllCheckBox.Checked; }
        }

        public bool IsConfigPressed { get; private set; } = false;

        #endregion

        #region Events handler

        private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ILogLineColumnizer col = columnizerList[this.filterComboBox.SelectedIndex];
            this.SelectedColumnizer = col;
            string description = col.GetDescription();
            description += "\r\nSupports timeshift: " +
                           (this.SelectedColumnizer.IsTimeshiftImplemented() ? "Yes" : "No");
            this.commentTextBox.Text = description;
            this.configButton.Enabled = this.SelectedColumnizer is IColumnizerConfigurator;
        }


        private void configButton_Click(object sender, EventArgs e)
        {
            if (this.SelectedColumnizer is IColumnizerConfigurator)
            {
                ((IColumnizerConfigurator) this.SelectedColumnizer).Configure(this.callback, ConfigManager.ConfigDir);
                this.IsConfigPressed = true;
            }
        }

        #endregion
    }
}