using System.Data;
using System.Text.RegularExpressions;

namespace RegexColumnizer;

public partial class RegexColumnizerConfigDialog : Form
{
    public RegexColumnizerConfigDialog ()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        ApplyResources();

        ResumeLayout();
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

    public RegexColumnizerConfig Config { get; set; }

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
        Check();
    }

    private bool Check ()
    {
        DataTable table = new();

        try
        {
            Regex regex = new(tbExpression.Text);
            var groupNames = regex.GetGroupNames();
            var offset = groupNames.Length > 1 ? 1 : 0;

            for (var i = offset; i < groupNames.Length; i++)
            {
                table.Columns.Add(groupNames[i]);
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
