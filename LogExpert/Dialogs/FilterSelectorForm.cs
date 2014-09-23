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
		IList<ILogLineColumnizer> columnizerList;
		ILogLineColumnizer selectedColumnizer = null;
		ILogLineColumnizerCallback callback = null;
		private bool isConfigPressed = false;

		public FilterSelectorForm(IList<ILogLineColumnizer> existingColumnizerList, ILogLineColumnizer currentColumnizer, ILogLineColumnizerCallback callback)
		{
			this.selectedColumnizer = currentColumnizer;
			this.callback = callback;
			InitializeComponent();
			this.filterComboBox.SelectedIndexChanged += new EventHandler(filterComboBox_SelectedIndexChanged);

			// for the currently selected columnizer use the current instance and not the template instance from
			// columnizer registry. This ensures that changes made in columnizer config dialogs
			// will apply to the current instance
			this.columnizerList = new List<ILogLineColumnizer>();
			foreach (ILogLineColumnizer col in existingColumnizerList)
			{
				if (col.GetType().Equals(this.selectedColumnizer.GetType()))
				{
					this.columnizerList.Add(this.selectedColumnizer);
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
				if (columnizer.GetType().Equals(this.selectedColumnizer.GetType()))
				{
					this.filterComboBox.SelectedItem = columnizer;
					break;
				}
			}
		}

		void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ILogLineColumnizer col = columnizerList[this.filterComboBox.SelectedIndex];
			this.selectedColumnizer = col;
			string description = col.GetDescription();
			description += "\r\nSupports timeshift: " + (this.selectedColumnizer.IsTimeshiftImplemented() ? "Yes" : "No");
			this.commentTextBox.Text = description;
			this.configButton.Enabled = (this.selectedColumnizer is IColumnizerConfigurator);
		}

		public ILogLineColumnizer SelectedColumnizer
		{
			get
			{
				return this.selectedColumnizer;
			}
		}

		public bool ApplyToAll
		{
			get
			{
				return this.applyToAllCheckBox.Checked;
			}
		}

		public bool IsConfigPressed
		{
			get
			{
				return this.isConfigPressed;
			}
		}

		private void configButton_Click(object sender, EventArgs e)
		{
			if (this.selectedColumnizer is IColumnizerConfigurator)
			{
				((IColumnizerConfigurator)this.selectedColumnizer).Configure(this.callback, ConfigManager.ConfigDir);
				this.isConfigPressed = true;
			}
		}
	}
}