using System.Data;
using System.Text.RegularExpressions;

using LogExpert.Core.Helpers;

namespace RegexColumnizer;

public partial class RegexColumnizerConfigDialog : Form
{
    public RegexColumnizerConfigDialog (RegexColumnizerConfig config)
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        ResumeLayout();

        Config = config;
    }

    private void ApplyResources ()
    {
        Text = Resources.RegexColumnizerConfigDialog_UI_Title;
        lblRegEx.Text = Resources.RegexColumnizerConfigDialog_UI_Label_Regex;
        label2.Text = Resources.RegexColumnizerConfigDialog_UI_Label_Name;
        gbTestZone.Text = Resources.RegexColumnizerConfigDialog_UI_GroupBox_TestZone;
        label1.Text = Resources.RegexColumnizerConfigDialog_UI_Label_Line;
        tbCheck.Text = Resources.RegexColumnizerConfigDialog_UI_Button_Check;
        btnOk.Text = Resources.RegexColumnizerConfigDialog_UI_Button_OK;
        btnCancel.Text = Resources.RegexColumnizerConfigDialog_UI_Button_Cancel;
    }

    private void OnBtnOkClick (object sender, EventArgs e)
    {
        if (Check())
        {
            Config.Expression = tbExpression.Text;
            Config.Name = tbName.Text;
        }

    }

    private void RegexColumnizerConfigDialog_Load (object sender, EventArgs e)
    {
        tbExpression.Text = Config.Expression;
        tbName.Text = Config.Name;
    }

    private void OnButtonCheckClick (object sender, EventArgs e)
    {
        _ = Check();
    }

    public RegexColumnizerConfig Config { get; }

    private bool Check ()
    {
        DataTable table = new();

        try
        {
            // Use RegexHelper for safe regex creation with timeout protection
            Regex regex = RegexHelper.CreateSafeRegex(tbExpression.Text);
            var groupNames = regex.GetGroupNames();
            var offset = groupNames.Length > 1 ? 1 : 0;

            for (var i = offset; i < groupNames.Length; i++)
            {
                _ = table.Columns.Add(groupNames[i]);
            }

            if (!string.IsNullOrEmpty(tbTestLine.Text))
            {
                var match = regex.Match(tbTestLine.Text);
                var row = table.NewRow();
                var values = match.Groups.OfType<Group>().Skip(offset).Select(group => group.Value).Cast<object>().ToArray();
                row.ItemArray = values;
                table.Rows.Add(row);
            }

            return true;
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($@"Invalid Regex !{Environment.NewLine}{ex.Message}", @"Regex Columnizer Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        finally
        {
            dataGridView1.DataSource = table;
        }
    }
}
