using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RegexColumnizer
{
	public partial class RegexColumnizerConfigDialog : Form
	{
		public RegexColumnizerConfigDialog()
		{
			InitializeComponent();
		}

		public RegexColumnizerConfig Config { get; set; }

		private void btnOk_Click(object sender, EventArgs e)
		{
			if (Check())
			{
				Config.Expression = tbExpression.Text;
                Config.Name = tbName.Text;
            }

		}

		private void RegexColumnizerConfigDialog_Load(object sender, EventArgs e)
		{
			tbExpression.Text = Config.Expression;
            tbName.Text = Config.Name;
        }

		private void tbCheck_Click(object sender, EventArgs e)
		{
			Check();
		}

		private bool Check()
		{
			DataTable table = new DataTable();
			try
			{
				Regex regex = new Regex(tbExpression.Text);
				var groupNames = regex.GetGroupNames();
				int offset = groupNames.Length > 1 ? 1 : 0;
				for (int i = offset; i < groupNames.Length; i++)
				{
					table.Columns.Add(groupNames[i]);
				}

				if (!string.IsNullOrEmpty(tbTestLine.Text))
				{
					Match match = regex.Match(tbTestLine.Text);
					var row = table.NewRow();
					var values = match.Groups.OfType<Group>().Skip(offset).Select(group => group.Value).Cast<object>().ToArray();
					row.ItemArray = values;
					table.Rows.Add(row);
				}

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Invalid Regex !{Environment.NewLine}{ex.Message}", @"Regex Columnizer Configuration",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			finally
			{
				dataGridView1.DataSource = table;
			}
		}
	}
}
