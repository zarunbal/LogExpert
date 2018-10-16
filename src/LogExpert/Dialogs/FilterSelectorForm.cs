using System;
using System.Collections.Generic;
using System.Windows.Forms;

// using System.Linq;
namespace LogExpert
{
    public partial class FilterSelectorForm : Form
    {
        #region Private Fields

        private readonly ILogLineColumnizerCallback callback;
        private readonly IList<ILogLineColumnizer> columnizerList;

        #endregion

        #region Ctor

        public FilterSelectorForm(IList<ILogLineColumnizer> existingColumnizerList,
                                  ILogLineColumnizer currentColumnizer, ILogLineColumnizerCallback callback)
        {
            SelectedColumnizer = currentColumnizer;
            this.callback = callback;
            InitializeComponent();
            filterComboBox.SelectedIndexChanged += filterComboBox_SelectedIndexChanged;

            // for the currently selected columnizer use the current instance and not the template instance from
            // columnizer registry. This ensures that changes made in columnizer config dialogs
            // will apply to the current instance
            columnizerList = new List<ILogLineColumnizer>();
            foreach (ILogLineColumnizer col in existingColumnizerList)
            {
                if (col.GetType().Equals(SelectedColumnizer.GetType()))
                {
                    columnizerList.Add(SelectedColumnizer);
                }
                else
                {
                    columnizerList.Add(col);
                }
            }

            foreach (ILogLineColumnizer col in columnizerList)
            {
                filterComboBox.Items.Add(col);
            }

            foreach (ILogLineColumnizer columnizer in columnizerList)
            {
                if (columnizer.GetType().Equals(SelectedColumnizer.GetType()))
                {
                    filterComboBox.SelectedItem = columnizer;
                    break;
                }
            }
        }

        #endregion

        #region Properties / Indexers

        public bool ApplyToAll => applyToAllCheckBox.Checked;

        public bool IsConfigPressed { get; private set; }

        public ILogLineColumnizer SelectedColumnizer { get; private set; }

        #endregion

        #region Private Methods

        private void configButton_Click(object sender, EventArgs e)
        {
            if (SelectedColumnizer is IColumnizerConfigurator)
            {
                ((IColumnizerConfigurator)SelectedColumnizer).Configure(callback, ConfigManager.ConfigDir);
                IsConfigPressed = true;
            }
        }

        private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ILogLineColumnizer col = columnizerList[filterComboBox.SelectedIndex];
            SelectedColumnizer = col;
            string description = col.GetDescription();
            description += "\r\nSupports timeshift: " +
                           (SelectedColumnizer.IsTimeshiftImplemented() ? "Yes" : "No");
            commentTextBox.Text = description;
            configButton.Enabled = SelectedColumnizer is IColumnizerConfigurator;
        }

        #endregion
    }
}
